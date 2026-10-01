using Ink_Canvas.Helpers;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace Ink_Canvas
{
    public partial class MainWindow : Ink_Canvas.Helpers.PerformanceTransparentWin
    {
        /// <summary>
        /// 墨迹重播按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void GridInkReplayButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("重播冻结页面内容")) return;
            //if (lastBorderMouseDownObject != sender) return;

            AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);
            AnimationsHelper.HideWithSlideAndFade(BoardImageOptionsPanel);

            CollapseBorderDrawShape();

            InkCanvasForInkReplay.Visibility = Visibility.Visible;
            InkCanvasGridForInkReplay.Visibility = Visibility.Hidden;
            InkCanvasGridForInkReplay.IsHitTestVisible = false;
            FloatingbarUIForInkReplay.Visibility = Visibility.Hidden;
            FloatingbarUIForInkReplay.IsHitTestVisible = false;
            BlackboardUIGridForInkReplay.Visibility = Visibility.Hidden;
            BlackboardUIGridForInkReplay.IsHitTestVisible = false;

            AnimationsHelper.ShowWithFadeIn(BorderInkReplayToolBox);
            InkReplayPanelStatusText.Text = Properties.MainWindowStrings.Main_InkReplayPlaying;
            InkReplayPlayPauseBorder.Background = new SolidColorBrush(Colors.Transparent);
            InkReplayPlayButtonImage.Visibility = Visibility.Collapsed;
            InkReplayPauseButtonImage.Visibility = Visibility.Visible;

            isStopInkReplay = false;
            isPauseInkReplay = false;
            isRestartInkReplay = false;
            inkReplaySpeed = 1;
            InkCanvasForInkReplay.Strokes.Clear();
            var strokes = inkCanvas.Strokes.Clone();
            if (inkCanvas.GetSelectedStrokes().Count != 0) strokes = inkCanvas.GetSelectedStrokes().Clone();
            int k = 1, i = 0;
            new Thread(() =>
            {
                isRestartInkReplay = true;
                while (isRestartInkReplay)
                {
                    isRestartInkReplay = false;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InkCanvasForInkReplay.Strokes.Clear();
                    });
                    foreach (var stroke in strokes)
                    {

                        if (isRestartInkReplay) break;

                        var stylusPoints = new StylusPointCollection();
                        if (stroke.StylusPoints.Count == 629) //圆或椭圆
                        {
                            Stroke s = null;
                            foreach (var stylusPoint in stroke.StylusPoints)
                            {

                                if (isRestartInkReplay) break;

                                while (isPauseInkReplay)
                                {
                                    Thread.Sleep(10);
                                }

                                if (i++ >= 50)
                                {
                                    i = 0;
                                    Thread.Sleep((int)(10 / inkReplaySpeed));
                                    if (isStopInkReplay) return;
                                }

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        InkCanvasForInkReplay.Strokes.Remove(s);
                                    }
                                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }

                                    stylusPoints.Add(stylusPoint);
                                    s = new Stroke(stylusPoints.Clone())
                                    {
                                        DrawingAttributes = stroke.DrawingAttributes
                                    };
                                    InkCanvasForInkReplay.Strokes.Add(s);
                                });
                            }
                        }
                        else
                        {
                            Stroke s = null;
                            foreach (var stylusPoint in stroke.StylusPoints)
                            {

                                if (isRestartInkReplay) break;

                                while (isPauseInkReplay)
                                {
                                    Thread.Sleep(10);
                                }

                                if (i++ >= k)
                                {
                                    i = 0;
                                    Thread.Sleep((int)(10 / inkReplaySpeed));
                                    if (isStopInkReplay) return;
                                }

                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    try
                                    {
                                        InkCanvasForInkReplay.Strokes.Remove(s);
                                    }
                                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }

                                    stylusPoints.Add(stylusPoint);
                                    s = new Stroke(stylusPoints.Clone())
                                    {
                                        DrawingAttributes = stroke.DrawingAttributes
                                    };
                                    InkCanvasForInkReplay.Strokes.Add(s);
                                });
                            }
                        }
                    }
                }

                Thread.Sleep(100);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InkCanvasForInkReplay.Visibility = Visibility.Collapsed;
                    InkCanvasGridForInkReplay.Visibility = Visibility.Visible;
                    InkCanvasGridForInkReplay.IsHitTestVisible = true;
                    AnimationsHelper.HideWithFadeOut(BorderInkReplayToolBox);
                    FloatingbarUIForInkReplay.Visibility = Visibility.Visible;
                    FloatingbarUIForInkReplay.IsHitTestVisible = true;
                    BlackboardUIGridForInkReplay.Visibility = Visibility.Visible;
                    BlackboardUIGridForInkReplay.IsHitTestVisible = true;
                    inkCanvas.IsHitTestVisible = true;
                    inkCanvas.IsManipulationEnabled = true;

                    if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                    {
                        inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    }

                    ResetTouchStates();
                });
            }).Start();
        }

        /// <summary>
        /// 是否停止墨迹重播
        /// </summary>
        private bool isStopInkReplay;
        /// <summary>
        /// 是否暂停墨迹重播
        /// </summary>
        private bool isPauseInkReplay;
        /// <summary>
        /// 是否重新开始墨迹重播
        /// </summary>
        private bool isRestartInkReplay;
        /// <summary>
        /// 墨迹重播速度
        /// </summary>
        private double inkReplaySpeed = 1;

        /// <summary>
        /// 墨迹重播画布鼠标按下事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkCanvasForInkReplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                InkCanvasForInkReplay.Visibility = Visibility.Collapsed;
                InkCanvasGridForInkReplay.Visibility = Visibility.Visible;
                InkCanvasGridForInkReplay.IsHitTestVisible = true;
                FloatingbarUIForInkReplay.Visibility = Visibility.Visible;
                FloatingbarUIForInkReplay.IsHitTestVisible = true;
                BlackboardUIGridForInkReplay.Visibility = Visibility.Visible;
                BlackboardUIGridForInkReplay.IsHitTestVisible = true;
                AnimationsHelper.HideWithFadeOut(BorderInkReplayToolBox);
                isStopInkReplay = true;
                inkCanvas.IsHitTestVisible = true;
                inkCanvas.IsManipulationEnabled = true;

                if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }

                ResetTouchStates();
            }
        }

        /// <summary>
        /// 墨迹重播播放/暂停按钮鼠标按下事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayPlayPauseBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            InkReplayPlayPauseBorder.Background = new SolidColorBrush(Color.FromArgb(34, 9, 9, 11));
        }

        /// <summary>
        /// 墨迹重播播放/暂停按钮鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayPlayPauseBorder_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            InkReplayPlayPauseBorder.Background = new SolidColorBrush(Colors.Transparent);
            isPauseInkReplay = !isPauseInkReplay;
            InkReplayPanelStatusText.Text = isPauseInkReplay ? Properties.MainWindowStrings.Main_InkReplay_Paused : Properties.MainWindowStrings.Main_InkReplayPlaying;
            InkReplayPlayButtonImage.Visibility = isPauseInkReplay ? Visibility.Visible : Visibility.Collapsed;
            InkReplayPauseButtonImage.Visibility = !isPauseInkReplay ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 墨迹重播停止按钮鼠标按下事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayStopButtonBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            InkReplayStopButtonBorder.Background = new SolidColorBrush(Color.FromArgb(34, 9, 9, 11));
        }

        /// <summary>
        /// 墨迹重播停止按钮鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayStopButtonBorder_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            InkReplayStopButtonBorder.Background = new SolidColorBrush(Colors.Transparent);
            InkCanvasForInkReplay.Visibility = Visibility.Collapsed;
            InkCanvasGridForInkReplay.Visibility = Visibility.Visible;
            InkCanvasGridForInkReplay.IsHitTestVisible = true;
            FloatingbarUIForInkReplay.Visibility = Visibility.Visible;
            FloatingbarUIForInkReplay.IsHitTestVisible = true;
            BlackboardUIGridForInkReplay.Visibility = Visibility.Visible;
            BlackboardUIGridForInkReplay.IsHitTestVisible = true;
            AnimationsHelper.HideWithFadeOut(BorderInkReplayToolBox);
            isStopInkReplay = true;
        }

        /// <summary>
        /// 墨迹重播重新开始按钮鼠标按下事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayReplayButtonBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            InkReplayReplayButtonBorder.Background = new SolidColorBrush(Color.FromArgb(34, 9, 9, 11));
        }

        /// <summary>
        /// 墨迹重播重新开始按钮鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplayReplayButtonBorder_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            InkReplayReplayButtonBorder.Background = new SolidColorBrush(Colors.Transparent);
            isRestartInkReplay = true;
            isPauseInkReplay = false;
            InkReplayPanelStatusText.Text = Properties.MainWindowStrings.Main_InkReplayPlaying;
            InkReplayPlayButtonImage.Visibility = Visibility.Collapsed;
            InkReplayPauseButtonImage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 墨迹重播速度按钮鼠标按下事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplaySpeedButtonBorder_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            InkReplaySpeedButtonBorder.Background = new SolidColorBrush(Color.FromArgb(34, 9, 9, 11));
        }

        /// <summary>
        /// 墨迹重播速度按钮鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void InkReplaySpeedButtonBorder_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            InkReplaySpeedButtonBorder.Background = new SolidColorBrush(Colors.Transparent);
            inkReplaySpeed = inkReplaySpeed == 0.5 ? 1 :
                inkReplaySpeed == 1 ? 2 :
                inkReplaySpeed == 2 ? 4 :
                inkReplaySpeed == 4 ? 8 : 0.5;
            InkReplaySpeedTextBlock.Text = inkReplaySpeed + "x";
        }

    }
}
