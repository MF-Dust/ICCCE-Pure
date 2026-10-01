using Ink_Canvas;
using Ink_Canvas.Helpers.Persistence;
using Ink_Canvas.UInk;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class PersistenceChecks
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static void Run(MainWindow window, InkCanvas canvas)
    {
        var original = MainWindow.Settings;
        var root = Path.Combine(Path.GetTempPath(), "icc-persistence-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            MainWindow.Settings = new Settings();
            var options = MainWindow.Settings.Automation;
            options.AutoSavedStrokesLocation = root;
            options.IsUseCustomSaveFileName = true;
            options.CustomSaveFileNameTemplate = "snapshot";
            Set(window, "_currentMode", 0);
            Set(window, "_currentWhiteboardIndex", 2);
            canvas.Width = 160;
            canvas.Height = 100;
            canvas.Measure(new Size(160, 100));
            canvas.Arrange(new Rect(0, 0, 160, 100));
            canvas.UpdateLayout();
            canvas.Strokes = Strokes(30);

            using (var snapshot = Capture(window))
            {
                canvas.Strokes[0].StylusPoints[0] = new StylusPoint(90, 10);
                canvas.Strokes.Add(Strokes(50));
                Set(window, "_currentWhiteboardIndex", 3); // Switching pages must not alter an accepted save.
                Task.Run(snapshot.Write).GetAwaiter().GetResult();
                var loaded = ReadIsf(snapshot.Path);
                Check(loaded.Count == 1 && IsfCoordinateEquals(loaded[0].StylusPoints[0].X, 30), $"icstk must roundtrip the captured, not live state (count={loaded.Count}, x={loaded[0].StylusPoints[0].X})");
                Check(File.ReadAllText(Path.ChangeExtension(snapshot.Path, ".elements.json")) == "[]", "single-page element metadata stays compatible");
            }

            canvas.Strokes = Strokes(40);
            options.IsSaveStrokesAsXML = true;
            using (var snapshot = Capture(window))
            {
                Task.Run(snapshot.Write).GetAwaiter().GetResult();
                var xml = Path.ChangeExtension(snapshot.Path, ".xml");
                var loaded = LoadXml(window, xml);
                Check(loaded.Count == 1 && loaded[0].StylusPoints[0].X == 40 && loaded[0].DrawingAttributes.Width == 4,
                    "XML must roundtrip points and attributes through the existing reader");
            }

            Set(window, "_currentMode", 1);
            Set(window, "_currentWhiteboardIndex", 2);
            typeof(MainWindow).GetProperty("WhiteboardTotalCount", Instance).SetValue(window, 3);
            var histories = new Ink_Canvas.Helpers.TimeMachineHistory[101][];
            histories[1] = new[] { new Ink_Canvas.Helpers.TimeMachineHistory(Strokes(10), Ink_Canvas.Helpers.TimeMachineHistoryType.UserInput, false) };
            Set(window, "TimeMachineHistories", histories);
            using (var snapshot = Capture(window))
            {
                snapshot.Write();
                using var zip = ZipFile.OpenRead(snapshot.Path);
                Check(zip.Entries.Select(e => e.FullName).OrderBy(x => x).SequenceEqual(new[] {
                    "metadata.txt", "page_0001.elements.json", "page_0001.xml", "page_0002.elements.json", "page_0002.xml" }),
                    "XML ZIP entries and sparse empty-page semantics must stay compatible");
                CheckMetadata(zip);
                var xml = Path.Combine(root, "read-page.xml");
                zip.GetEntry("page_0002.xml").ExtractToFile(xml);
                Check(LoadXml(window, xml)[0].StylusPoints[0].X == 40, "XML ZIP page roundtrip");
            }
            options.IsSaveStrokesAsXML = false;
            using (var snapshot = Capture(window))
            {
                snapshot.Write();
                Check(snapshot.Files.Count == 2 && snapshot.Files.All(x => x.name.EndsWith(".icstk")), "ordinary multipage exports skip empty pages");
                Check(IsfCoordinateEquals(ReadIsf(snapshot.Files[0].name)[0].StylusPoints[0].X, 10), "historical icstk page roundtrip");
                Check(IsfCoordinateEquals(ReadIsf(snapshot.Files[1].name)[0].StylusPoints[0].X, 40), "current icstk page roundtrip");
            }
            options.IsSaveFullPageStrokes = true;
            using (var snapshot = Capture(window))
            {
                Task.Run(snapshot.Write).GetAwaiter().GetResult();
                using var zip = ZipFile.OpenRead(snapshot.Path);
                Check(zip.Entries.Select(e => e.FullName).OrderBy(x => x).SequenceEqual(new[] {
                    "metadata.txt", "page_0001.icstk", "page_0001.png", "page_0002.icstk", "page_0002.png" }),
                    "full-page ZIP entries must stay compatible");
                CheckMetadata(zip);
                using var isf = zip.GetEntry("page_0002.icstk").Open();
                Check(IsfCoordinateEquals(new StrokeCollection(isf)[0].StylusPoints[0].X, 40), "full-page ZIP ISF roundtrip");
                using var png = zip.GetEntry("page_0002.png").Open();
                CheckPng(png, (int)canvas.ActualWidth, (int)canvas.ActualHeight, requireInk: true);
            }
            Set(window, "_currentMode", 0);
            using (var snapshot = Capture(window))
            {
                Task.Run(snapshot.Write).GetAwaiter().GetResult();
                Check(ReadIsf(snapshot.Path).Count == 1, "full-page single export retains icstk compatibility");
                using var png = File.OpenRead(Path.ChangeExtension(snapshot.Path, ".png"));
                Check(BitmapDecoder.Create(png, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.Count == 1, "full-page single PNG is readable");
            }
            options.IsSaveFullPageStrokes = false;
            options.IsSaveStrokesAsUInK = true;
            Set(window, "_currentMode", 1);
            string mediaSource = Path.Combine(root, "captured.png");
            var pixel = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0, 0, 255, 255 }, 4);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(pixel));
            using (var imageFile = File.Create(mediaSource)) encoder.Save(imageFile);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(mediaSource);
            bitmap.EndInit();
            bitmap.Freeze();
            var image = new Image { Source = bitmap, Width = 32, Height = 24 };
            InkCanvas.SetLeft(image, 8);
            InkCanvas.SetTop(image, 4);
            canvas.Children.Add(image);
            using (var snapshot = Capture(window))
            {
                ExpectIoFailure(() => File.Delete(mediaSource), "UI-captured media is protected while the async save is pending");
                canvas.Children.Clear();
                canvas.Strokes.Clear();
                Task.Run(snapshot.Write).GetAwaiter().GetResult();
                var doc = UInkReader.Load(snapshot.Path);
                var pages = UInkIccMapper.ToPages(doc, UInkConversion.BlockToStroke);
                Check(doc.Header.PageNum == 3 && pages.Count == 3 && pages[2].FinalStrokes.Count == 0,
                    "UInk preserves empty canvases and page counts");
                Check(pages[1].FinalStrokes[0].StylusPoints[0].X == 40, "UInk independent snapshot roundtrip");
                var media = pages[1].Media.Single();
                Check(media.Width == 32 && media.Height == 24 && media.Transform[4] == 8 && media.Transform[5] == 4,
                    "UInk captured media layout survives changing the live canvas");
                using var archive = ZipFile.OpenRead(snapshot.Path + ".extra");
                Check(archive.GetEntry(media.Path) != null, "UInk document media points to a staged archive entry");
            }
            File.Delete(mediaSource);
            CheckMediaAndFailures(root);
            CheckCoordinator();
            CheckModelAndRelease();
            Console.WriteLine("Persistence snapshot/ISF/XML/ZIP/UInk/queue/failure checks passed.");
        }
        finally
        {
            MainWindow.Settings = original;
            Directory.Delete(root, true);
        }
    }

    // Informational timings, not machine-dependent CI assertions. Capture includes UI serialization.
    public static void Benchmark()
    {
        var previousSettings = MainWindow.Settings;
        var root = Path.Combine(Path.GetTempPath(), "icc-save-benchmark-" + Guid.NewGuid().ToString("N"));
        var window = (MainWindow)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(MainWindow));
        var canvas = new InkCanvas();
        Set(window, "inkCanvas", canvas);
        Set(window, "_currentWhiteboardIndex", 1);
        try
        {
            foreach (var scenario in new[] { (name: "single-isf", pages: 1, strokes: 1000),
                (name: "multi-xml-zip", pages: 20, strokes: 200), (name: "multi-uink", pages: 20, strokes: 200) })
            {
                MainWindow.Settings = new Settings();
                var options = MainWindow.Settings.Automation;
                options.AutoSavedStrokesLocation = root;
                options.IsUseCustomSaveFileName = true;
                options.CustomSaveFileNameTemplate = scenario.name;
                options.IsSaveStrokesAsXML = scenario.name == "multi-xml-zip";
                options.IsSaveStrokesAsUInK = scenario.name == "multi-uink";
                options.IsSaveFullPageStrokes = false;
                Set(window, "_currentMode", scenario.pages > 1 ? 1 : 0);
                typeof(MainWindow).GetProperty("WhiteboardTotalCount", Instance).SetValue(window, scenario.pages);
                var strokes = new StrokeCollection();
                for (int i = 0; i < scenario.strokes; i++) strokes.Add(Strokes(i % 100));
                canvas.Strokes = strokes;
                var histories = new Ink_Canvas.Helpers.TimeMachineHistory[101][];
                for (int page = 2; page <= scenario.pages; page++)
                    histories[page] = new[] { new Ink_Canvas.Helpers.TimeMachineHistory(strokes.Clone(),
                        Ink_Canvas.Helpers.TimeMachineHistoryType.UserInput, false) };
                Set(window, "TimeMachineHistories", histories);
                var captures = new double[5];
                var writes = new double[5];
                var allocations = new long[5];
                long bytes = 0;
                for (int run = -1; run < captures.Length; run++) // One warmup before five samples.
                {
                    long allocated = GC.GetAllocatedBytesForCurrentThread();
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    using var snapshot = Capture(window);
                    double captureMs = timer.Elapsed.TotalMilliseconds;
                    allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
                    double writeMs = Task.Run(() =>
                    {
                        var io = System.Diagnostics.Stopwatch.StartNew();
                        snapshot.Write();
                        return io.Elapsed.TotalMilliseconds;
                    }).GetAwaiter().GetResult();
                    bytes = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                        .Where(p => Path.GetFileName(p).StartsWith(scenario.name, StringComparison.Ordinal))
                        .Sum(p => new FileInfo(p).Length);
                    if (run < 0) continue;
                    captures[run] = captureMs;
                    writes[run] = writeMs;
                    allocations[run] = allocated;
                }
                Array.Sort(captures);
                Array.Sort(writes);
                Array.Sort(allocations);
                Console.WriteLine($"{scenario.name} {scenario.pages}x{scenario.strokes}: UI capture median {captures[2]:F2} ms, " +
                    $"worker write median {writes[2]:F2} ms, UI allocated {allocations[2]} bytes, output {bytes} bytes");
            }
        }
        finally
        {
            MainWindow.Settings = previousSettings;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void CheckMetadata(ZipArchive zip)
    {
        using var reader = new StreamReader(zip.GetEntry("metadata.txt").Open());
        var metadata = reader.ReadToEnd();
        Check(metadata.Contains("总页数: 3") && metadata.Contains("当前页面: 2") && metadata.Contains("页面 3: 0 条墨迹"),
            "ZIP metadata must preserve total/current/empty-page information");
    }

    private static void CheckPng(Stream stream, int width, int height, bool requireInk)
    {
        // ZIP entry streams are not seekable; WPF otherwise returns a deferred 1x1 placeholder.
        using var buffered = new MemoryStream();
        stream.CopyTo(buffered);
        buffered.Position = 0;
        var bitmap = BitmapDecoder.Create(buffered, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames[0];
        Check(bitmap.PixelWidth == width && bitmap.PixelHeight == height, $"PNG size roundtrip: {bitmap.PixelWidth}x{bitmap.PixelHeight}, expected {width}x{height}");
        if (!requireInk) return;
        var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
        var pixels = new byte[width * height * 4];
        converted.CopyPixels(pixels, width * 4, 0);
        Check(Enumerable.Range(0, width * height).Any(i => pixels[i * 4 + 3] != 0), "full-page PNG must contain rendered ink, not an unarranged empty canvas");
    }

    private static void CheckMediaAndFailures(string root)
    {
        var source = Path.Combine(root, "media.bin");
        File.WriteAllBytes(source, new byte[] { 1, 2, 3 });
        var target = Path.Combine(root, "media.uink");
        using (var snapshot = new SaveSnapshot { Path = target, UInkDocument = MediaDocument("media/old.bin") })
        {
            snapshot.Resources.Add(("media/old.bin", source));
            snapshot.PinMedia(source);
            ExpectIoFailure(() => File.Delete(source), "captured UInk media must remain available until it is staged");
            Task.Run(snapshot.Write).GetAwaiter().GetResult();
            using var archive = ZipFile.OpenRead(target + ".extra");
            using var stream = archive.GetEntry("media/old.bin").Open();
            Check(stream.ReadByte() == 1 && stream.ReadByte() == 2 && stream.ReadByte() == 3, "UInk media ZIP roundtrip");
        }
        var newSource = Path.Combine(root, "new-media.bin");
        File.WriteAllBytes(newSource, new byte[] { 4, 5 });
        UInkSaveService.SaveFull(MediaDocument("media/new.bin"), target, new[] { ("media/new.bin", newSource) });
        using (var archive = ZipFile.OpenRead(target + ".extra"))
            Check(archive.GetEntry("media/old.bin") != null && archive.GetEntry("media/new.bin") != null,
                "UInk extra-first commit preserves resources referenced by the previous main file");
        byte[] oldMain = File.ReadAllBytes(target);
        byte[] oldExtra = File.ReadAllBytes(target + ".extra");
        ExpectIoFailure(() => UInkSaveService.SaveFull(MediaDocument("media/missing.bin"), target,
            new[] { ("media/missing.bin", Path.Combine(root, "missing.bin")) }), "missing media must fail, not claim a successful UInk save");
        Check(File.ReadAllBytes(target).SequenceEqual(oldMain) && File.ReadAllBytes(target + ".extra").SequenceEqual(oldExtra),
            "failed UInk staging must retain the previous main and extra files");
        Check(!File.Exists(target + ".tmp") && !File.Exists(target + ".extra.tmp"), "failed UInk staging removes only temporary files");
        using (var hold = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read))
            ExpectIoFailure(() => UInkSaveService.SaveFull(MediaDocument("media/next.bin"), target,
                new[] { ("media/next.bin", newSource) }), "UInk main replacement failure must propagate after extra-first commit");
        Check(File.ReadAllBytes(target).SequenceEqual(oldMain), "failed final UInk commit leaves the old main intact");
        using (var archive = ZipFile.OpenRead(target + ".extra"))
            Check(archive.GetEntry("media/new.bin") != null && archive.GetEntry("media/next.bin") != null,
                "extra-first failure retains media referenced by both old and pending main documents");

        var zipTarget = Path.Combine(root, "atomic.zip");
        using var zipSnapshot = new SaveSnapshot { Path = zipTarget, IsZip = true };
        zipSnapshot.Files.Add(("metadata.txt", new byte[] { 7 }));
        zipSnapshot.Write();
        var originalZip = File.ReadAllBytes(zipTarget);
        using (var hold = new FileStream(zipTarget, FileMode.Open, FileAccess.Read, FileShare.Read))
            ExpectIoFailure(zipSnapshot.Write, "ZIP replacement IO failure must propagate");
        Check(File.ReadAllBytes(zipTarget).SequenceEqual(originalZip) && !File.Exists(zipTarget + ".tmp"), "failed ZIP replacement preserves the existing ZIP");
        var isfTarget = Path.Combine(root, "atomic.icstk");
        File.WriteAllBytes(isfTarget, new byte[] { 8, 9 });
        ExpectIoFailure(() => SaveSnapshot.AtomicWrite(isfTarget, stream =>
        {
            stream.WriteByte(0);
            throw new IOException("injected IO failure");
        }), "atomic file write must propagate failures");
        Check(File.ReadAllBytes(isfTarget).SequenceEqual(new byte[] { 8, 9 }) && !File.Exists(isfTarget + ".tmp"), "atomic file failure must not truncate user data");
    }

    private static UInkDocument MediaDocument(string path) => UInkIccMapper.BuildDocument(UInkIccMapper.NewFileGuid(),
        Array.Empty<UInkDevice>(), Array.Empty<UInkWorkspace>(), new[] { new UInkPageInput
        {
            Canvas = UInkIccMapper.BuildCanvas("", "", Guid.NewGuid().ToString(), 0, 1, null, null),
            Strokes = new StrokeCollection(),
            Media = new System.Collections.Generic.List<UInkMedia> { new UInkMedia { Path = path, MimeType = "application/octet-stream" } }
        } }, 1);

    private static void CheckCoordinator()
    {
        var queue = new SaveCoordinator();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        int running = 0;
        int capturedValue = 1;
        int savedValue = 0;
        var first = queue.Submit(() => () =>
        {
            Check(Interlocked.Increment(ref running) == 1, "writers must not overlap");
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new Exception("save gate timeout");
            Interlocked.Decrement(ref running);
        }, false);
        try
        {
            Check(entered.Wait(TimeSpan.FromSeconds(10)), "first save started");
            bool capturedAuto = false;
            Check(queue.Submit(() => { capturedAuto = true; return () => { }; }, true) == null && !capturedAuto,
                "busy automatic saves skip without capture or queueing");
            var manual = queue.Submit(() =>
            {
                int snapshot = capturedValue;
                return () =>
                {
                    Check(Interlocked.Increment(ref running) == 1, "queued manual writer must be mutually exclusive");
                    savedValue = snapshot;
                    Interlocked.Decrement(ref running);
                };
            }, false);
            capturedValue = 2;
            Check(!manual.IsCompleted, "manual save waits for the accepted writer");
            var drain = queue.StopAndDrainAsync();
            Check(!drain.IsCompleted && queue.Submit(() => () => { }, false) == null, "closing waits for all accepted saves and rejects new ones");
            release.Set();
            drain.GetAwaiter().GetResult();
            first.GetAwaiter().GetResult();
            Check(savedValue == 1 && manual.IsCompletedSuccessfully, "queued manual snapshot is captured before live state changes");
        }
        finally { release.Set(); }
        var failedQueue = new SaveCoordinator();
        var failed = failedQueue.Submit(() => () => throw new IOException("injected writer failure"), false);
        int recovered = 0;
        var next = failedQueue.Submit(() => () => recovered++, false);
        ExpectIoFailure(() => failed.GetAwaiter().GetResult(), "save queue reports IO failures");
        next.GetAwaiter().GetResult();
        Check(recovered == 1, "one failed save cannot discard the next accepted manual save");
    }

    private static void CheckModelAndRelease()
    {
        var element = new CanvasElementInfo { Type = "Pdf", SourcePath = "paper.pdf", PdfCurrentPage = 2, PdfPageCount = 7 };
        var json = JObject.FromObject(element);
        Check(typeof(CanvasElementInfo).FullName == "Ink_Canvas.CanvasElementInfo" && typeof(CanvasElementInfo).IsPublic,
            "moving the metadata model must preserve its public type identity");
        Check(json.Properties().Select(x => x.Name).SequenceEqual(new[] { "Type", "SourcePath", "Left", "Top", "Width", "Height",
            "Stretch", "MediaKind", "MediaDisplayName", "MediaPositionSeconds", "MediaSpeedRatio", "MediaVolume", "PdfCurrentPage", "PdfPageCount" }),
            "element JSON property names must remain unchanged");
        Check(json.ToObject<CanvasElementInfo>().PdfCurrentPage == 2 && (string)json["Stretch"] == "Fill", "element JSON roundtrip/defaults");
#if !DEBUG
        Check(typeof(App).GetMethod("ExtractDispatcherOpName", BindingFlags.Static | BindingFlags.NonPublic) == null,
            "Release must not contain per-operation Dispatcher diagnostics");
#endif
        Check(!typeof(App).Assembly.GetReferencedAssemblies().Any(x => x.Name.StartsWith("Vortice.") || x.Name == "InkCanvas.PPTAgent.Contracts"),
            "removed Vortice/PPTAgent dependencies must not remain in the application assembly");
    }

    public static SaveSnapshot Capture(MainWindow window)
        => (SaveSnapshot)typeof(MainWindow).GetMethod("CaptureSaveSnapshot", Instance).Invoke(window, new object[] { true });
    private static StrokeCollection LoadXml(MainWindow window, string path)
        => (StrokeCollection)typeof(MainWindow).GetMethod("LoadStrokesFromXML", Instance).Invoke(window, new object[] { path });
    private static StrokeCollection ReadIsf(string path)
    {
        using var stream = File.OpenRead(path);
        return new StrokeCollection(stream);
    }
    public static StrokeCollection Strokes(double x) => new StrokeCollection
    {
        new Stroke(new StylusPointCollection { new StylusPoint(x, 10), new StylusPoint(x + 15, 25) })
        { DrawingAttributes = new DrawingAttributes { Width = 4, Height = 4 } }
    };
    // ISF stores positions in integral HIMETRIC units (one unit = 96 / 2540 DIPs).
    public static bool IsfCoordinateEquals(double actual, double expected) => Math.Abs(actual - expected) <= 96.0 / 2540;
    public static void Set(MainWindow window, string name, object value) => typeof(MainWindow).GetField(name, Instance).SetValue(window, value);
    private static void ExpectIoFailure(Action action, string message)
    {
        bool failed = false;
        try { action(); }
        catch (IOException) { failed = true; }
        catch (UnauthorizedAccessException) { failed = true; }
        Check(failed, message);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
