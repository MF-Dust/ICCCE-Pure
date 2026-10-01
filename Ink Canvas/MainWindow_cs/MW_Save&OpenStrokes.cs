using Ink_Canvas.Controls;
using Ink_Canvas.Helpers;
using Ink_Canvas.Helpers.Persistence;
using Ink_Canvas.Properties;
using Ink_Canvas.UInk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Color = System.Drawing.Color;
using File = System.IO.File;
using Image = System.Windows.Controls.Image;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Ink_Canvas
{
    public partial class MainWindow : Ink_Canvas.Helpers.PerformanceTransparentWin
    {
        /// <summary>收集画布上图片与 PDF 的元数据，写入 .elements.json（与墨迹文件同路径）。</summary>
        private void CollectCanvasElementsMetadata(List<CanvasElementInfo> elementInfos)
        {
            if (elementInfos == null || inkCanvas == null) return;

            foreach (var child in inkCanvas.Children)
            {
                if (child is Image img && img.Source is BitmapImage bmp)
                {
                    elementInfos.Add(new CanvasElementInfo
                    {
                        Type = "Image",
                        SourcePath = bmp.UriSource?.LocalPath ?? "",
                        Left = InkCanvas.GetLeft(img),
                        Top = InkCanvas.GetTop(img),
                        Width = img.Width,
                        Height = img.Height,
                        Stretch = img.Stretch.ToString()
                    });
                }
                else if (child is PdfEmbeddedView pdf && !string.IsNullOrEmpty(pdf.PdfPath))
                {
                    elementInfos.Add(new CanvasElementInfo
                    {
                        Type = "Pdf",
                        SourcePath = pdf.PdfPath,
                        Left = InkCanvas.GetLeft(pdf),
                        Top = InkCanvas.GetTop(pdf),
                        Width = pdf.Width,
                        Height = pdf.Height,
                        Stretch = "Uniform",
                        PdfCurrentPage = (int)pdf.CurrentPageIndex,
                        PdfPageCount = (int)pdf.PageCount
                    });
                }
                else if (child is CanvasMediaControl mediaControl && TryGetMediaSourcePath(mediaControl, out var mediaSourcePath))
                {
                    string extension = Path.GetExtension(mediaSourcePath);
                    var mediaPosition = mediaControl.GetPlaybackPositionOrNull();
                    elementInfos.Add(new CanvasElementInfo
                    {
                        Type = "Media",
                        SourcePath = mediaSourcePath,
                        Left = InkCanvas.GetLeft(mediaControl),
                        Top = InkCanvas.GetTop(mediaControl),
                        Width = mediaControl.Width,
                        Height = mediaControl.Height,
                        Stretch = "Uniform",
                        MediaKind = mediaControl.IsAudioOnly ? "Audio" : "Video",
                        MediaDisplayName = mediaControl.DisplayName,
                        MediaPositionSeconds = mediaPosition?.TotalSeconds,
                        MediaSpeedRatio = mediaControl.PlaybackRate,
                        MediaVolume = mediaControl.VolumeLevel
                    });
                }
                else if (child is MediaElement media && media.Source != null)
                {
                    string sourcePath = media.Source.IsFile ? media.Source.LocalPath : media.Source.OriginalString;
                    if (!string.IsNullOrEmpty(sourcePath))
                    {
                        string extension = Path.GetExtension(sourcePath);
                        elementInfos.Add(new CanvasElementInfo
                        {
                            Type = "Media",
                            SourcePath = sourcePath,
                            Left = InkCanvas.GetLeft(media),
                            Top = InkCanvas.GetTop(media),
                            Width = media.Width,
                            Height = media.Height,
                            Stretch = media.Stretch.ToString(),
                            MediaKind = string.Equals(extension, ".mp3", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(extension, ".m4a", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(extension, ".aac", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(extension, ".flac", StringComparison.OrdinalIgnoreCase) ? "Audio" : "Video"
                        });
                    }
                }
            }
        }

        private void RestoreMediaFromElementInfo(CanvasElementInfo info)
        {
            if (info == null || inkCanvas == null) return;
            if (!string.Equals(info.Type, "Media", StringComparison.OrdinalIgnoreCase)) return;
            if (string.IsNullOrEmpty(info.SourcePath) || !File.Exists(info.SourcePath)) return;

            try
            {
                var mediaControl = new CanvasMediaControl
                {
                    Name = "media_" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss_fff"),
                    Width = info.Width > 0 && !double.IsNaN(info.Width) ? info.Width : 800,
                    Height = info.Height > 0 && !double.IsNaN(info.Height) ? info.Height : (string.Equals(info.MediaKind, "Audio", StringComparison.OrdinalIgnoreCase) ? 168 : 520),
                    ToolTip = string.IsNullOrWhiteSpace(info.MediaDisplayName) ? Path.GetFileName(info.SourcePath) : info.MediaDisplayName
                };
                mediaControl.Initialize(info.SourcePath, info.MediaDisplayName);
                if (info.MediaSpeedRatio.HasValue)
                {
                    mediaControl.SetPlaybackRate(info.MediaSpeedRatio.Value);
                }
                if (info.MediaVolume.HasValue)
                {
                    mediaControl.SetVolumeLevel(info.MediaVolume.Value);
                }
                if (info.MediaPositionSeconds.HasValue)
                {
                    mediaControl.SetPlaybackPosition(TimeSpan.FromSeconds(Math.Max(0, info.MediaPositionSeconds.Value)));
                }

                AttachMediaFailureNotification(mediaControl);
                InkCanvas.SetLeft(mediaControl, info.Left);
                InkCanvas.SetTop(mediaControl, info.Top);
                InitializeElementTransform(mediaControl);
                BindElementEvents(mediaControl);
                inkCanvas.Children.Add(mediaControl);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"从 .elements.json 恢复媒体失败: {ex.Message}", LogHelper.LogType.Error);
            }
        }

        /// <summary>
        /// 保存墨迹的鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        /// <remarks>
        /// 该方法会：
        /// 1. 检查是否是当前按下的对象，且墨迹画布是否可见
        /// 2. 隐藏工具面板
        /// 3. 隐藏通知面板
        /// 4. 调用SaveInkCanvasStrokes方法保存墨迹
        /// </remarks>
        private async void SymbolIconSaveStrokes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject != sender || inkCanvas.Visibility != Visibility.Visible) return;

            AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);

            GridNotifications.Visibility = Visibility.Collapsed;

            await SaveInkCanvasStrokesAsync(true, true);
        }

        private SaveCoordinator _saveCoordinator;
        private SaveCoordinator Saves => _saveCoordinator ??= new SaveCoordinator();
        private bool _saveClosePending;
        private bool _saveCloseReady;

        private sealed class SavePages
        {
            public readonly List<StrokeCollection> Strokes = new List<StrokeCollection>();
            public bool IsPpt;
            public bool IsMultiple;
            public int CurrentPage;
        }

        // All WPF/history/PPT reads happen here, on the UI thread, once per save.
        private SavePages CaptureSavePages()
        {
            var pages = new SavePages
            {
                IsPpt = IsInPPTPresentationMode && _pptManager?.IsConnected == true,
                CurrentPage = CurrentWhiteboardIndex
            };
            if (pages.IsPpt)
            {
                int total = _pptManager.SlidesCount;
                pages.CurrentPage = _pptManager.GetCurrentSlideNumber();
                pages.IsMultiple = total > 0; // Legacy PPT export uses Page-N even for one slide.
                for (int i = 1; i <= total; i++) pages.Strokes.Add(GetPptStrokesForSave(i, pages.CurrentPage));
            }
            else if (currentMode != 0 && WhiteboardTotalCount > 1)
            {
                pages.IsMultiple = true;
                for (int i = 1; i <= WhiteboardTotalCount; i++) pages.Strokes.Add(GetWhiteboardStrokesForSave(i));
            }
            if (pages.Strokes.Count == 0) pages.Strokes.Add(inkCanvas.Strokes.Clone());
            return pages;
        }

        private StrokeCollection GetWhiteboardStrokesForSave(int page)
        {
            return page == CurrentWhiteboardIndex
                ? inkCanvas.Strokes.Clone()
                : TimeMachineHistories[page] != null
                    ? ApplyHistoriesToNewStrokeCollection(TimeMachineHistories[page]).Clone()
                    : new StrokeCollection();
        }

        private StrokeCollection GetPptStrokesForSave(int slide, int currentSlide)
        {
            return slide == currentSlide
                ? inkCanvas.Strokes.Clone()
                : _singlePPTInkManager?.LoadSlideStrokes(slide)?.Clone() ?? new StrokeCollection();
        }

        // Completion is intentionally synchronous for screenshot/automation callers.
        public void SaveInkCanvasStrokes(bool newNotice = true, bool saveByUser = false)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SaveInkCanvasStrokes(newNotice, saveByUser));
                return;
            }
            try
            {
                SaveSnapshot snapshot = null;
                var task = Saves.Submit(() =>
                {
                    snapshot = CaptureSaveSnapshot(saveByUser);
                    return () => { using (snapshot) snapshot.Write(); };
                }, automatic: false);
                if (task == null) return;
                // Writers have no UI continuations: waiting here cannot deadlock the Dispatcher.
                task.GetAwaiter().GetResult();
                if (newNotice) ShowNotification(snapshot.SuccessMessage);
            }
            catch (Exception ex) { ReportSaveFailure(ex); }
        }

        internal async Task SaveInkCanvasStrokesAsync(bool newNotice = true, bool saveByUser = false, bool automatic = false)
        {
            if (!Dispatcher.CheckAccess())
            {
                await Dispatcher.InvokeAsync(() => SaveInkCanvasStrokesAsync(newNotice, saveByUser, automatic)).Task.Unwrap();
                return;
            }
            try
            {
                SaveSnapshot snapshot = null;
                var task = Saves.Submit(() =>
                {
                    snapshot = CaptureSaveSnapshot(saveByUser);
                    return () => { using (snapshot) snapshot.Write(); };
                }, automatic);
                if (task == null) return;
                await task;
                if (newNotice && !_saveClosePending && !_saveCloseReady) ShowNotification(snapshot.SuccessMessage);
            }
            catch (Exception ex) { ReportSaveFailure(ex); }
        }

        private void ReportSaveFailure(Exception ex)
        {
            if (!_saveCloseReady) ShowNotification(MainWindowStrings.Main_Strokes_SaveFailed);
            LogHelper.WriteLogToFile("墨迹保存失败 | " + ex, LogHelper.LogType.Error);
        }

        private SaveSnapshot CaptureSaveSnapshot(bool saveByUser)
        {
            inkCanvas.Dispatcher.VerifyAccess();
            string directory = Settings.Automation.AutoSavedStrokesLocation
                + (saveByUser ? @"\User Saved - " : @"\Auto Saved - ")
                + (currentMode == 0 ? "Annotation Strokes" : "BlackBoard Strokes");
            string filename;
            if (Settings.Automation.IsUseCustomSaveFileName)
            {
                filename = SaveFileNameHelper.Render(Settings.Automation.CustomSaveFileNameTemplate, new SaveFileNameContext
                {
                    Mode = currentMode == 0 ? "Annotation" : "BlackBoard",
                    Type = saveByUser ? "User" : "Auto",
                    Page = currentMode != 0 ? CurrentWhiteboardIndex : null,
                    Count = inkCanvas.Strokes.Count
                });
            }
            else
            {
                filename = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff");
                if (currentMode != 0) filename += " Page-" + CurrentWhiteboardIndex + " StrokesCount-" + inkCanvas.Strokes.Count;
            }
            string path = Path.Combine(directory, filename + ".icstk");
            var pages = CaptureSavePages();
            if (Settings.Automation.IsSaveStrokesAsUInK)
                return CaptureUInkSnapshot(Path.ChangeExtension(path, ".uink"), pages);

            var snapshot = new SaveSnapshot { Path = path };
            bool xml = Settings.Automation.IsSaveStrokesAsXML;
            bool full = !xml && Settings.Automation.IsSaveFullPageStrokes;
            var elements = new List<CanvasElementInfo>();
            CollectCanvasElementsMetadata(elements);
            byte[] elementsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(elements, Newtonsoft.Json.Formatting.Indented));
            bool zip = pages.IsMultiple && (full || (xml && !pages.IsPpt));
            snapshot.IsZip = zip;
            if (zip)
            {
                snapshot.Path = Path.ChangeExtension(path, ".zip");
                for (int i = 0; i < pages.Strokes.Count; i++)
                {
                    var strokes = pages.Strokes[i];
                    if (strokes.Count == 0) continue; // Empty pages are represented by metadata, as before.
                    string entry = $"page_{i + 1:D4}";
                    snapshot.Files.Add((entry + (xml ? ".xml" : ".icstk"), SerializeStrokes(strokes, xml)));
                    if (xml) snapshot.Files.Add((entry + ".elements.json", elementsBytes));
                    else
                    {
                        using var image = new MemoryStream();
                        SavePageAsImage(strokes, image);
                        snapshot.Files.Add((entry + ".png", image.ToArray()));
                    }
                }
                snapshot.Files.Add(("metadata.txt", CaptureSaveMetadata(pages, xml)));
                snapshot.SuccessMessage = string.Format(xml ? MainWindowStrings.Main_Strokes_SaveMultiPageXmlZipSuccess
                    : MainWindowStrings.Main_Strokes_SaveMultiPageZipSuccess, snapshot.Path);
            }
            else if (pages.IsMultiple)
            {
                int saved = 0;
                for (int i = 0; i < pages.Strokes.Count; i++)
                {
                    if (pages.Strokes[i].Count == 0) continue;
                    string pagePath = Path.Combine(directory, $"{filename}_Page-{i + 1}" + (xml ? ".xml" : ".icstk"));
                    snapshot.Files.Add((pagePath, SerializeStrokes(pages.Strokes[i], xml)));
                    if (xml) snapshot.Files.Add((Path.ChangeExtension(pagePath, ".elements.json"), elementsBytes));
                    saved++;
                }
                snapshot.SuccessMessage = string.Format(xml ? MainWindowStrings.Main_Strokes_SaveMultiPageXmlSuccess
                    : MainWindowStrings.Main_Strokes_SaveMultiPageIcstkSuccess, xml ? saved : pages.Strokes.Count);
            }
            else
            {
                string strokePath = xml ? Path.ChangeExtension(path, ".xml") : path;
                snapshot.Files.Add((strokePath, SerializeStrokes(pages.Strokes[0], xml)));
                if (full)
                {
                    string imagePath = Path.ChangeExtension(path, ".png");
                    snapshot.Files.Add((imagePath, CaptureSinglePageImage()));
                    snapshot.SuccessMessage = string.Format(MainWindowStrings.Main_Strokes_SaveFullPageSuccess, imagePath);
                }
                else
                {
                    snapshot.Files.Add((Path.ChangeExtension(path, ".elements.json"), elementsBytes));
                    snapshot.SuccessMessage = string.Format(xml ? MainWindowStrings.Main_Strokes_SaveXmlSuccess
                        : MainWindowStrings.Main_Strokes_SaveSuccess, strokePath);
                }
            }
            return snapshot;
        }

        private byte[] CaptureSaveMetadata(SavePages pages, bool xml)
        {
            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true))
            {
                writer.WriteLine($"保存时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"总页数: {pages.Strokes.Count}");
                writer.WriteLine($"模式: {(currentMode == 0 ? "PPT放映" : FloatingBarStrings.FloatingBar_Whiteboard)}");
                if (xml) writer.WriteLine("格式: XML");
                if (currentMode != 0)
                {
                    writer.WriteLine($"当前页面: {CurrentWhiteboardIndex}");
                    writer.WriteLine($"总页面数: {WhiteboardTotalCount}");
                }
                else if (_pptManager?.PPTApplication != null)
                {
                    // Read COM while capturing, never from a worker thread.
                    dynamic application = _pptManager.PPTApplication;
                    var presentation = application.SlideShowWindows[1].Presentation;
                    writer.WriteLine($"PPT名称: {presentation.Name}");
                    writer.WriteLine($"PPT总页数: {presentation.Slides.Count}");
                    writer.WriteLine($"PPT文件路径: {presentation.FullName}");
                }
                for (int i = 0; i < pages.Strokes.Count; i++)
                    writer.WriteLine($"页面 {i + 1}: {pages.Strokes[i].Count} 条墨迹");
            }
            return stream.ToArray();
        }

        private byte[] SerializeStrokes(StrokeCollection strokes, bool xml)
        {
            using var stream = new MemoryStream();
            if (!xml) strokes.Save(stream);
            else
            {
                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("InkCanvasStrokes", new XAttribute("Version", "1.0"),
                        new XAttribute("StrokeCount", strokes.Count),
                        new XAttribute("SaveTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                        from stroke in strokes
                        select new XElement("Stroke", new XAttribute("DrawingAttributes", SerializeDrawingAttributes(stroke.DrawingAttributes)),
                            new XElement("StylusPoints", from point in stroke.StylusPoints
                                select new XElement("StylusPoint", new XAttribute("X", point.X),
                                    new XAttribute("Y", point.Y), new XAttribute("PressureFactor", point.PressureFactor))))));
                using var writer = new XmlTextWriter(stream, Encoding.UTF8) { Formatting = System.Xml.Formatting.Indented };
                doc.Save(writer);
                writer.Flush();
                return stream.ToArray();
            }
            return stream.ToArray();
        }

        private string SerializeDrawingAttributes(DrawingAttributes da)
        {
            return $"Color={da.Color};Width={da.Width};Height={da.Height};FitToCurve={da.FitToCurve};" +
                $"IsHighlighter={da.IsHighlighter};IgnorePressure={da.IgnorePressure};StylusTip={da.StylusTip};";
        }

        // Rendering and PNG encoding remain on the UI thread; only the resulting bytes leave it.
        private byte[] CaptureSinglePageImage()
        {
            using var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Settings.Canvas.UsingWhiteboard ? Color.White : Color.FromArgb(22, 41, 36));
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
                dc.DrawRectangle(new VisualBrush(inkCanvas), null, new Rect(0, 0, inkCanvas.ActualWidth, inkCanvas.ActualHeight));
            var rtb = new RenderTargetBitmap((int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var rendered = new MemoryStream();
            encoder.Save(rendered);
            rendered.Position = 0;
            using var image = new Bitmap(rendered);
            graphics.DrawImage(image, (bitmap.Width - image.Width) / 2, (bitmap.Height - image.Height) / 2);
            using var result = new MemoryStream();
            bitmap.Save(result, ImageFormat.Png);
            return result.ToArray();
        }

        /// <summary>
        /// 将指定墨迹集合保存为图像到指定流
        /// </summary>
        private void SavePageAsImage(StrokeCollection strokes, Stream outputStream)
        {
            try
            {
                // 创建临时InkCanvas来渲染墨迹
                var tempCanvas = new InkCanvas();
                tempCanvas.Strokes = strokes;
                tempCanvas.Width = inkCanvas.ActualWidth;
                tempCanvas.Height = inkCanvas.ActualHeight;
                tempCanvas.Measure(new System.Windows.Size(tempCanvas.Width, tempCanvas.Height));
                tempCanvas.Arrange(new Rect(0, 0, tempCanvas.Width, tempCanvas.Height));
                tempCanvas.UpdateLayout();

                // 创建渲染位图
                var rtb = new RenderTargetBitmap(
                    (int)tempCanvas.Width, (int)tempCanvas.Height,
                    96, 96,
                    PixelFormats.Pbgra32);
                rtb.Render(tempCanvas);

                // 保存为PNG
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(outputStream);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"保存页面图像失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 打开墨迹文件的鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        /// <remarks>
        /// 该方法会：
        /// 1. 检查是否是当前按下的对象
        /// 2. 隐藏工具面板
        /// 3. 打开文件选择对话框
        /// 4. 根据文件扩展名选择不同的打开方式：
        ///    - .zip：处理ICC压缩包
        ///    - .xml：处理XML格式墨迹文件
        ///    - 其他：处理单个墨迹文件（二进制格式）
        /// 5. 如果墨迹画布不可见，切换到鼠标模式
        /// </remarks>
        private void SymbolIconOpenStrokes_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("打开墨迹文件")) return;
            if (lastBorderMouseDownObject != sender) return;
            AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);

            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Settings.Automation.AutoSavedStrokesLocation;
            openFileDialog.Title = MainWindowStrings.Main_Strokes_OpenFileDialogTitle;
            openFileDialog.Filter = MainWindowStrings.Main_Strokes_OpenFileDialogFilter;
            if (openFileDialog.ShowDialog() != true) return;
            LogHelper.WriteLogToFile($"Strokes Insert: Name: {openFileDialog.FileName}",
                LogHelper.LogType.Event);

            try
            {
                string fileExtension = Path.GetExtension(openFileDialog.FileName).ToLower();

                if (fileExtension == ".uink")
                {
                    // UInk 1.0 墨迹文件（MessagePack 对象流 + .uink.extra 资源包）
                    OpenUInkFile(openFileDialog.FileName);
                }
                else if (fileExtension == ".zip")
                {
                    // 处理ICC压缩包（可能包含XML格式）
                    OpenICCZipFile(openFileDialog.FileName);
                }
                else if (fileExtension == ".xml")
                {
                    // 处理XML格式墨迹文件
                    OpenXMLStrokeFile(openFileDialog.FileName);
                }
                else
                {
                    // 处理单个墨迹文件（二进制格式）
                    OpenSingleStrokeFile(openFileDialog.FileName);
                }

                if (inkCanvas.Visibility != Visibility.Visible) SymbolIconCursor_Click(sender, null);
            }
            catch (Exception ex)
            {
                ShowNotification(MainWindowStrings.Main_Strokes_OpenFailed);
                LogHelper.WriteLogToFile($"墨迹打开失败: {ex}", LogHelper.LogType.Error);
            }
        }

        /// <summary>
        /// 打开ICC创建的.zip压缩包
        /// </summary>
        private void OpenICCZipFile(string zipFilePath)
        {
            try
            {
                // 创建临时目录来解压文件
                string tempDir = Path.Combine(Path.GetTempPath(), $"InkCanvas_Open_{DateTime.Now:yyyyMMdd_HHmmss}");
                Directory.CreateDirectory(tempDir);

                try
                {
                    // 解压ZIP文件
                    SafeZipExtractor.ExtractZipSafely(zipFilePath, tempDir, overwrite: true);

                    // 读取元数据文件
                    string metadataFile = Path.Combine(tempDir, "metadata.txt");
                    if (!File.Exists(metadataFile))
                    {
                        throw new Exception("压缩包中未找到元数据文件");
                    }

                    var metadata = ReadMetadataFile(metadataFile);

                    // 根据元数据信息决定恢复模式
                    bool isPPTMode = metadata.ContainsKey("模式") && metadata["模式"].Contains("PPT放映");
                    bool isWhiteboardMode = metadata.ContainsKey("模式") && metadata["模式"].Contains(FloatingBarStrings.FloatingBar_Whiteboard);

                    // 检查当前是否处于PPT模式
                    bool isCurrentlyInPPTMode = IsInPPTPresentationMode && _pptManager?.PPTApplication != null;

                    // 检查当前是否处于白板模式
                    bool isCurrentlyInWhiteboardMode = currentMode != 0;

                    // 严格模式隔离：只在对应模式下恢复对应墨迹
                    if (isPPTMode && isCurrentlyInPPTMode)
                    {
                        // 只在PPT放映模式下恢复PPT墨迹
                        RestorePPTStrokesFromZip(tempDir, metadata);
                    }
                    else if (isWhiteboardMode && isCurrentlyInWhiteboardMode)
                    {
                        // 只在白板模式下恢复白板墨迹
                        RestoreWhiteboardStrokesFromZip(tempDir, metadata);
                    }
                    else
                    {
                        // 模式不匹配时，显示提示信息
                        string savedMode = isPPTMode ? "PPT放映" : (isWhiteboardMode ? FloatingBarStrings.FloatingBar_Whiteboard : "未知");
                        string currentMode = isCurrentlyInPPTMode ? "PPT放映" : (isCurrentlyInWhiteboardMode ? FloatingBarStrings.FloatingBar_Whiteboard : "桌面");
                        ShowNotification(string.Format(MainWindowStrings.Main_Strokes_ModeMismatch, savedMode, currentMode));
                        LogHelper.WriteLogToFile($"模式不匹配：保存模式={savedMode}，当前模式={currentMode}", LogHelper.LogType.Warning);
                    }

                    ShowNotification(string.Format(MainWindowStrings.Main_Strokes_OpenIccSuccess, metadata.ContainsKey("总页数") ? metadata["总页数"] : "0"));
                }
                finally
                {
                    // 清理临时目录
                    try
                    {
                        if (Directory.Exists(tempDir))
                            Directory.Delete(tempDir, true);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLogToFile($"清理临时目录失败: {ex}", LogHelper.LogType.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"打开ICC压缩包失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 读取元数据文件
        /// </summary>
        private Dictionary<string, string> ReadMetadataFile(string metadataPath)
        {
            var metadata = new Dictionary<string, string>();

            using (var reader = new StreamReader(metadataPath, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(":"))
                    {
                        var parts = line.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            metadata[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }

            return metadata;
        }

        /// <summary>
        /// 从ZIP文件恢复PPT墨迹
        /// </summary>
        private void RestorePPTStrokesFromZip(string tempDir, Dictionary<string, string> metadata)
        {
            if (TryBlockFrozenPageMutation("恢复墨迹文件")) return;
            try
            {
                // 确保当前处于PPT放映模式
                if (!IsInPPTPresentationMode || _pptManager?.PPTApplication == null)
                {
                    throw new InvalidOperationException("当前不在PPT放映模式，无法恢复PPT墨迹");
                }

                // 检查PPT文件路径是否匹配
                if (metadata.ContainsKey("PPT文件路径"))
                {
                    string savedPPTPath = metadata["PPT文件路径"];
                    dynamic application = _pptManager.PPTApplication;
                    string currentPPTPath = application.SlideShowWindows[1].Presentation.FullName;

                    if (!string.IsNullOrEmpty(savedPPTPath) && !string.IsNullOrEmpty(currentPPTPath))
                    {
                        // 使用文件路径哈希值进行比较，避免路径格式差异
                        string savedHash = HashHelper.GetFileHash(savedPPTPath);
                        string currentHash = HashHelper.GetFileHash(currentPPTPath);

                        if (savedHash != currentHash)
                        {
                            throw new InvalidOperationException($"墨迹文件与当前PPT文件不匹配。保存的PPT: {savedPPTPath}，当前PPT: {currentPPTPath}");
                        }
                    }
                }

                // 先把所有页面的墨迹全部解析进内存，再清空原画布。
                // 一旦循环中任意一页解析失败抛出，原画布墨迹不会被销毁、撤销历史可恢复。
                // TimeMachineHistories 容量固定为 101（MW_BoardControls.cs:47），对超出范围或负数页码直接跳过。
                var parsedStrokesByPage = new Dictionary<int, StrokeCollection>();
                var icstkFiles = Directory.GetFiles(tempDir, "page_*.icstk");
                var xmlFiles = Directory.GetFiles(tempDir, "page_*.xml");
                var allFiles = new List<string>();
                allFiles.AddRange(icstkFiles);
                allFiles.AddRange(xmlFiles);

                foreach (var file in allFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (!fileName.StartsWith("page_") || !int.TryParse(fileName.Substring(5), out int pageNumber))
                        continue;
                    if (pageNumber < 1 || pageNumber >= TimeMachineHistories.Length)
                    {
                        LogHelper.WriteLogToFile($"跳过非法页码 {pageNumber}（文件 {file}）", LogHelper.LogType.Warning);
                        continue;
                    }

                    StrokeCollection strokes;
                    string extension = Path.GetExtension(file).ToLower();

                    if (extension == ".xml")
                    {
                        strokes = LoadStrokesFromXML(file);
                    }
                    else
                    {
                        using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            strokes = new StrokeCollection(fs);
                        }
                    }

                    if (strokes != null && strokes.Count > 0)
                        parsedStrokesByPage[pageNumber] = strokes;
                }

                // 清空当前墨迹
                ClearStrokes(true);
                timeMachine.ClearStrokeHistory();

                // 重置PPT墨迹存储
                _singlePPTInkManager?.ClearAllStrokes();

                foreach (var pair in parsedStrokesByPage)
                    _singlePPTInkManager?.ForceSaveSlideStrokes(pair.Key, pair.Value);

                // 恢复当前页面的墨迹
                if (_pptManager?.IsInSlideShow == true)
                {
                    int currentSlide = _pptManager.GetCurrentSlideNumber();
                    var currentStrokes = _singlePPTInkManager?.LoadSlideStrokes(currentSlide);
                    if (currentStrokes != null && currentStrokes.Count > 0)
                    {
                        inkCanvas.Strokes.Add(currentStrokes);
                    }
                }

                LogHelper.WriteLogToFile($"成功恢复PPT墨迹，共{allFiles.Count}页");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"恢复PPT墨迹失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 从ZIP文件恢复白板墨迹
        /// </summary>
        private void RestoreWhiteboardStrokesFromZip(string tempDir, Dictionary<string, string> metadata)
        {
            if (TryBlockFrozenPageMutation("恢复墨迹文件")) return;
            try
            {
                // 确保当前处于白板模式
                if (currentMode == 0)
                {
                    throw new InvalidOperationException("当前不在白板模式，无法恢复白板墨迹");
                }

                // 先把全部墨迹解析进内存，循环结束后才清空原画布——解析失败时原墨迹仍存在。
                // 同时把 pageNumber 限定在 TimeMachineHistories 容量（101）内，避免 IndexOutOfRangeException。
                var parsedHistoriesByPage = new Dictionary<int, TimeMachineHistory[]>();
                var files = Directory.GetFiles(tempDir, "page_*.icstk")
                    .Concat(Directory.GetFiles(tempDir, "page_*.xml")).ToArray();
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (!fileName.StartsWith("page_") || !int.TryParse(fileName.Substring(5), out int pageNumber))
                        continue;
                    if (pageNumber < 1 || pageNumber >= TimeMachineHistories.Length)
                    {
                        LogHelper.WriteLogToFile($"跳过非法页码 {pageNumber}（文件 {file}）", LogHelper.LogType.Warning);
                        continue;
                    }

                    StrokeCollection strokes;
                    if (string.Equals(Path.GetExtension(file), ".xml", StringComparison.OrdinalIgnoreCase))
                        strokes = LoadStrokesFromXML(file);
                    else
                    {
                        using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                        strokes = new StrokeCollection(fs);
                    }
                    if (strokes.Count > 0)
                    {
                        var history = new TimeMachineHistory(strokes, TimeMachineHistoryType.UserInput, false);
                        parsedHistoriesByPage[pageNumber] = new[] { history };
                    }
                }

                // 清空当前墨迹
                ClearStrokes(true);
                timeMachine.ClearStrokeHistory();

                // 读取总页数
                int totalPages = 1;
                if (metadata.ContainsKey("总页数") && int.TryParse(metadata["总页数"], out int parsedPages))
                {
                    totalPages = parsedPages;
                }

                // 重置白板状态
                WhiteboardTotalCount = totalPages;
                CurrentWhiteboardIndex = 1;
                ResetInkFreezePageStates();

                // 清空历史记录
                for (int i = 0; i < TimeMachineHistories.Length; i++)
                {
                    TimeMachineHistories[i] = null;
                }

                foreach (var pair in parsedHistoriesByPage)
                    TimeMachineHistories[pair.Key] = pair.Value;

                // 恢复第一页的墨迹
                if (TimeMachineHistories[1] != null)
                {
                    RestoreStrokes();
                }

                // 更新UI显示
                UpdateIndexInfoDisplay();

                LogHelper.WriteLogToFile($"成功恢复白板墨迹，共{totalPages}页");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"恢复白板墨迹失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 打开XML格式的墨迹文件
        /// </summary>
        public void OpenXMLStrokeFile(string filePath)
        {
            if (TryBlockFrozenPageMutation("打开墨迹文件")) return;
            try
            {
                XDocument doc = XDocument.Load(filePath);
                var root = doc.Root;
                if (root == null || root.Name != "InkCanvasStrokes")
                {
                    throw new Exception("无效的XML墨迹文件格式");
                }

                var strokes = new StrokeCollection();
                foreach (var strokeElement in root.Elements("Stroke"))
                {
                    var drawingAttributesStr = strokeElement.Attribute("DrawingAttributes")?.Value ?? "";
                    var da = ParseDrawingAttributes(drawingAttributesStr);

                    var stylusPoints = new StylusPointCollection();
                    var stylusPointsElement = strokeElement.Element("StylusPoints");
                    if (stylusPointsElement != null)
                    {
                        foreach (var pointElement in stylusPointsElement.Elements("StylusPoint"))
                        {
                            double x = double.Parse(pointElement.Attribute("X")?.Value ?? "0");
                            double y = double.Parse(pointElement.Attribute("Y")?.Value ?? "0");
                            float pressure = float.Parse(pointElement.Attribute("PressureFactor")?.Value ?? "0.5");
                            stylusPoints.Add(new StylusPoint(x, y, pressure));
                        }
                    }

                    if (stylusPoints.Count > 0)
                    {
                        var stroke = new Stroke(stylusPoints) { DrawingAttributes = da };
                        strokes.Add(stroke);
                    }
                }

                ClearStrokes(true);
                timeMachine.ClearStrokeHistory();
                inkCanvas.Strokes.Add(strokes);
                LogHelper.NewLog($"XML Strokes Insert: Strokes Count: {inkCanvas.Strokes.Count}");

                // 恢复元素信息
                var elementsFile = Path.ChangeExtension(filePath, ".elements.json");
                if (File.Exists(elementsFile))
                {
                    var elementInfos = JsonConvert.DeserializeObject<List<CanvasElementInfo>>(File.ReadAllText(elementsFile));
                    foreach (var info in elementInfos)
                    {
                        if (info.Type == "Image" && File.Exists(info.SourcePath))
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.UriSource = new Uri(info.SourcePath);
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                            var img = new Image
                            {
                                Source = bitmapImage,
                                Width = info.Width,
                                Height = info.Height,
                                Stretch = Enum.TryParse<Stretch>(info.Stretch, out var stretch) ? stretch : Stretch.Fill
                            };
                            InkCanvas.SetLeft(img, info.Left);
                            InkCanvas.SetTop(img, info.Top);
                            inkCanvas.Children.Add(img);
                        }
                        else if (string.Equals(info.Type, "Pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(info.SourcePath))
                        {
                            Dispatcher.BeginInvoke(new Action(() => { _ = RestorePdfFromElementInfoAsync(info); }), DispatcherPriority.Loaded);
                        }
                        else if (string.Equals(info.Type, "Media", StringComparison.OrdinalIgnoreCase) && File.Exists(info.SourcePath))
                        {
                            RestoreMediaFromElementInfo(info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"打开XML墨迹文件失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 从XML文件加载StrokeCollection（辅助方法，用于ZIP文件恢复）
        /// </summary>
        private StrokeCollection LoadStrokesFromXML(string xmlPath)
        {
            try
            {
                XDocument doc = XDocument.Load(xmlPath);
                var root = doc.Root;
                if (root == null || root.Name != "InkCanvasStrokes")
                {
                    throw new InvalidDataException("无效的XML墨迹文件格式");
                }

                var strokes = new StrokeCollection();
                foreach (var strokeElement in root.Elements("Stroke"))
                {
                    var drawingAttributesStr = strokeElement.Attribute("DrawingAttributes")?.Value ?? "";
                    var da = ParseDrawingAttributes(drawingAttributesStr);

                    var stylusPoints = new StylusPointCollection();
                    var stylusPointsElement = strokeElement.Element("StylusPoints");
                    if (stylusPointsElement != null)
                    {
                        foreach (var pointElement in stylusPointsElement.Elements("StylusPoint"))
                        {
                            double x = double.Parse(pointElement.Attribute("X")?.Value ?? "0");
                            double y = double.Parse(pointElement.Attribute("Y")?.Value ?? "0");
                            float pressure = float.Parse(pointElement.Attribute("PressureFactor")?.Value ?? "0.5");
                            stylusPoints.Add(new StylusPoint(x, y, pressure));
                        }
                    }

                    if (stylusPoints.Count > 0)
                    {
                        var stroke = new Stroke(stylusPoints) { DrawingAttributes = da };
                        strokes.Add(stroke);
                    }
                }

                return strokes;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"从XML加载墨迹失败: {ex}", LogHelper.LogType.Error);
                throw;
            }
        }

        /// <summary>
        /// 从字符串解析DrawingAttributes
        /// </summary>
        private DrawingAttributes ParseDrawingAttributes(string attributesStr)
        {
            var da = new DrawingAttributes();
            var parts = attributesStr.Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    var value = kv[1].Trim();
                    switch (key)
                    {
                        case "Color":
                            da.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
                            break;
                        case "Width":
                            da.Width = double.Parse(value);
                            break;
                        case "Height":
                            da.Height = double.Parse(value);
                            break;
                        case "FitToCurve":
                            da.FitToCurve = bool.Parse(value);
                            break;
                        case "IsHighlighter":
                            da.IsHighlighter = bool.Parse(value);
                            break;
                        case "IgnorePressure":
                            da.IgnorePressure = bool.Parse(value);
                            break;
                        case "StylusTip":
                            da.StylusTip = Enum.TryParse<StylusTip>(value, out var tip) ? tip : StylusTip.Ellipse;
                            break;
                    }
                }
            }
            return da;
        }

        /// <summary>
        /// 打开单个墨迹文件
        /// </summary>
        /// <param name="filePath">墨迹文件的路径</param>
        /// <remarks>
        /// 该方法会：
        /// 1. 打开墨迹文件并加载墨迹
        /// 2. 检查文件是否包含墨迹
        /// 3. 如果包含墨迹，清空当前墨迹并添加新墨迹
        /// 4. 恢复元素信息
        /// 5. 如果文件流中没有墨迹，尝试从内存流中加载
        /// </remarks>
        public void OpenSingleStrokeFile(string filePath)
        {
            if (TryBlockFrozenPageMutation("打开墨迹文件")) return;
            var fileStreamHasNoStroke = false;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var strokes = new StrokeCollection(fs);
                fileStreamHasNoStroke = strokes.Count == 0;
                if (!fileStreamHasNoStroke)
                {
                    ClearStrokes(true);
                    timeMachine.ClearStrokeHistory();
                    inkCanvas.Strokes.Add(strokes);
                    LogHelper.NewLog($"Strokes Insert: Strokes Count: {inkCanvas.Strokes.Count.ToString()}");
                }
            }

            // 恢复元素信息
            var elementsFile = Path.ChangeExtension(filePath, ".elements.json");
            if (File.Exists(elementsFile))
            {
                var elementInfos = JsonConvert.DeserializeObject<List<CanvasElementInfo>>(File.ReadAllText(elementsFile));
                foreach (var info in elementInfos)
                {
                    if (info.Type == "Image" && File.Exists(info.SourcePath))
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri(info.SourcePath);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        var img = new Image
                        {
                            Source = bitmapImage,
                            Width = info.Width,
                            Height = info.Height,
                            Stretch = Enum.TryParse<Stretch>(info.Stretch, out var stretch) ? stretch : Stretch.Fill
                        };
                        InkCanvas.SetLeft(img, info.Left);
                        InkCanvas.SetTop(img, info.Top);
                        inkCanvas.Children.Add(img);
                    }
                    else if (string.Equals(info.Type, "Pdf", StringComparison.OrdinalIgnoreCase) && File.Exists(info.SourcePath))
                    {
                        Dispatcher.BeginInvoke(new Action(() => { _ = RestorePdfFromElementInfoAsync(info); }), DispatcherPriority.Loaded);
                    }
                    else if (string.Equals(info.Type, "Media", StringComparison.OrdinalIgnoreCase) && File.Exists(info.SourcePath))
                    {
                        RestoreMediaFromElementInfo(info);
                    }
                }
            }

            if (fileStreamHasNoStroke)
                using (var ms = new MemoryStream(File.ReadAllBytes(filePath)))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var strokes = new StrokeCollection(ms);
                    ClearStrokes(true);
                    timeMachine.ClearStrokeHistory();
                    inkCanvas.Strokes.Add(strokes);
                    LogHelper.NewLog($"Strokes Insert (2): Strokes Count: {strokes.Count.ToString()}");
                }
        }
    }
}

