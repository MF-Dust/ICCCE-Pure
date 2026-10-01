using Ink_Canvas.Controls;
using Ink_Canvas.Controls.Toolbar.FloatingToolbar;
using Ink_Canvas.Helpers;
using Ink_Canvas.Properties;
using Ink_Canvas.WorkflowAutomation;
using iNKORE.UI.WPF.Modern;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using Point = System.Windows.Point;

namespace Ink_Canvas
{
    public partial class MainWindow : Ink_Canvas.Helpers.PerformanceTransparentWin
    {
        private static Windows.SettingsViews.SettingsWindow _settingsWindow = null;

        #region "手勢"按鈕

        /// <summary>
        /// 用於浮動工具欄的"手勢"按鈕和白板工具欄的"手勢"按鈕的點擊事件
        /// </summary>
        internal void TwoFingerGestureBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TwoFingerGestureBorder.IsOpen || BoardTwoFingerGestureBorder.IsOpen)
            {
                AnimationsHelper.HidePopupWithSlideAndFade(TwoFingerGestureBorder);
                AnimationsHelper.HidePopupWithSlideAndFade(BoardTwoFingerGestureBorder);
            }
            else
            {
                HideSubPanels();
                if (currentMode == 0)
                {
                    AnimationsHelper.ShowPopupWithSlideAndFade(TwoFingerGestureBorder);
                    _popupManager?.BringToFront(TwoFingerGestureBorder);
                }
                else
                {
                    AnimationsHelper.ShowPopupWithSlideAndFade(BoardTwoFingerGestureBorder);
                    _popupManager?.BringToFront(BoardTwoFingerGestureBorder);
                }
            }
        }

        /// <summary>
        /// 用於更新浮動工具欄的"手勢"按鈕和白板工具欄的"手勢"按鈕的樣式（開啟和關閉狀態）
        /// </summary>
        private void CheckEnableTwoFingerGestureBtnColorPrompt()
        {
            // 根据主题选择手势图标和颜色
            bool isDarkTheme = Settings.Appearance.Theme == 1 ||
                               (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());
            bool isLightTheme = !isDarkTheme;
            string gestureIconPath = isLightTheme ? "/Resources/new-icons/gesture.png" : "/Resources/new-icons/gesture_white.png";

            // 根据主题设置白板模式下的颜色
            Color boardBgColor, boardIconColor, boardTextColor, boardBorderColor;
            if (isLightTheme)
            {
                boardBgColor = Color.FromRgb(244, 244, 245);
                boardIconColor = Color.FromRgb(24, 24, 27);
                boardTextColor = Color.FromRgb(24, 24, 27);
                boardBorderColor = Color.FromRgb(161, 161, 170);
            }
            else
            {
                boardBgColor = Color.FromRgb(39, 39, 42);
                boardIconColor = Color.FromRgb(244, 244, 245);
                boardTextColor = Color.FromRgb(244, 244, 245);
                boardBorderColor = Color.FromRgb(113, 113, 122);
            }

            bool floatingBarAnyOn = Settings.Gesture.IsEnableMultiTouchMode
                || Settings.Gesture.IsEnableTwoFingerZoom
                || Settings.Gesture.IsEnableTwoFingerTranslate
                || Settings.Gesture.IsEnableTwoFingerRotation;
            bool boardAnyOn = Settings.Gesture.IsEnableMultiTouchModeBoard
                || Settings.Gesture.IsEnableTwoFingerZoomBoard
                || Settings.Gesture.IsEnableTwoFingerTranslateBoard
                || Settings.Gesture.IsEnableTwoFingerRotationBoard;

            TwoFingerGestureSimpleStackPanel.Opacity = 1;
            TwoFingerGestureSimpleStackPanel.IsHitTestVisible = true;

            if (floatingBarAnyOn)
            {
                if (Gesture_Icon != null)
                {
                    Gesture_Icon.Icon.Geometry = Geometry.Parse(XamlGraphicsIconGeometries.EnabledGestureIcon);
                    Gesture_Icon.Badge.Geometry = Geometry.Parse("F0 M24,24z M0,0z " + XamlGraphicsIconGeometries.EnabledGestureIconBadgeCheck);
                    if (!ToolbarRegistry.GetUseRedStyle(Gesture_Icon))
                    {
                        Gesture_Icon.IconBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                        Gesture_Icon.Badge.Brush = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                    }
                }
            }
            else
            {
                if (Gesture_Icon != null)
                {
                    Gesture_Icon.Icon.Geometry = Geometry.Parse(XamlGraphicsIconGeometries.DisabledGestureIcon);
                    Gesture_Icon.Badge.Geometry = Geometry.Parse("F0 M24,24z M0,0z");
                    if (!ToolbarRegistry.GetUseRedStyle(Gesture_Icon))
                    {
                        Gesture_Icon.IconBrush = isDarkTheme
                            ? new SolidColorBrush(Color.FromRgb(244, 244, 245))
                            : new SolidColorBrush(Color.FromRgb(24, 24, 27));
                    }
                }
            }

            var boardGestureBtn = FindView("board.gesture") as BoardToolbarButton;
            if (boardGestureBtn != null)
            {
                var boardThemeAccent = Application.Current.TryFindResource("FloatingBarAccentBrush") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(37, 99, 235));
                if (boardAnyOn)
                {
                    boardGestureBtn.Background = boardThemeAccent;
                    boardGestureBtn.IconGeometryDrawing.Brush = new SolidColorBrush(Colors.GhostWhite);
                    boardGestureBtn.IconGeometryDrawing2.Brush = new SolidColorBrush(Colors.GhostWhite);
                    boardGestureBtn.Foreground = new SolidColorBrush(Colors.GhostWhite);
                    boardGestureBtn.BorderBrush = Brushes.Transparent;
                    boardGestureBtn.IconGeometryDrawing.Geometry = Geometry.Parse(XamlGraphicsIconGeometries.EnabledGestureIcon);
                    boardGestureBtn.IconGeometryDrawing2.Geometry = Geometry.Parse("F0 M24,24z M0,0z " + XamlGraphicsIconGeometries.EnabledGestureIconBadgeCheck);
                }
                else
                {
                    var boardThemeNormalBackground = Application.Current.TryFindResource("FloatingBarBackgroundBrush") as Brush
                        ?? Application.Current.TryFindResource("BoardFloatBarBackground") as Brush
                        ?? new SolidColorBrush(boardBgColor);
                    var boardThemeNormalForeground = Application.Current.TryFindResource("FloatingBarForegroundBrush") as Brush
                        ?? Application.Current.TryFindResource("FloatBarForeground") as Brush
                        ?? new SolidColorBrush(boardIconColor);
                    boardGestureBtn.Background = Brushes.Transparent;
                    boardGestureBtn.IconGeometryDrawing.Brush = boardThemeNormalForeground;
                    boardGestureBtn.IconGeometryDrawing2.Brush = boardThemeNormalForeground;
                    boardGestureBtn.Foreground = boardThemeNormalForeground;
                    boardGestureBtn.BorderBrush = Brushes.Transparent;
                    boardGestureBtn.IconGeometryDrawing.Geometry = Geometry.Parse(XamlGraphicsIconGeometries.DisabledGestureIcon);
                    boardGestureBtn.IconGeometryDrawing2.Geometry = Geometry.Parse("F0 M24,24z M0,0z");
                }
            }

            UpdateBoardInkFreezeButtonStyle();
        }

        private void UpdateBoardInkFreezeButtonStyle()
        {
            var boardInkFreezeBtn = FindView("board.inkFreeze") as BoardToolbarButton;
            if (boardInkFreezeBtn != null)
            {
                var accent = Application.Current.TryFindResource("FloatingBarAccentBrush") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(37, 99, 235));
                var normalBackground = Application.Current.TryFindResource("FloatingBarBackgroundBrush") as Brush
                    ?? Application.Current.TryFindResource("BoardFloatBarBackground") as Brush
                    ?? new SolidColorBrush(Color.FromRgb(42, 42, 42));
                var normalForeground = Application.Current.TryFindResource("FloatingBarForegroundBrush") as Brush
                    ?? Application.Current.TryFindResource("FloatBarForeground") as Brush
                    ?? Brushes.White;

                boardInkFreezeBtn.Background = Brushes.Transparent;
                boardInkFreezeBtn.IconGeometryDrawing.Brush = normalForeground;
                boardInkFreezeBtn.Foreground = normalForeground;
            }
        }

        /// <summary>
        /// 控制是否顯示浮動工具欄的"手勢"按鈕
        /// </summary>
        private void CheckEnableTwoFingerGestureBtnVisibility(bool isVisible)
        {
            UpdateToolbarComponentVisibility();
        }

        #endregion "手勢"按鈕

        #region 隱藏子面板和按鈕背景高亮

        /// <summary>
        /// 隐藏形状绘制面板
        /// </summary>
        private void CollapseBorderDrawShape()
        {
            AnimationsHelper.HidePopupWithSlideAndFade(BorderDrawShape);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderDrawShape);
        }

        /// <summary>
        /// 返回 <see cref="HideSubPanels"/> 中已由 <see cref="AnimationsHelper"/> 播放关闭动画的宿主自带弹窗。
        /// 供 <c>CloseAllRegisteredPopups</c> 跳过，避免直接置 IsOpen=false 截断动画。
        /// </summary>
        private HashSet<Popup> GetAnimatedSubPanelPopups()
        {
            return new HashSet<Popup>
            {
                BorderTools,
                BoardBorderToolsPopup,
                PenPalette,
                BoardPenPalette,
                BoardEraserSizePanel,
                EraserSizePanel,
                BorderDrawShape,
                BoardBorderDrawShape,
                BoardImageOptionsPanel,
                TwoFingerGestureBorder,
                BoardTwoFingerGestureBorder,
                BackgroundPalette,
                BoothPopup
            };
        }

        /// <summary>
        /// HideSubPanels的简化版，立即隐藏所有子面板，无动画效果
        /// </summary>
        private void HideSubPanelsImmediately()
        {
            BorderTools.IsOpen = false;
            BoardBorderToolsPopup.IsOpen = false;
            PenPalette.IsOpen = false;
            BoardPenPalette.IsOpen = false;
            BoardEraserSizePanel.IsOpen = false;
            EraserSizePanel.IsOpen = false;
            var leftBorder = FindView("board.pageList.leftBorder") as Border;
            var rightBorder = FindView("board.pageList.rightBorder") as Border;
            if (leftBorder != null) leftBorder.Visibility = Visibility.Collapsed;
            if (rightBorder != null) rightBorder.Visibility = Visibility.Collapsed;
            BoardImageOptionsPanel.IsOpen = false;
            TwoFingerGestureBorder.IsOpen = false;
            BoardTwoFingerGestureBorder.IsOpen = false;
            BoardRoamingPopup.IsOpen = false;
            // 添加隐藏图形工具的二级菜单面板
            BorderDrawShape.IsOpen = false;
            BoardBorderDrawShape.IsOpen = false;

            BackgroundPalette.IsOpen = false;
            BoothPopup.IsOpen = false;

            // 上面是按名字硬编码的宿主自带面板；插件注册的弹窗不在其中，
            // 统一交给 PopupManagerHelper 兜底关闭，保证点击空白处时行为一致。
            _popupManager?.CloseAllRegisteredPopups();
        }

        /// <summary>
        ///     <para>
        ///         易嚴定真，這個多功能函數包括了以下的內容：
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             隱藏浮動工具欄和白板模式下的"更多功能"面板
        ///         </item>
        ///         <item>
        ///             隱藏白板模式下和浮動工具欄的畫筆調色盤
        ///         </item>
        ///         <item>
        ///             隱藏白板模式下的"清屏"按鈕（已作廢）
        ///         </item>
        ///         <item>
        ///             負責給Settings設置面板做隱藏動畫
        ///         </item>
        ///         <item>
        ///             隱藏白板模式下和浮動工具欄的"手勢"面板
        ///         </item>
        ///         <item>
        ///             當<c>ToggleSwitchDrawShapeBorderAutoHide</c>開啟時，會自動隱藏白板模式下和浮動工具欄的"形狀"面板
        ///         </item>
        ///         <item>
        ///             按需高亮指定的浮動工具欄和白板工具欄中的按鈕，通過param：<paramref name="mode"/> 來指定
        ///         </item>
        ///         <item>
        ///             將浮動工具欄自動居中，通過param：<paramref name="autoAlignCenter"/>
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="mode">
        ///     <para>
        ///         按需高亮指定的浮動工具欄和白板工具欄中的按鈕，有下面幾種情況：
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             當<c><paramref name="mode"/>==null</c>時，不會執行任何有關操作
        ///         </item>
        ///         <item>
        ///             當<c><paramref name="mode"/>!="clear"</c>時，會先取消高亮所有工具欄按鈕，然後根據下面的情況進行高亮處理
        ///         </item>
        ///         <item>
        ///             當<c><paramref name="mode"/>=="color" || <paramref name="mode"/>=="pen"</c>時，會高亮浮動工具欄和白板工具欄中的"批註"，"筆"按鈕
        ///         </item>
        ///         <item>
        ///             當<c><paramref name="mode"/>=="eraser"</c>時，會高亮白板工具欄中的"橡皮"和浮動工具欄中的"面積擦"按鈕
        ///         </item>
        ///         <item>
        ///             當<c><paramref name="mode"/>=="eraserByStrokes"</c>時，會高亮白板工具欄中的"橡皮"和浮動工具欄中的"墨跡擦"按鈕
        ///         </item>
        ///         <item>
        ///             當<c><paramref name="mode"/>=="select"</c>時，會高亮浮動工具欄和白板工具欄中的"選擇"，"套索選"按鈕
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="autoAlignCenter">
        ///     是否自動居中浮動工具欄
        /// </param>
        internal async void HideSubPanels(string mode = null, bool autoAlignCenter = false)
        {
            mode = NormalizeToolModeForFreeze(mode);

            var boardPen = FindView("board.pen") as BoardToolbarButton;
            var boardEraser = FindView("board.eraser") as BoardToolbarButton;
            var boardStrokeEraser = FindView("board.strokeEraser") as BoardToolbarButton;
            var boardSelect = FindView("board.select") as BoardToolbarButton;
            var boardRoaming = FindView("board.roaming") as BoardToolbarButton;
            var boardThemeBackground = Application.Current.TryFindResource("FloatingBarBackgroundBrush") as Brush
                ?? Application.Current.TryFindResource("BoardFloatBarBackground") as Brush
                ?? Brushes.Transparent;
            var boardThemeForeground = Application.Current.TryFindResource("FloatingBarForegroundBrush") as Brush
                ?? Application.Current.TryFindResource("FloatBarForeground") as Brush
                ?? Brushes.White;
            var boardThemeAccent = Application.Current.TryFindResource("FloatingBarAccentBrush") as Brush
                ?? new SolidColorBrush(Color.FromRgb(37, 99, 235));

            AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);
            AnimationsHelper.HidePopupWithSlideAndFade(PenPalette);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardPenPalette);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardEraserSizePanel);
            AnimationsHelper.HidePopupWithSlideAndFade(EraserSizePanel);
            AnimationsHelper.HidePopupWithSlideAndFade(BorderDrawShape);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderDrawShape);
            var leftBorderAnim = FindView("board.pageList.leftBorder") as Border;
            var rightBorderAnim = FindView("board.pageList.rightBorder") as Border;
            if (leftBorderAnim != null) AnimationsHelper.HideWithSlideAndFade(leftBorderAnim);
            if (rightBorderAnim != null) AnimationsHelper.HideWithSlideAndFade(rightBorderAnim);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardImageOptionsPanel);
            AnimationsHelper.HidePopupWithSlideAndFade(TwoFingerGestureBorder);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardTwoFingerGestureBorder);
            // 漫游弹窗必须同步关闭，避免关闭动画的 Completed 在重新打开后再次将其关闭。
            BoardRoamingPopup.IsOpen = false;

            AnimationsHelper.HidePopupWithSlideAndFade(BackgroundPalette);

            // 视频展台弹窗也属于"子面板"，HideSubPanels 时一并隐藏
            AnimationsHelper.HidePopupWithSlideAndFade(BoothPopup);

            // 上面是按名字硬编码的宿主自带面板；插件注册的弹窗不在其中，
            // 统一交给 PopupManagerHelper 兜底关闭，保证点击空白处时行为一致。
            // 已启动关闭动画的要跳过，否则直接置 IsOpen=false 会截断动画。
            _popupManager?.CloseAllRegisteredPopups(GetAnimatedSubPanelPopups());

            if (mode != null)
            {
                if (mode != "clear")
                {
                    if (Cursor_Icon != null) { if (!ToolbarRegistry.GetUseRedStyle(Cursor_Icon)) Cursor_Icon.Icon.Brush = new SolidColorBrush(FloatBarForegroundColor); Cursor_Icon.Icon.Geometry = Geometry.Parse(GetCorrectIcon("cursor", false)); }
                    if (Pen_Icon != null) { if (!ToolbarRegistry.GetUseRedStyle(Pen_Icon)) Pen_Icon.Icon.Brush = new SolidColorBrush(FloatBarForegroundColor); Pen_Icon.Icon.Geometry = Geometry.Parse(GetCorrectIcon("pen", false)); }
                    if (EraserByStrokes_Icon != null) { if (!ToolbarRegistry.GetUseRedStyle(EraserByStrokes_Icon)) EraserByStrokes_Icon.Icon.Brush = new SolidColorBrush(FloatBarForegroundColor); EraserByStrokes_Icon.Icon.Geometry = Geometry.Parse(GetCorrectIcon("eraserStroke", false)); }
                    if (Eraser_Icon != null) { if (!ToolbarRegistry.GetUseRedStyle(Eraser_Icon)) Eraser_Icon.Icon.Brush = new SolidColorBrush(FloatBarForegroundColor); Eraser_Icon.Icon.Geometry = Geometry.Parse(GetCorrectIcon("eraserCircle", false)); }
                    if (SymbolIconSelect != null) { if (!ToolbarRegistry.GetUseRedStyle(SymbolIconSelect)) SymbolIconSelect.Icon.Brush = new SolidColorBrush(FloatBarForegroundColor); SymbolIconSelect.Icon.Geometry = Geometry.Parse(GetCorrectIcon("lassoSelect", false)); }

                    bool isDarkThemeForButtons = Settings.Appearance.Theme == 1 ||
                                                 (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());
                    if (isDarkThemeForButtons)
                    {
                        if (boardPen != null) { boardPen.Background = Brushes.Transparent; boardPen.IconGeometryDrawing.Brush = boardThemeForeground; boardPen.Foreground = boardThemeForeground; }
                        if (boardSelect != null) { boardSelect.Background = Brushes.Transparent; boardSelect.IconGeometryDrawing.Brush = boardThemeForeground; boardSelect.Foreground = boardThemeForeground; }
                        if (boardRoaming != null) { boardRoaming.Background = Brushes.Transparent; boardRoaming.IconGeometryDrawing.Brush = boardThemeForeground; boardRoaming.Foreground = boardThemeForeground; }
                        if (boardEraser != null) { boardEraser.Background = Brushes.Transparent; boardEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardEraser.Foreground = boardThemeForeground; }
                        if (boardStrokeEraser != null) { boardStrokeEraser.Background = Brushes.Transparent; boardStrokeEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardStrokeEraser.Foreground = boardThemeForeground; }
                    }
                    else
                    {
                        if (boardPen != null) { boardPen.Background = Brushes.Transparent; boardPen.IconGeometryDrawing.Brush = boardThemeForeground; boardPen.Foreground = boardThemeForeground; }
                        if (boardSelect != null) { boardSelect.Background = Brushes.Transparent; boardSelect.IconGeometryDrawing.Brush = boardThemeForeground; boardSelect.Foreground = boardThemeForeground; }
                        if (boardRoaming != null) { boardRoaming.Background = Brushes.Transparent; boardRoaming.IconGeometryDrawing.Brush = boardThemeForeground; boardRoaming.Foreground = boardThemeForeground; }
                        if (boardEraser != null) { boardEraser.Background = Brushes.Transparent; boardEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardEraser.Foreground = boardThemeForeground; }
                        if (boardStrokeEraser != null) { boardStrokeEraser.Background = Brushes.Transparent; boardStrokeEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardStrokeEraser.Foreground = boardThemeForeground; }
                    }
                }

                // 根据主题选择高光颜色
                Color highlightColor;
                bool isDarkTheme = Settings.Appearance.Theme == 1 ||
                                   (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());

                if (isDarkTheme)
                {
                    highlightColor = Color.FromRgb(102, 204, 255); // #66ccff for dark theme
                }
                else
                {
                    highlightColor = Color.FromRgb(30, 58, 138); // Keep current color for light theme
                }

                switch (mode)
                {
                    case "pen":
                    case "color":
                        {
                            if (Pen_Icon != null && Pen_Icon.Icon != null)
                            {
                                if (!ToolbarRegistry.GetUseRedStyle(Pen_Icon))
                                {
                                    if (Settings.Appearance.ShowPenColorOnFloatingBarIcon)
                                        Pen_Icon.Icon.Brush = new SolidColorBrush(inkCanvas.DefaultDrawingAttributes.Color);
                                    else
                                        Pen_Icon.Icon.Brush = new SolidColorBrush(highlightColor);
                                }
                                Pen_Icon.Icon.Geometry = Geometry.Parse(GetCorrectIcon("pen", true));
                            }
                            if (boardPen != null)
                            {
                                boardPen.Background = boardThemeAccent;
                                boardPen.IconGeometryDrawing.Brush = Brushes.White;
                                boardPen.Foreground = Brushes.White;
                            }

                            SetFloatingBarHighlightPosition("pen");
                            break;
                        }
                    case "eraser":
                        {
                            if (Eraser_Icon != null && Eraser_Icon.Icon != null)
                            {
                                if (!ToolbarRegistry.GetUseRedStyle(Eraser_Icon))
                                    Eraser_Icon.Icon.Brush = new SolidColorBrush(highlightColor);
                                Eraser_Icon.Icon.Geometry =
                                    Geometry.Parse(GetCorrectIcon("eraserCircle", true));
                            }
                            if (boardEraser != null)
                            {
                                boardEraser.Background = boardThemeAccent;
                                boardEraser.IconGeometryDrawing.Brush = Brushes.White;
                                boardEraser.Foreground = Brushes.White;
                            }

                            SetFloatingBarHighlightPosition("eraser");
                            break;
                        }
                    case "eraserByStrokes":
                        {
                            if (EraserByStrokes_Icon != null && EraserByStrokes_Icon.Icon != null)
                            {
                                if (!ToolbarRegistry.GetUseRedStyle(EraserByStrokes_Icon))
                                    EraserByStrokes_Icon.Icon.Brush = new SolidColorBrush(highlightColor);
                                EraserByStrokes_Icon.Icon.Geometry =
                                    Geometry.Parse(GetCorrectIcon("eraserStroke", true));
                            }
                            if (boardStrokeEraser != null)
                            {
                                boardStrokeEraser.Background = boardThemeAccent;
                                boardStrokeEraser.IconGeometryDrawing.Brush = Brushes.White;
                                boardStrokeEraser.Foreground = Brushes.White;
                            }

                            SetFloatingBarHighlightPosition("eraserByStrokes");
                            break;
                        }
                    case "select":
                        {
                            if (SymbolIconSelect != null && SymbolIconSelect.Icon != null)
                            {
                                if (!ToolbarRegistry.GetUseRedStyle(SymbolIconSelect))
                                    SymbolIconSelect.Icon.Brush = new SolidColorBrush(highlightColor);
                                SymbolIconSelect.Icon.Geometry =
                                    Geometry.Parse(GetCorrectIcon("lassoSelect", true));
                            }
                            if (boardSelect != null)
                            {
                                boardSelect.Background = boardThemeAccent;
                                boardSelect.IconGeometryDrawing.Brush = Brushes.White;
                                boardSelect.Foreground = Brushes.White;
                            }

                            SetFloatingBarHighlightPosition("select");
                            break;
                        }
                    case "roaming":
                        {
                            if (boardRoaming != null)
                            {
                                boardRoaming.Background = boardThemeAccent;
                                boardRoaming.IconGeometryDrawing.Brush = Brushes.White;
                                boardRoaming.Foreground = Brushes.White;
                            }

                            SetFloatingBarHighlightPosition("roaming");
                            break;
                        }
                    case "cursor":
                        {
                            if (Cursor_Icon != null && Cursor_Icon.Icon != null)
                            {
                                if (!ToolbarRegistry.GetUseRedStyle(Cursor_Icon))
                                    Cursor_Icon.Icon.Brush = new SolidColorBrush(highlightColor);
                                Cursor_Icon.Icon.Geometry =
                                    Geometry.Parse(GetCorrectIcon("cursor", true));
                            }
                            bool isDarkThemeForCursor = Settings.Appearance.Theme == 1 ||
                                                        (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());
                            if (isDarkThemeForCursor)
                            {
                                if (boardPen != null) { boardPen.Background = Brushes.Transparent; boardPen.IconGeometryDrawing.Brush = boardThemeForeground; boardPen.Foreground = boardThemeForeground; }
                                if (boardEraser != null) { boardEraser.Background = Brushes.Transparent; boardEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardEraser.Foreground = boardThemeForeground; }
                                if (boardStrokeEraser != null) { boardStrokeEraser.Background = Brushes.Transparent; boardStrokeEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardStrokeEraser.Foreground = boardThemeForeground; }
                                if (boardSelect != null) { boardSelect.Background = Brushes.Transparent; boardSelect.IconGeometryDrawing.Brush = boardThemeForeground; boardSelect.Foreground = boardThemeForeground; }
                                if (boardRoaming != null) { boardRoaming.Background = Brushes.Transparent; boardRoaming.IconGeometryDrawing.Brush = boardThemeForeground; boardRoaming.Foreground = boardThemeForeground; }

                                if (BoardInkFreezeBtn != null)
                                {
                                    BoardInkFreezeBtn.Background = Brushes.Transparent;
                                    BoardInkFreezeBtn.IconBrush = boardThemeForeground;
                                    BoardInkFreezeBtn.Foreground = boardThemeForeground;
                                }
                            }
                            else
                            {
                                if (boardPen != null) { boardPen.Background = Brushes.Transparent; boardPen.IconGeometryDrawing.Brush = boardThemeForeground; boardPen.Foreground = boardThemeForeground; }
                                if (boardEraser != null) { boardEraser.Background = Brushes.Transparent; boardEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardEraser.Foreground = boardThemeForeground; }
                                if (boardStrokeEraser != null) { boardStrokeEraser.Background = Brushes.Transparent; boardStrokeEraser.IconGeometryDrawing.Brush = boardThemeForeground; boardStrokeEraser.Foreground = boardThemeForeground; }
                                if (boardSelect != null) { boardSelect.Background = Brushes.Transparent; boardSelect.IconGeometryDrawing.Brush = boardThemeForeground; boardSelect.Foreground = boardThemeForeground; }
                                if (boardRoaming != null) { boardRoaming.Background = Brushes.Transparent; boardRoaming.IconGeometryDrawing.Brush = boardThemeForeground; boardRoaming.Foreground = boardThemeForeground; }

                                if (BoardInkFreezeBtn != null)
                                {
                                    BoardInkFreezeBtn.Background = Brushes.Transparent;
                                    BoardInkFreezeBtn.IconBrush = boardThemeForeground;
                                    BoardInkFreezeBtn.Foreground = boardThemeForeground;
                                }
                            }

                            SetFloatingBarHighlightPosition("cursor");
                            break;
                        }
                    case "shape":
                        {
                            break;
                        }
                }


                if (autoAlignCenter) // 控制居中
                {
                    if (IsInPPTPresentationMode)
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(60);
                    }
                    else if (Topmost) //非黑板
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(100, true);
                    }
                    else //黑板
                    {
                        await Task.Delay(50);
                        ViewboxFloatingBarMarginAnimation(60);
                    }
                }
            }

            await Task.Delay(150);
            isHidingSubPanelsWhenInking = false;
        }

        #endregion

        #region 撤銷重做按鈕

        /// <summary>
        /// 撤销按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconUndo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("撤销冻结页面内容")) return;
            if (!IsUndoEnabled) return;
            BtnUndo_Click(null, null);
            HideSubPanels();
        }

        /// <summary>
        /// 重做按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconRedo_MouseUp(object sender, RoutedEventArgs e)
        {
            if (TryBlockFrozenPageMutation("重做冻结页面内容")) return;
            if (!IsRedoEnabled) return;
            BtnRedo_Click(null, null);
            HideSubPanels();
        }

        #endregion

        #region 白板按鈕和退出白板模式按鈕

        /// <summary>
        /// 是否正在显示或隐藏黑板
        /// </summary>
        private bool isDisplayingOrHidingBlackboard;

        /// <summary>
        /// 进入白板后的程序性切笔是否需要抑制一次笔设置弹窗。
        /// </summary>
        private bool suppressNextPenPaletteOpen;

        /// <summary>
        /// 白板按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void ImageBlackboard_MouseUp(object sender, MouseButtonEventArgs e)
        {

            LeftUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            RightUnFoldButtonQuickPanel.Visibility = Visibility.Collapsed;
            if (isDisplayingOrHidingBlackboard) return;
            isDisplayingOrHidingBlackboard = true;

            UnFoldFloatingBar_MouseUp(null, null);

            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select) PenIcon_Click(null, null);

            if (currentMode == 0)
            {
                LeftBottomPanelForPPTNavigation.Visibility = Visibility.Collapsed;
                RightBottomPanelForPPTNavigation.Visibility = Visibility.Collapsed;
                LeftSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                RightSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                //進入黑板

                /*
                if (Not_Enter_Blackboard_fir_Mouse_Click) {// BUG-Fixed_tmp：程序启动后直接进入白板会导致后续撤销功能、退出白板无法恢复墨迹
                    BtnColorRed_Click(BorderPenColorRed, null);
                    await Task.Delay(200);
                    SimulateMouseClick.SimulateMouseClickAtTopLeft();
                    await Task.Delay(10);
                    Not_Enter_Blackboard_fir_Mouse_Click = false;
                }
                */
                new Thread(() =>
                {
                    Thread.Sleep(100);
                    Application.Current.Dispatcher.Invoke(() => { ViewboxFloatingBarMarginAnimation(60); });
                }).Start();

                HideSubPanels();

                if (GridTransparencyFakeBackground.Background == Brushes.Transparent)
                {
                    if (currentMode == 1)
                    {
                        currentMode = 0;
                        AutomationBootstrap.Monitor?.NotifyInternalStateChanged();
                        GridBackgroundCover.Visibility = Visibility.Collapsed;
                        AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);
                    }

                    BtnHideInkCanvas_Click(null, null);
                }

                if (Settings.Appearance.EnableTimeDisplayInWhiteboardMode)
                {
                    WaterMarkTime.Visibility = Visibility.Visible;
                    WaterMarkDate.Visibility = Visibility.Visible;
                }
                else
                {
                    WaterMarkTime.Visibility = Visibility.Collapsed;
                    WaterMarkDate.Visibility = Visibility.Collapsed;
                }

                if (Settings.Canvas.UsingWhiteboard)
                {
                    ICCWaterMarkDark.Visibility = Visibility.Visible;
                    ICCWaterMarkWhite.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ICCWaterMarkWhite.Visibility = Visibility.Visible;
                    ICCWaterMarkDark.Visibility = Visibility.Collapsed;
                }

                ViewboxFloatingBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                //关闭黑板
                PauseAllCanvasMediaPlayback();
                HideSubPanelsImmediately();

                // 只有在PPT放映模式下且页数有效时才显示翻页按钮
                if (ArePPTControlsVisible &&
                    IsInPPTPresentationMode &&
                    PPTManager?.IsInSlideShow == true &&
                    PPTManager?.SlidesCount > 0)
                {
                    var dops = Settings.PowerPointSettings.PPTButtonsDisplayOption.ToString();
                    var dopsc = dops.ToCharArray();
                    if (dopsc[0] == '2' && !isDisplayingOrHidingBlackboard) AnimationsHelper.ShowWithFadeIn(LeftBottomPanelForPPTNavigation);
                    if (dopsc[1] == '2' && !isDisplayingOrHidingBlackboard) AnimationsHelper.ShowWithFadeIn(RightBottomPanelForPPTNavigation);
                    if (dopsc[2] == '2' && !isDisplayingOrHidingBlackboard) AnimationsHelper.ShowWithFadeIn(LeftSidePanelForPPTNavigation);
                    if (dopsc[3] == '2' && !isDisplayingOrHidingBlackboard) AnimationsHelper.ShowWithFadeIn(RightSidePanelForPPTNavigation);
                }
                else
                {
                    // 如果不在放映模式或页数无效，隐藏所有翻页按钮
                    LeftBottomPanelForPPTNavigation.Visibility = Visibility.Collapsed;
                    RightBottomPanelForPPTNavigation.Visibility = Visibility.Collapsed;
                    LeftSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                    RightSidePanelForPPTNavigation.Visibility = Visibility.Collapsed;
                }

                // 使用PPT UI管理器来正确更新翻页按钮显示状态，确保遵循用户设置
                _pptUIManager?.UpdateNavigationPanelsVisibility();

                if (Settings.Automation.IsAutoSaveScreenshotAtClear &&
                    inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber) CaptureAndEnqueueScreenshotSave(true);

                if (!IsInPPTPresentationMode)
                    new Thread(() =>
                    {
                        Thread.Sleep(300);
                        Application.Current.Dispatcher.Invoke(() => { ViewboxFloatingBarMarginAnimation(100, true); });
                    }).Start();
                else
                    new Thread(() =>
                    {
                        Thread.Sleep(300);
                        Application.Current.Dispatcher.Invoke(() => { ViewboxFloatingBarMarginAnimation(60); });
                    }).Start();

                if (GetSelectionBGLeft() != 28)
                {
                    // 退出白板是程序内部的状态同步，不应触发笔设置弹窗。
                    suppressNextPenPaletteOpen = true;
                    PenIcon_Click(null, null);
                }

                WaterMarkTime.Visibility = Visibility.Collapsed;
                WaterMarkDate.Visibility = Visibility.Collapsed;
                ICCWaterMarkDark.Visibility = Visibility.Collapsed;
                ICCWaterMarkWhite.Visibility = Visibility.Collapsed;

                // 新增：退出白板模式时恢复基础浮动栏的显示
                ViewboxFloatingBar.Visibility = Visibility.Visible;
            }

            SwitchBackground(null, null);

            if (currentMode == 0)
            {
                // 根据当前编辑模式正确设置工具模式和高光位置
                if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    UpdateCurrentToolMode("cursor");
                    SetFloatingBarHighlightPosition("cursor");
                }
                else if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
                {
                    UpdateCurrentToolMode("pen");
                    SetFloatingBarHighlightPosition("pen");
                }
                else if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint)
                {
                    UpdateCurrentToolMode("eraser");
                    SetFloatingBarHighlightPosition("eraser");
                }
                else if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByStroke)
                {
                    UpdateCurrentToolMode("eraserByStrokes");
                    SetFloatingBarHighlightPosition("eraserByStrokes");
                }
                else if (inkCanvas.EditingMode == InkCanvasEditingMode.Select)
                {
                    UpdateCurrentToolMode("select");
                    SetFloatingBarHighlightPosition("select");
                }
            }

            if (currentMode == 0 && inkCanvas.Strokes.Count == 0 && !IsInPPTPresentationMode)
                CursorIcon_Click(null, null);

            { /* Old UI removed */ }
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;

            new Thread(() =>
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(() => { isDisplayingOrHidingBlackboard = false; });
            }).Start();

            SwitchToDefaultPen(null, null);
            CheckColorTheme(true);

            // 进入白板模式后刷新页码按钮状态：启动时硬编码的禁用色不随主题切换，
            // 需要在此用当前主题的 IconForeground 重新计算上一页按钮的灰色画刷，否则深色主题下图标不可见
            if (currentMode != 0)
            {
                UpdateIndexInfoDisplay();

                // 进入白板模式时显式切换到笔模式：SwitchBackground 不会更新 _currentToolMode，
                // 这里需要复用 PenIcon_Click 的完整切笔逻辑，但应避免被误判为"再次点击笔"而弹出笔设置面板。
                if (inkCanvas.EditingMode != InkCanvasEditingMode.Ink
                    && inkCanvas.EditingMode != InkCanvasEditingMode.Select)
                {
                    suppressNextPenPaletteOpen = true;
                    PenIcon_Click(null, null);
                }
            }
        }

        #endregion
        /// <summary>
        /// 光标图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private async void SymbolIconCursor_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode != 0)
            {
                ImageBlackboard_MouseUp(null, null);
            }
            else
            {
                BtnHideInkCanvas_Click(null, null);

                if (IsInPPTPresentationMode)
                {
                    await Task.Delay(100);
                    ViewboxFloatingBarMarginAnimation(60);
                }
            }
        }

        #region 清空畫布按鈕

        /// <summary>
        /// 清空画布按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconDelete_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("清除冻结页面内容")) return;
            if (inkCanvas.GetSelectedStrokes().Count > 0)
            {
                inkCanvas.Strokes.Remove(inkCanvas.GetSelectedStrokes());
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
            }
            else if (inkCanvas.Strokes.Count > 0)
            {
                if (Settings.Automation.IsAutoSaveScreenshotAtClear &&
                    inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber)
                {
                    if (IsInPPTPresentationMode)
                    {
                        var currentSlide = _pptManager?.GetCurrentSlideNumber() ?? 0;
                        var presentationName = _pptManager?.GetPresentationName() ?? "";
                        CaptureAndEnqueueScreenshotSave(true, $"{presentationName}/{currentSlide}_{DateTime.Now:HH-mm-ss}");
                    }
                    else
                        CaptureAndEnqueueScreenshotSave(true);
                }

                BtnClear_Click(null, null);
            }
        }

        #endregion

        /// <summary>
        /// 面积擦子面板的清空墨迹按钮事件处理
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">RoutedEventArgs</param>
        private void EraserPanelSymbolIconDelete_MouseUp(object sender, RoutedEventArgs e)
        {
            PenIcon_Click(null, null);
            SymbolIconDelete_MouseUp(null, null);
        }

        #region 主要的工具按鈕事件

        /// <summary>
        /// 浮动工具栏的"套索选"按钮事件，重定向到旧UI的BtnSelect_Click方法
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconSelect_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (lastBorderMouseDownObject is Panel panel)
                panel.Background = new SolidColorBrush(Colors.Transparent);

            // 如果当前不在批注模式，先切换到批注模式
            if (!IsAnnotating)
            {
                PenIcon_Click(sender, e);
            }

            BtnSelect_Click(null, null);

            // 更新模式缓存
            UpdateCurrentToolMode("select");

            HideSubPanels("select");

        }

        #endregion

        /// <summary>
        /// 浮动工具栏按钮鼠标按下反馈效果处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void FloatingBarToolBtnMouseDownFeedback_Panel(object sender, MouseButtonEventArgs e)
        {
            if (sender is Panel panel)
            {
                lastBorderMouseDownObject = sender;
                panel.Background = new SolidColorBrush(Color.FromArgb(28, 24, 24, 27));
            }
            else if (sender is Border border)
            {
                lastBorderMouseDownObject = sender;
                if (border.Name?.StartsWith("QuickColor") == true)
                {
                    if (border.Background is SolidColorBrush originalColor)
                    {
                        border.Background = new SolidColorBrush(Color.FromArgb(180, originalColor.Color.R, originalColor.Color.G, originalColor.Color.B));
                    }
                }
                else
                {
                    border.Background = new SolidColorBrush(Color.FromArgb(28, 24, 24, 27));
                }
            }
            else if (sender is Ink_Canvas.Controls.ColorPickerButton colorPicker)
            {
                lastBorderMouseDownObject = sender;
            }
        }

        /// <summary>
        /// 浮动工具栏按钮鼠标离开反馈效果处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void FloatingBarToolBtnMouseLeaveFeedback_Panel(object sender, MouseEventArgs e)
        {
            if (sender is Panel panel)
            {
                lastBorderMouseDownObject = null;
                panel.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (sender is Border border)
            {
                lastBorderMouseDownObject = null;
                // 对于快捷调色板的颜色球，恢复原始颜色
                if (border.Name?.StartsWith("QuickColor") == true)
                {
                    // 根据颜色球名称恢复对应的颜色
                    switch (border.Name)
                    {
                        case "QuickColorWhite":
                        case "QuickColorWhiteSingle":
                            border.Background = new SolidColorBrush(Colors.White);
                            break;
                        case "QuickColorOrange":
                        case "QuickColorOrangeSingle":
                            border.Background = new SolidColorBrush(Color.FromRgb(251, 150, 80));
                            break;
                        case "QuickColorYellow":
                        case "QuickColorYellowSingle":
                            border.Background = new SolidColorBrush(Colors.Yellow);
                            break;
                        case "QuickColorBlack":
                        case "QuickColorBlackSingle":
                            border.Background = new SolidColorBrush(Colors.Black);
                            break;
                        case "QuickColorBlue":
                            border.Background = new SolidColorBrush(Color.FromRgb(37, 99, 235));
                            break;
                        case "QuickColorRed":
                        case "QuickColorRedSingle":
                            border.Background = new SolidColorBrush(Colors.Red);
                            break;
                        case "QuickColorGreen":
                        case "QuickColorGreenSingle":
                            border.Background = new SolidColorBrush(Color.FromRgb(22, 163, 74));
                            break;
                        case "QuickColorPurple":
                            border.Background = new SolidColorBrush(Color.FromRgb(147, 51, 234));
                            break;
                    }
                }
                else
                {
                    border.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            else if (sender is Ink_Canvas.Controls.ColorPickerButton colorPicker)
            {
                lastBorderMouseDownObject = null;
            }
        }

        /// <summary>
        /// 设置图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void SymbolIconSettings_Click(object sender, MouseButtonEventArgs e)
        {
            HideSubPanels();
            BtnSettings_Click(null, null);
        }
        /// <summary>
        /// 截图图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal async void SymbolIconScreenshot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSubPanelsImmediately();
            await Task.Delay(50);

            // 白板模式下默认全屏截图到桌面；其余模式默认调用可选区截图
            if (currentMode == 1)
            {
                SaveScreenShotToDesktop();
            }
            else
            {
                await SaveAreaScreenShotToDesktop();
            }
        }

        /// <summary>
        /// 操作指南窗口图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void OperatingGuideWindowIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
            AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);
            AnimationsHelper.HideWithSlideAndFade(BoardImageOptionsPanel);

            new OperatingGuideWindow().Show();
        }

        /// <summary>
        /// 检查并更新橡皮擦类型标签的状态
        /// </summary>
        public void CheckEraserTypeTab()
        {
            if (EraserTypeTab != null)
                EraserTypeTab.SelectedIndex = Settings.Canvas.EraserShapeType;
            if (BoardEraserTypeTab != null)
                BoardEraserTypeTab.SelectedIndex = Settings.Canvas.EraserShapeType;
        }

        /// <summary>
        /// 工具图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconTools_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (BorderTools.IsOpen || BoardBorderToolsPopup.IsOpen)
            {
                AnimationsHelper.HidePopupWithSlideAndFade(BorderTools);
                AnimationsHelper.HidePopupWithSlideAndFade(BoardBorderToolsPopup);
            }
            else
            {
                HideSubPanels();
                if (currentMode == 0)
                {
                    MainToolsPopupContent?.ApplyMenuLayout();
                    AnimationsHelper.ShowPopupWithSlideAndFade(BorderTools);
                    _popupManager?.BringToFront(BorderTools);
                }
                else
                {
                    BoardToolsPopupContent?.ApplyMenuLayout();
                    AnimationsHelper.ShowPopupWithSlideAndFade(BoardBorderToolsPopup);
                    _popupManager?.BringToFront(BoardBorderToolsPopup);
                }
            }
        }

        private void PlaceFloatingBarAfterHeadToggle(double headLeft, bool isExpanding)
        {
            if (IsVerticalToolbar)
            {
                PlaceFloatingBarAfterHeadToggleVertical(headLeft, isExpanding);
                return;
            }

            var screenWidth = GetFloatingBarScreenWidth(Settings.Advanced.IsEnableAvoidFullScreenHelper);
            ViewboxFloatingBar.UpdateLayout();

            if (!isExpanding)
            {
                var fullCollapsedWidth = GetFloatingBarScaledWidth();
                var collapsedHeadWidth = GetFloatingBarHeadScaledWidth();

                var useFullWidth = fullCollapsedWidth > collapsedHeadWidth * 1.5;
                var collapsedWidth = useFullWidth ? fullCollapsedWidth : collapsedHeadWidth;

                var shouldFlipOnCollapse = !Settings.Appearance.AutoFlipWhenSpaceInsufficient ? isFloatingBarHeadOnRight :
                    (Settings.Appearance.ToolbarPosition == ToolbarPosition.Left
                    ? headLeft - Math.Max(0, collapsedWidth - collapsedHeadWidth) < 0
                    : headLeft + collapsedWidth > screenWidth);
                var wasCollapsedHeadOnRight = isFloatingBarHeadOnRight;
                SetFloatingBarHeadPlacement(shouldFlipOnCollapse);

                ViewboxFloatingBar.UpdateLayout();

                fullCollapsedWidth = GetFloatingBarScaledWidth();
                collapsedHeadWidth = GetFloatingBarHeadScaledWidth();
                useFullWidth = fullCollapsedWidth > collapsedHeadWidth * 1.5;
                collapsedWidth = useFullWidth ? fullCollapsedWidth : collapsedHeadWidth;

                var nextCollapsedLeft = shouldFlipOnCollapse
                    ? headLeft - Math.Max(0, collapsedWidth - collapsedHeadWidth)
                    : headLeft;

                pos.X = ClampFloatingBarLeft(nextCollapsedLeft, collapsedWidth, screenWidth);
                ViewboxFloatingBar.Margin = new Thickness(pos.X, ViewboxFloatingBar.Margin.Top, -2000, -200);

                if (shouldFlipOnCollapse != wasCollapsedHeadOnRight)
                {
                    var actualHeadLeft = ViewboxFloatingBar.Margin.Left + (isFloatingBarHeadOnRight ? Math.Max(0, collapsedWidth - collapsedHeadWidth) : 0);
                    var correction = headLeft - actualHeadLeft;
                    if (Math.Abs(correction) > 0.5)
                    {
                        pos.X += correction;
                        pos.X = ClampFloatingBarLeft(pos.X, collapsedWidth, screenWidth);
                        ViewboxFloatingBar.Margin = new Thickness(pos.X, ViewboxFloatingBar.Margin.Top, -2000, -200);
                    }
                }
                SaveFloatingBarPositionPoint();
                return;
            }

            ViewboxFloatingBar.UpdateLayout();

            var floatingBarWidth = GetFloatingBarScaledWidth();
            var expandedHeadWidth = GetFloatingBarHeadScaledWidth();
            var shouldPlaceToolsOnLeft = !Settings.Appearance.AutoFlipWhenSpaceInsufficient ? isFloatingBarHeadOnRight :
                (Settings.Appearance.ToolbarPosition == ToolbarPosition.Left
                ? headLeft - Math.Max(0, floatingBarWidth - expandedHeadWidth) >= 0
                : headLeft + floatingBarWidth > screenWidth);
            var wasHeadOnRight = isFloatingBarHeadOnRight;

            SetFloatingBarHeadPlacement(shouldPlaceToolsOnLeft);

            ViewboxFloatingBar.UpdateLayout();

            floatingBarWidth = GetFloatingBarScaledWidth();
            expandedHeadWidth = GetFloatingBarHeadScaledWidth();

            var nextLeft = shouldPlaceToolsOnLeft
                ? headLeft - Math.Max(0, floatingBarWidth - expandedHeadWidth)
                : headLeft;

            pos.X = ClampFloatingBarLeft(nextLeft, floatingBarWidth, screenWidth);
            ViewboxFloatingBar.Margin = new Thickness(pos.X, ViewboxFloatingBar.Margin.Top, -2000, -200);

            if (shouldPlaceToolsOnLeft != wasHeadOnRight)
            {
                var actualHeadLeft = ViewboxFloatingBar.Margin.Left + (isFloatingBarHeadOnRight ? Math.Max(0, floatingBarWidth - expandedHeadWidth) : 0);
                var correction = headLeft - actualHeadLeft;
                if (Math.Abs(correction) > 0.5)
                {
                    pos.X += correction;
                    pos.X = ClampFloatingBarLeft(pos.X, GetFloatingBarScaledWidth(), screenWidth);
                    ViewboxFloatingBar.Margin = new Thickness(pos.X, ViewboxFloatingBar.Margin.Top, -2000, -200);
                }
            }
            SaveFloatingBarPositionPoint();
        }

        private void PlaceFloatingBarAfterHeadToggleVertical(double headTop, bool isExpanding)
        {
            var screenHeight = GetFloatingBarScreenHeight(Settings.Advanced.IsEnableAvoidFullScreenHelper);
            ViewboxFloatingBar.UpdateLayout();

            if (!isExpanding)
            {
                var fullCollapsedHeight = GetFloatingBarScaledHeight();
                var collapsedHeadHeight = GetFloatingBarHeadScaledHeight();

                var useFullHeight = fullCollapsedHeight > collapsedHeadHeight * 1.5;
                var collapsedHeight = useFullHeight ? fullCollapsedHeight : collapsedHeadHeight;

                var shouldFlipOnCollapse = !Settings.Appearance.AutoFlipWhenSpaceInsufficient ? isFloatingBarHeadOnBottom :
                    (Settings.Appearance.ToolbarPosition == ToolbarPosition.Top
                    ? headTop - Math.Max(0, collapsedHeight - collapsedHeadHeight) < 0
                    : headTop + collapsedHeight > screenHeight);
                var wasCollapsedHeadOnBottom = isFloatingBarHeadOnBottom;
                SetFloatingBarHeadPlacementVertical(shouldFlipOnCollapse);

                ViewboxFloatingBar.UpdateLayout();

                fullCollapsedHeight = GetFloatingBarScaledHeight();
                collapsedHeadHeight = GetFloatingBarHeadScaledHeight();
                useFullHeight = fullCollapsedHeight > collapsedHeadHeight * 1.5;
                collapsedHeight = useFullHeight ? fullCollapsedHeight : collapsedHeadHeight;

                var nextCollapsedTop = shouldFlipOnCollapse
                    ? headTop - Math.Max(0, collapsedHeight - collapsedHeadHeight)
                    : headTop;

                pos.Y = ClampFloatingBarTop(nextCollapsedTop, collapsedHeight, screenHeight);
                ViewboxFloatingBar.Margin = new Thickness(ViewboxFloatingBar.Margin.Left, pos.Y, -2000, -200);

                if (shouldFlipOnCollapse != wasCollapsedHeadOnBottom)
                {
                    var actualHeadTop = ViewboxFloatingBar.Margin.Top + (isFloatingBarHeadOnBottom ? Math.Max(0, collapsedHeight - collapsedHeadHeight) : 0);
                    var correction = headTop - actualHeadTop;
                    if (Math.Abs(correction) > 0.5)
                    {
                        pos.Y += correction;
                        pos.Y = ClampFloatingBarTop(pos.Y, collapsedHeight, screenHeight);
                        ViewboxFloatingBar.Margin = new Thickness(ViewboxFloatingBar.Margin.Left, pos.Y, -2000, -200);
                    }
                }
                SaveFloatingBarPositionPoint();
                return;
            }

            ViewboxFloatingBar.UpdateLayout();

            var floatingBarHeight = GetFloatingBarScaledHeight();
            var expandedHeadHeight = GetFloatingBarHeadScaledHeight();
            var shouldPlaceToolsOnTop = !Settings.Appearance.AutoFlipWhenSpaceInsufficient ? isFloatingBarHeadOnBottom :
                (Settings.Appearance.ToolbarPosition == ToolbarPosition.Top
                ? headTop - Math.Max(0, floatingBarHeight - expandedHeadHeight) >= 0
                : headTop + floatingBarHeight > screenHeight);
            var wasHeadOnBottom = isFloatingBarHeadOnBottom;

            SetFloatingBarHeadPlacementVertical(shouldPlaceToolsOnTop);

            ViewboxFloatingBar.UpdateLayout();

            floatingBarHeight = GetFloatingBarScaledHeight();
            expandedHeadHeight = GetFloatingBarHeadScaledHeight();

            var nextTop = shouldPlaceToolsOnTop
                ? headTop - Math.Max(0, floatingBarHeight - expandedHeadHeight)
                : headTop;

            pos.Y = ClampFloatingBarTop(nextTop, floatingBarHeight, screenHeight);
            ViewboxFloatingBar.Margin = new Thickness(ViewboxFloatingBar.Margin.Left, pos.Y, -2000, -200);

            if (shouldPlaceToolsOnTop != wasHeadOnBottom)
            {
                var actualHeadTop = ViewboxFloatingBar.Margin.Top + (isFloatingBarHeadOnBottom ? Math.Max(0, floatingBarHeight - expandedHeadHeight) : 0);
                var correction = headTop - actualHeadTop;
                if (Math.Abs(correction) > 0.5)
                {
                    pos.Y += correction;
                    pos.Y = ClampFloatingBarTop(pos.Y, GetFloatingBarScaledHeight(), screenHeight);
                    ViewboxFloatingBar.Margin = new Thickness(ViewboxFloatingBar.Margin.Left, pos.Y, -2000, -200);
                }
            }
            SaveFloatingBarPositionPoint();
        }

        private double NormalizeFloatingBarLeftForScreen(double requestedLeft, double floatingBarWidth,
            double screenWidth)
        {
            if (IsVerticalToolbar)
            {
                return ClampFloatingBarLeft(requestedLeft, floatingBarWidth, screenWidth);
            }

            var headWidth = GetFloatingBarHeadScaledWidth();
            var nextLeft = requestedLeft;
            var shouldPlaceToolsOnLeft = isFloatingBarHeadOnRight;
            var wasHeadOnRight = isFloatingBarHeadOnRight;

            if (Settings.Appearance.AutoFlipWhenSpaceInsufficient)
            {
                var requestedHeadLeft = isFloatingBarHeadOnRight
                    ? requestedLeft + Math.Max(0, floatingBarWidth - headWidth)
                    : requestedLeft;

                if (Settings.Appearance.ToolbarPosition == ToolbarPosition.Right)
                {
                    if (!isFloatingBarHeadOnRight && requestedHeadLeft + floatingBarWidth > screenWidth)
                    {
                        shouldPlaceToolsOnLeft = true;
                        nextLeft = requestedHeadLeft - Math.Max(0, floatingBarWidth - headWidth);
                    }
                    else if (isFloatingBarHeadOnRight && requestedHeadLeft + floatingBarWidth <= screenWidth)
                    {
                        shouldPlaceToolsOnLeft = false;
                    }
                }
                else
                {
                    var toolsLeftWhenUnflipped = requestedHeadLeft - Math.Max(0, floatingBarWidth - headWidth);
                    if (isFloatingBarHeadOnRight && toolsLeftWhenUnflipped < 0)
                    {
                        shouldPlaceToolsOnLeft = false;
                    }
                    else if (!isFloatingBarHeadOnRight && toolsLeftWhenUnflipped >= 0)
                    {
                        shouldPlaceToolsOnLeft = true;
                    }
                }
            }

            SetFloatingBarHeadPlacement(shouldPlaceToolsOnLeft);

            if (shouldPlaceToolsOnLeft != wasHeadOnRight)
            {
                floatingBarWidth = GetFloatingBarScaledWidth();
            }

            return ClampFloatingBarLeft(nextLeft, floatingBarWidth, screenWidth);
        }

        private double NormalizeFloatingBarTopForScreen(double requestedTop, double floatingBarHeight,
            double screenHeight)
        {
            var headHeight = GetFloatingBarHeadScaledHeight();
            var nextTop = requestedTop;
            var shouldPlaceToolsOnTop = isFloatingBarHeadOnBottom;
            var wasHeadOnBottom = isFloatingBarHeadOnBottom;

            if (Settings.Appearance.AutoFlipWhenSpaceInsufficient)
            {
                var requestedHeadTop = isFloatingBarHeadOnBottom
                    ? requestedTop + Math.Max(0, floatingBarHeight - headHeight)
                    : requestedTop;

                if (Settings.Appearance.ToolbarPosition == ToolbarPosition.Bottom)
                {
                    if (!isFloatingBarHeadOnBottom && requestedHeadTop + floatingBarHeight > screenHeight)
                    {
                        shouldPlaceToolsOnTop = true;
                        nextTop = requestedHeadTop - Math.Max(0, floatingBarHeight - headHeight);
                    }
                    else if (isFloatingBarHeadOnBottom && requestedHeadTop + floatingBarHeight <= screenHeight)
                    {
                        shouldPlaceToolsOnTop = false;
                    }
                }
                else
                {
                    var toolsTopWhenUnflipped = requestedHeadTop - Math.Max(0, floatingBarHeight - headHeight);
                    if (isFloatingBarHeadOnBottom && toolsTopWhenUnflipped < 0)
                    {
                        shouldPlaceToolsOnTop = false;
                    }
                    else if (!isFloatingBarHeadOnBottom && toolsTopWhenUnflipped >= 0)
                    {
                        shouldPlaceToolsOnTop = true;
                    }
                }
            }

            SetFloatingBarHeadPlacementVertical(shouldPlaceToolsOnTop);

            if (shouldPlaceToolsOnTop != wasHeadOnBottom)
            {
                floatingBarHeight = GetFloatingBarScaledHeight();
            }

            return ClampFloatingBarTop(nextTop, floatingBarHeight, screenHeight);
        }

        private double GetCurrentFloatingBarHeadLeft()
        {
            if (IsVerticalToolbar)
            {
                return ViewboxFloatingBar.Margin.Left;
            }

            var floatingBarWidth = GetFloatingBarScaledWidth();
            var headWidth = GetFloatingBarHeadScaledWidth();
            var headOffset = isFloatingBarHeadOnRight
                ? Math.Max(0, floatingBarWidth - headWidth)
                : 0;
            return ViewboxFloatingBar.Margin.Left + headOffset;
        }

        private double GetCurrentFloatingBarHeadTop()
        {
            if (!IsVerticalToolbar)
            {
                return ViewboxFloatingBar.Margin.Top;
            }

            var floatingBarHeight = GetFloatingBarScaledHeight();
            var headHeight = GetFloatingBarHeadScaledHeight();
            var headOffset = isFloatingBarHeadOnBottom
                ? Math.Max(0, floatingBarHeight - headHeight)
                : 0;
            return ViewboxFloatingBar.Margin.Top + headOffset;
        }

        private void SaveFloatingBarPositionPoint()
        {
            var currentPoint = new Point(ViewboxFloatingBar.Margin.Left, ViewboxFloatingBar.Margin.Top);
            if (IsInPPTPresentationMode)
                pointPPT = currentPoint;
            else
                pointDesktop = currentPoint;
        }

        /// <summary>
        /// 浮动工具栏边距动画处理
        /// </summary>
        /// <param name="MarginFromEdge">边缘边距</param>
        /// <param name="PosXCaculatedWithTaskbarHeight">是否考虑任务栏高度计算位置</param>
        /// <param name="skipAnimation">是否跳过动画直接定位（用于启动时快速恢复位置）</param>
        public async void ViewboxFloatingBarMarginAnimation(int MarginFromEdge,
            bool PosXCaculatedWithTaskbarHeight = false, bool skipAnimation = false)
        {
            if (currentMode == 1)
            {
                return;
            }

            if (MarginFromEdge == 60) MarginFromEdge = 55;

            await Dispatcher.InvokeAsync(() =>
            {
                if (skipAnimation)
                {
                    ViewboxFloatingBarMarginAnimationCore(MarginFromEdge, PosXCaculatedWithTaskbarHeight, false);
                    return;
                }

                ViewboxFloatingBarMarginAnimationCore(MarginFromEdge, PosXCaculatedWithTaskbarHeight, true);
            });

            await Task.Delay(skipAnimation ? 0 : 200);

            await Dispatcher.InvokeAsync(() =>
            {
                ViewboxFloatingBar.Margin = new Thickness(pos.X, pos.Y, -2000, -200);
                ViewboxFloatingBar.BeginAnimation(MarginProperty, null);
                isViewboxFloatingBarMarginAnimationRunning = false;
                if (!Topmost) ViewboxFloatingBar.Visibility = Visibility.Hidden;

                if (!string.IsNullOrEmpty(_currentToolMode))
                {
                    SetFloatingBarHighlightPosition(_currentToolMode);
                }

                // Issue #285：动画会重新显示完整浮动栏，若此时应处于迷你栏状态则重新接管
                RefreshIdleMiniBarState();
            });
        }

        private void ViewboxFloatingBarMarginAnimationCore(int MarginFromEdge,
            bool PosXCaculatedWithTaskbarHeight = false, bool animate = false)
        {
            if (Topmost)
            {
                ViewboxFloatingBar.Visibility = Visibility.Visible;
                ViewboxFloatingBar.UpdateLayout();
            }
            isViewboxFloatingBarMarginAnimationRunning = true;

            double dpiScaleX = 1, dpiScaleY = 1;
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            var screen = GetFloatingBarTargetScreen();
            double screenWidth, screenHeight;
            double toolbarHeight;
            double screenBoundsWidth = screen.Bounds.Width / dpiScaleX;
            double screenBoundsHeight = screen.Bounds.Height / dpiScaleY;
            if (Settings.Advanced.IsEnableAvoidFullScreenHelper && PosXCaculatedWithTaskbarHeight)
            {
                screenWidth = screen.WorkingArea.Width / dpiScaleX;
                screenHeight = screen.WorkingArea.Height / dpiScaleY;
                toolbarHeight = 0;
            }
            else
            {
                screenWidth = screen.Bounds.Width / dpiScaleX;
                screenHeight = screen.Bounds.Height / dpiScaleY;
                toolbarHeight = ForegroundWindowInfo.GetTaskbarHeight(screen, dpiScaleY);
            }

            // 非置顶时使用 rcWork 获取的任务栏高度，确保浮动栏完全隐藏到屏幕底部以下
            if (!Topmost)
                MarginFromEdge = -60 - (int)Math.Round(toolbarHeight);

            double baseWidth = ViewboxFloatingBar.ActualWidth;

            if (baseWidth <= 0)
            {
                baseWidth = ViewboxFloatingBar.DesiredSize.Width;
            }

            if (baseWidth <= 0)
            {
                baseWidth = ViewboxFloatingBar.RenderSize.Width;
            }

            if (baseWidth <= 0)
            {
                baseWidth = 200;
                LogHelper.WriteLogToFile($"浮动栏宽度无法获取，使用估算值: {baseWidth}");
            }

            double floatingBarWidth = baseWidth * ViewboxFloatingBarScaleTransform.ScaleX;

            double baseHeight = ViewboxFloatingBar.ActualHeight;
            if (baseHeight <= 0)
            {
                baseHeight = ViewboxFloatingBar.DesiredSize.Height;
            }
            if (baseHeight <= 0)
            {
                baseHeight = ViewboxFloatingBar.RenderSize.Height;
            }
            if (baseHeight <= 0)
            {
                baseHeight = 58;
            }
            double floatingBarHeight = baseHeight * ViewboxFloatingBarScaleTransform.ScaleY;


            if (!IsVerticalToolbar && QuickColorPalette != null &&
                (QuickColorPalette.QuickColorPalettePanel.Visibility == Visibility.Visible ||
                 QuickColorPalette.QuickColorPaletteSingleRowPanel.Visibility == Visibility.Visible))
            {
                if (Settings.Appearance.QuickColorPaletteDisplayMode == 0)
                {
                    floatingBarWidth = Math.Max(floatingBarWidth, 120 * ViewboxFloatingBarScaleTransform.ScaleX);
                }
                else
                {
                    floatingBarWidth = Math.Max(floatingBarWidth, 68 * ViewboxFloatingBarScaleTransform.ScaleX);
                }
            }

            pos.X = (screenWidth - floatingBarWidth) / 2;

            if (IsVerticalToolbar)
            {
                pos.Y = (screenHeight - floatingBarHeight) / 2;
            }
            else
            {
                if (!PosXCaculatedWithTaskbarHeight)
                {
                    if (toolbarHeight == 0)
                    {
                        pos.Y = screenHeight - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleY;
                    }
                    else
                    {
                        pos.Y = screenHeight - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleY - toolbarHeight;
                    }

                    baseWidth = GetElementWidthForFloatingBar(ViewboxFloatingBar, 200);
                    if (baseWidth <= 0)
                    {
                        pos.Y = screenHeight - floatingBarHeight -
                               3 * ViewboxFloatingBarScaleTransform.ScaleY;
                    }
                    floatingBarWidth = baseWidth * ViewboxFloatingBarScaleTransform.ScaleX;

                    baseHeight = GetElementHeightForFloatingBar(ViewboxFloatingBar, 58);
                    if (baseHeight <= 0)
                    {
                        baseHeight = 58;
                    }
                }
            }

            if (MarginFromEdge > -60)
            {
                if (!IsVerticalToolbar && QuickColorPalette?.Visibility == Visibility.Visible)
                {
                    if (Settings.Appearance.QuickColorPaletteDisplayMode == 0)
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 200 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                    else
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 108 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                }

                var toolbarPosition = Settings.Appearance.ToolbarPosition;
                switch (toolbarPosition)
                {
                    case ToolbarPosition.Right:
                    case ToolbarPosition.Left:
                        pos.X = (screenWidth - floatingBarWidth) / 2;
                        if (toolbarHeight == 0)
                        {
                            pos.Y = screenHeight - floatingBarHeight -
                                   3 * ViewboxFloatingBarScaleTransform.ScaleY;
                        }
                        else
                        {
                            pos.Y = screenHeight - floatingBarHeight -
                                   toolbarHeight - ViewboxFloatingBarScaleTransform.ScaleY * 3;
                        }
                        break;

                    case ToolbarPosition.Top:
                    case ToolbarPosition.Bottom:
                        pos.X = screenBoundsWidth - floatingBarWidth -
                               3 * ViewboxFloatingBarScaleTransform.ScaleX;
                        pos.Y = (screenHeight - floatingBarHeight) / 2;
                        break;
                }

                if (MarginFromEdge < 0)
                {
                    if (IsVerticalToolbar)
                        pos.Y = screenHeight - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleY;
                    else
                        pos.X = screenWidth - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleX;
                }
                else if (IsInPPTPresentationMode)
                {
                    switch (toolbarPosition)
                    {
                        case ToolbarPosition.Right:
                        case ToolbarPosition.Left:
                            pos.X = (screenWidth - floatingBarWidth) / 2;
                            pos.Y = screenHeight - floatingBarHeight +
                                   2 * ViewboxFloatingBarScaleTransform.ScaleY;
                            break;

                        case ToolbarPosition.Top:
                        case ToolbarPosition.Bottom:
                            pos.X = screenBoundsWidth - floatingBarWidth -
                                   3 * ViewboxFloatingBarScaleTransform.ScaleX;
                            pos.Y = (screenHeight - floatingBarHeight) / 2;
                            break;
                    }
                }

                if (IsVerticalToolbar)
                {
                    pos.Y = NormalizeFloatingBarTopForScreen(pos.Y, floatingBarHeight, screenHeight);
                }
                else
                {
                    pos.X = NormalizeFloatingBarLeftForScreen(pos.X, floatingBarWidth, screenWidth);
                }

                if (IsInPPTPresentationMode)
                {
                    if (pointPPT.X != -1 || pointPPT.Y != -1)
                    {
                        if (Math.Abs(pointPPT.Y - pos.Y) > 50)
                            pos = pointPPT;
                        else
                            pointPPT = pos;
                    }
                }
                else
                {
                    if (pointDesktop.X != -1 || pointDesktop.Y != -1)
                    {
                        if (Math.Abs(pointDesktop.Y - pos.Y) > 50)
                            pos = pointDesktop;
                        else
                            pointDesktop = pos;
                    }
                }
            }
            else if (IsVerticalToolbar)
            {
                pos.X = screenWidth - MarginFromEdge * ViewboxFloatingBarScaleTransform.ScaleX;
            }

            if (animate)
            {
                var marginAnimation = new ThicknessAnimation
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    From = ViewboxFloatingBar.Margin,
                    To = new Thickness(pos.X, pos.Y, 0, -20),
                    EasingFunction = new CircleEase()
                };
                ViewboxFloatingBar.BeginAnimation(MarginProperty, marginAnimation);
            }
            else
            {
                ViewboxFloatingBar.Margin = new Thickness(pos.X, pos.Y, 0, -20);
            }

            if (!animate) isViewboxFloatingBarMarginAnimationRunning = false;
            if (!Topmost) ViewboxFloatingBar.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 桌面模式下的浮动工具栏边距动画处理
        /// </summary>
        public async void PureViewboxFloatingBarMarginAnimationInDesktopMode()
        {
            // 在白板模式下不执行浮动栏动画
            if (currentMode == 1)
            {
                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                ViewboxFloatingBar.Visibility = Visibility.Visible;
                ViewboxFloatingBar.UpdateLayout();
                isViewboxFloatingBarMarginAnimationRunning = true;

                double dpiScaleX = 1, dpiScaleY = 1;
                var source = PresentationSource.FromVisual(this);
                if (source != null)
                {
                    dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                }

                var screen = GetFloatingBarTargetScreen();
                double screenWidth, screenHeight;
                double toolbarHeight;
                double screenBoundsWidth = screen.Bounds.Width / dpiScaleX;
                if (Settings.Advanced.IsEnableAvoidFullScreenHelper)
                {
                    screenWidth = screen.WorkingArea.Width / dpiScaleX;
                    screenHeight = screen.WorkingArea.Height / dpiScaleY;
                    toolbarHeight = 0;
                }
                else
                {
                    screenWidth = screen.Bounds.Width / dpiScaleX;
                    screenHeight = screen.Bounds.Height / dpiScaleY;
                    toolbarHeight = ForegroundWindowInfo.GetTaskbarHeight(screen, dpiScaleY);
                }

                double baseWidth = GetElementWidthForFloatingBar(ViewboxFloatingBar, 200);
                if (baseWidth <= 0)
                {
                    baseWidth = 200;
                    LogHelper.WriteLogToFile($"浮动栏宽度无法获取，使用估算值: {baseWidth}");
                }
                double floatingBarWidth = baseWidth * ViewboxFloatingBarScaleTransform.ScaleX;

                double baseHeight = GetElementHeightForFloatingBar(ViewboxFloatingBar, 58);
                if (baseHeight <= 0)
                {
                    baseHeight = 58;
                }
                double floatingBarHeight = baseHeight * ViewboxFloatingBarScaleTransform.ScaleY;


                if (!IsVerticalToolbar && QuickColorPalette?.Visibility == Visibility.Visible)
                {
                    if (Settings.Appearance.QuickColorPaletteDisplayMode == 0)
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 140 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                    else
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 86 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                }

                var toolbarPosition = Settings.Appearance.ToolbarPosition;
                switch (toolbarPosition)
                {
                    case ToolbarPosition.Right:
                    case ToolbarPosition.Left:
                        pos.X = (screenWidth - floatingBarWidth) / 2;
                        if (toolbarHeight == 0)
                        {
                            pos.Y = screenHeight - floatingBarHeight -
                                   3 * ViewboxFloatingBarScaleTransform.ScaleY;
                        }
                        else
                        {
                            pos.Y = screenHeight - floatingBarHeight -
                                   toolbarHeight - ViewboxFloatingBarScaleTransform.ScaleY * 3;
                        }
                        break;

                    case ToolbarPosition.Top:
                    case ToolbarPosition.Bottom:
                        pos.X = screenBoundsWidth - floatingBarWidth -
                               3 * ViewboxFloatingBarScaleTransform.ScaleX;
                        pos.Y = (screenHeight - floatingBarHeight) / 2;
                        break;
                }

                if (IsVerticalToolbar)
                {
                    pos.Y = NormalizeFloatingBarTopForScreen(pos.Y, floatingBarHeight, screenHeight);
                }
                else
                {
                    pos.X = NormalizeFloatingBarLeftForScreen(pos.X, floatingBarWidth, screenWidth);
                }

                if (pointDesktop.X != -1 || pointDesktop.Y != -1)
                {
                    if (Math.Abs(pointDesktop.Y - pos.Y) > 50)
                        pos = pointDesktop;
                    else
                        pointDesktop = pos;
                }

                var marginAnimation = new ThicknessAnimation
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    From = ViewboxFloatingBar.Margin,
                    To = new Thickness(pos.X, pos.Y, 0, -20),
                    EasingFunction = new CircleEase()
                };
                ViewboxFloatingBar.BeginAnimation(MarginProperty, marginAnimation);
            });

            await Task.Delay(349);

            await Dispatcher.InvokeAsync(() =>
            {
                ViewboxFloatingBar.Margin = new Thickness(pos.X, pos.Y, -2000, -200);

                if (!string.IsNullOrEmpty(_currentToolMode))
                {
                    SetFloatingBarHighlightPosition(_currentToolMode);
                }
            });
        }

        /// <summary>
        /// PPT模式下的浮动工具栏边距动画处理
        /// </summary>
        /// <param name="isRetry">是否为重试操作</param>
        public async void PureViewboxFloatingBarMarginAnimationInPPTMode(bool isRetry = false)
        {
            // 新增：在白板模式下不执行浮动栏动画
            if (currentMode == 1)
            {
                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                ViewboxFloatingBar.Visibility = Visibility.Visible;
                ViewboxFloatingBar.UpdateLayout();
                isViewboxFloatingBarMarginAnimationRunning = true;

                double dpiScaleX = 1, dpiScaleY = 1;
                var source = PresentationSource.FromVisual(this);
                if (source != null)
                {
                    dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                }

                var screen = GetFloatingBarTargetScreen();
                double screenWidth = screen.Bounds.Width / dpiScaleX, screenHeight = screen.Bounds.Height / dpiScaleY;
                // 仅计算Windows任务栏高度，不考虑其他程序对工作区的影响
                var toolbarHeight = ForegroundWindowInfo.GetTaskbarHeight(screen, dpiScaleY);

                // 计算浮动栏位置，考虑快捷调色盘的显示状态
                // 使用更可靠的方法获取浮动栏宽度
                double baseWidth = GetElementWidthForFloatingBar(ViewboxFloatingBar, 200);
                if (baseWidth <= 0)
                {
                    baseWidth = 200;
                    LogHelper.WriteLogToFile($"浮动栏宽度无法获取，使用估算值: {baseWidth}");
                }
                double floatingBarWidth = baseWidth * ViewboxFloatingBarScaleTransform.ScaleX;

                double baseHeight = GetElementHeightForFloatingBar(ViewboxFloatingBar, 58);
                if (baseHeight <= 0) baseHeight = 58;
                double floatingBarHeight = baseHeight * ViewboxFloatingBarScaleTransform.ScaleY;


                if (!IsVerticalToolbar && QuickColorPalette?.Visibility == Visibility.Visible)
                {
                    if (Settings.Appearance.QuickColorPaletteDisplayMode == 0)
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 140 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                    else
                    {
                        floatingBarWidth = Math.Max(floatingBarWidth, 86 * ViewboxFloatingBarScaleTransform.ScaleX);
                    }
                }

                var toolbarPosition = Settings.Appearance.ToolbarPosition;
                switch (toolbarPosition)
                {
                    case ToolbarPosition.Right:
                    case ToolbarPosition.Left:
                        pos.X = (screenWidth - floatingBarWidth) / 2;
                        pos.Y = screenHeight - floatingBarHeight +
                               2 * ViewboxFloatingBarScaleTransform.ScaleY;
                        break;

                    case ToolbarPosition.Top:
                    case ToolbarPosition.Bottom:
                        pos.X = screenWidth - floatingBarWidth -
                               3 * ViewboxFloatingBarScaleTransform.ScaleX;
                        pos.Y = (screenHeight - floatingBarHeight) / 2;
                        break;
                }

                if (IsVerticalToolbar)
                {
                    pos.Y = NormalizeFloatingBarTopForScreen(pos.Y, floatingBarHeight, screenHeight);
                }
                else
                {
                    pos.X = NormalizeFloatingBarLeftForScreen(pos.X, floatingBarWidth, screenWidth);
                }

                if (pointPPT.X != -1 || pointPPT.Y != -1)
                {
                    if (Math.Abs(pointPPT.Y - pos.Y) > 50)
                        pos = pointPPT;
                    else
                        pointPPT = pos;
                }

                var marginAnimation = new ThicknessAnimation
                {
                    Duration = TimeSpan.FromSeconds(0.35),
                    From = ViewboxFloatingBar.Margin,
                    To = new Thickness(pos.X, pos.Y, 0, -20),
                    EasingFunction = new CircleEase()
                };
                ViewboxFloatingBar.BeginAnimation(MarginProperty, marginAnimation);
            });

            await Task.Delay(349);

            await Dispatcher.InvokeAsync(() =>
            {
                ViewboxFloatingBar.Margin = new Thickness(pos.X, pos.Y, -2000, -200);

                if (!string.IsNullOrEmpty(_currentToolMode))
                {
                    SetFloatingBarHighlightPosition(_currentToolMode);
                }
            });

            if (Settings.ModeSettings.IsPPTOnlyMode && !isRetry)
            {
                await Task.Delay(2000); // 等待动画完成后再检查

                bool isFloatingBarVisible = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    // 检查浮动栏是否真的显示了
                    isFloatingBarVisible = ViewboxFloatingBar.Visibility == Visibility.Visible &&
                                          ViewboxFloatingBar.Margin.Left >= 0 &&
                                          ViewboxFloatingBar.Margin.Top >= 0;
                });

                if (!isFloatingBarVisible)
                {
                    PureViewboxFloatingBarMarginAnimationInPPTMode(true);
                }
            }
        }

        private Screen GetFloatingBarTargetScreen()
        {
            try
            {
                if (Settings.Advanced.EnableMultiScreenSupport &&
                    Settings.Advanced.FollowMouseForScreenSelection &&
                    ScreenDetectionHelper.HasMultipleScreens())
                {
                    var mouseScreen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
                    if (mouseScreen != null)
                    {
                        return mouseScreen;
                    }
                }

                var windowHandle = new WindowInteropHelper(this).Handle;
                return Screen.FromHandle(windowHandle);
            }
            catch
            {
                return Screen.PrimaryScreen;
            }
        }

        private Screen GetCurrentFloatingBarScreen()
        {
            try
            {
                if (ViewboxFloatingBar == null || !IsLoaded)
                {
                    return null;
                }

                var center = ViewboxFloatingBar.PointToScreen(new Point(
                    Math.Max(0, ViewboxFloatingBar.ActualWidth / 2),
                    Math.Max(0, ViewboxFloatingBar.ActualHeight / 2)));
                return Screen.FromPoint(new System.Drawing.Point((int)center.X, (int)center.Y));
            }
            catch
            {
                return null;
            }
        }

        internal void RefreshFloatingBarScreenFollowState()
        {
            try
            {
                var enableFollow = Settings.Advanced.EnableMultiScreenSupport &&
                                   Settings.Advanced.FollowMouseForScreenSelection &&
                                   ScreenDetectionHelper.HasMultipleScreens();

                if (!enableFollow)
                {
                    _floatingBarScreenFollowTimer?.Stop();
                    _lastFloatingBarScreenDeviceName = null;
                    return;
                }

                if (_floatingBarScreenFollowTimer == null)
                {
                    _floatingBarScreenFollowTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(350)
                    };
                    _floatingBarScreenFollowTimer.Tick += FloatingBarScreenFollowTimer_Tick;
                }

                _lastFloatingBarScreenDeviceName = GetCurrentFloatingBarScreen()?.DeviceName;
                _lastCanvasScreenDeviceName = _lastFloatingBarScreenDeviceName;

                if (!_floatingBarScreenFollowTimer.IsEnabled)
                {
                    _floatingBarScreenFollowTimer.Start();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"刷新浮动栏多屏跟随状态失败: {ex.Message}", LogHelper.LogType.Warning);
            }
        }

        private void FloatingBarScreenFollowTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!Settings.Advanced.EnableMultiScreenSupport ||
                    !Settings.Advanced.FollowMouseForScreenSelection ||
                    !ScreenDetectionHelper.HasMultipleScreens())
                {
                    _floatingBarScreenFollowTimer?.Stop();
                    _lastFloatingBarScreenDeviceName = null;
                    return;
                }

                if (currentMode == 1 || isFloatingBarFolded || isFloatingBarChangingHideMode ||
                    isDragDropInEffect || ViewboxFloatingBar.Visibility != Visibility.Visible)
                {
                    return;
                }

                var mouseScreen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
                var currentFloatingBarScreen = GetCurrentFloatingBarScreen();

                if (mouseScreen == null || currentFloatingBarScreen == null)
                {
                    return;
                }

                if (mouseScreen.DeviceName == currentFloatingBarScreen.DeviceName)
                {
                    _lastFloatingBarScreenDeviceName = currentFloatingBarScreen.DeviceName;
                    return;
                }

                if (mouseScreen.DeviceName == _lastFloatingBarScreenDeviceName)
                {
                    return;
                }

                _lastFloatingBarScreenDeviceName = mouseScreen.DeviceName;
                RebuildCanvasOnTargetScreen(mouseScreen);

                if (IsInPPTPresentationMode)
                {
                    PureViewboxFloatingBarMarginAnimationInPPTMode();
                }
                else
                {
                    PureViewboxFloatingBarMarginAnimationInDesktopMode();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"浮动栏跨屏跟随失败: {ex.Message}", LogHelper.LogType.Warning);
            }
        }

        private void RebuildCanvasOnTargetScreen(Screen targetScreen)
        {
            try
            {
                if (targetScreen == null || _isRebuildingCanvasForScreen)
                {
                    return;
                }

                if (_lastCanvasScreenDeviceName == targetScreen.DeviceName)
                {
                    return;
                }

                _isRebuildingCanvasForScreen = true;

                double dpiScaleX = 1, dpiScaleY = 1;
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                    dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
                }

                // 先移动主窗口到目标屏，确保画布承载区域切换到新屏幕。
                MainWindow.MoveWindow(
                    new WindowInteropHelper(this).Handle,
                    targetScreen.Bounds.X,
                    targetScreen.Bounds.Y,
                    targetScreen.Bounds.Width,
                    targetScreen.Bounds.Height,
                    true);

                // 重新铺设画布尺寸，强制触发布局刷新。
                inkCanvas.Width = targetScreen.Bounds.Width / dpiScaleX;
                inkCanvas.Height = targetScreen.Bounds.Height / dpiScaleY;
                inkCanvas.InvalidateMeasure();
                inkCanvas.InvalidateArrange();
                inkCanvas.UpdateLayout();

                if (GridInkCanvasSelectionCover != null)
                {
                    GridInkCanvasSelectionCover.Width = inkCanvas.Width;
                    GridInkCanvasSelectionCover.Height = inkCanvas.Height;
                    GridInkCanvasSelectionCover.InvalidateMeasure();
                    GridInkCanvasSelectionCover.InvalidateArrange();
                }

                _lastCanvasScreenDeviceName = targetScreen.DeviceName;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"在新屏重建画布失败: {ex.Message}", LogHelper.LogType.Warning);
            }
            finally
            {
                _isRebuildingCanvasForScreen = false;
            }
        }

        /// <summary>
        /// 光标图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal async void CursorIcon_Click(object sender, MouseButtonEventArgs e)
        {
            if (lastBorderMouseDownObject is Panel panel)
                panel.Background = new SolidColorBrush(Colors.Transparent);

            // 禁用高级橡皮擦系统
            DisableEraserOverlay();
            SetCurrentToolMode(InkCanvasEditingMode.None);

            UpdateCurrentToolMode("cursor");

            SetFloatingBarHighlightPosition("cursor");

            // 切换前自动截图保存墨迹
            if (inkCanvas.Strokes.Count > 0 &&
                inkCanvas.Strokes.Count > Settings.Automation.MinimumAutomationStrokeNumber)
            {
                if (IsInPPTPresentationMode)
                {
                    var currentSlide = _pptManager?.GetCurrentSlideNumber() ?? 0;
                    var presentationName = _pptManager?.GetPresentationName() ?? "";
                    CaptureAndEnqueueScreenshotSave(true, $"{presentationName}/{currentSlide}_{DateTime.Now:HH-mm-ss}");
                }
                else CaptureAndEnqueueScreenshotSave(true);
            }

            if (!IsInPPTPresentationMode)
            {
                if (Settings.Canvas.HideStrokeWhenSelecting)
                {
                    inkCanvas.Visibility = Visibility.Collapsed;
                }
                else
                {
                    inkCanvas.IsHitTestVisible = false;
                    inkCanvas.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint)
                {
                    inkCanvas.Visibility = Visibility.Visible;
                    inkCanvas.IsHitTestVisible = true;
                }
                else
                {
                    if (Settings.Canvas.HideStrokeWhenSelecting)
                    {
                        inkCanvas.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        inkCanvas.IsHitTestVisible = false;
                        inkCanvas.Visibility = Visibility.Visible;
                    }
                }
            }

            GridTransparencyFakeBackground.Opacity = 0;
            GridTransparencyFakeBackground.Background = Brushes.Transparent;
            SetTransparentHitThrough();

            GridBackgroundCoverHolder.Visibility = Visibility.Collapsed;

            // 点击鼠标按钮退出批注模式时的全屏还原
            RestoreFullScreenOnExitAnnotationMode();

            inkCanvas.Select(new StrokeCollection());
            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            if (currentMode != 0)
            {
                SaveStrokes();
                RestoreStrokes(true);
            }

            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
            { /* Old UI removed */ }
            else
            { /* Old UI removed */ }

            { /* Old UI removed */ }
            { /* Old UI removed */ }
            UpdateToolbarComponentVisibility();


            UpdateToolbarComponentVisibility();

            // 注意：快捷调色盘的可见性现在完全由工具栏规则集管理，不需要手动设置

            if (!isFloatingBarFolded)
            {
                HideSubPanels("cursor", true);
                await Task.Delay(50);

                if (IsInPPTPresentationMode)
                    ViewboxFloatingBarMarginAnimation(60);
                else
                    ViewboxFloatingBarMarginAnimation(100, true);
            }
        }

        /// <summary>
        /// 画笔图标点击事件处理，用于切换到批注模式或显示画笔调色盘
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal void PenIcon_Click(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到画笔")) return;

            EndBoardRoaming();

            if (lastBorderMouseDownObject is Panel panel)
                panel.Background = new SolidColorBrush(Colors.Transparent);

            // 如果当前有选中的图片元素，先取消选中
            if (currentSelectedElement != null)
            {
                UnselectElement(currentSelectedElement);
                currentSelectedElement = null;
            }

            // 禁用高级橡皮擦系统
            DisableEraserOverlay();

            // 停止橡皮擦自动切换计时器（如果正在运行）
            StopEraserAutoSwitchBackTimer();

            bool isRealtimePenState = inkCanvas.EditingMode == InkCanvasEditingMode.None
                                      && ShouldUseRealtimeVelocityBrushTip()
                                      && string.Equals(GetCurrentSelectedMode(), "pen", StringComparison.OrdinalIgnoreCase);
            bool wasInInkMode = inkCanvas.EditingMode == InkCanvasEditingMode.Ink
                                || isRealtimePenState
                                || (Pen_Icon.Background != null
                                    && IsAnnotating
                                    && string.Equals(GetCurrentSelectedMode(), "pen", StringComparison.OrdinalIgnoreCase));
            bool wasHighlighter = drawingAttributes.IsHighlighter;

            if (drawingShapeMode != 0 && !isLongPressSelected)
            {
                return;
            }

            if (Pen_Icon.Background == null || !IsAnnotating)
            {
                if (isLongPressSelected)
                {
                    drawingShapeMode = 0;
                    isLongPressSelected = false;
                }

                // 使用集中化的工具模式切换方法
                SetCurrentToolMode(InkCanvasEditingMode.Ink);

                // 更新模式缓存
                UpdateCurrentToolMode("pen");

                GridTransparencyFakeBackground.Opacity = 1;
                GridTransparencyFakeBackground.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
                SetTransparentNotHitThrough();

                inkCanvas.IsHitTestVisible = true;
                inkCanvas.Visibility = Visibility.Visible;

                GridBackgroundCoverHolder.Visibility = Visibility.Visible;
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

                /*if (forceEraser && currentMode == 0)
                    BtnColorRed_Click(sender, null);*/

                if (GridBackgroundCover.Visibility == Visibility.Collapsed)
                {
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                    { /* Old UI removed */ }
                    else
                    { /* Old UI removed */ }
                    { /* Old UI removed */ }
                }
                else
                {
                    { /* Old UI removed */ }
                    { /* Old UI removed */ }
                }

                { /* Old UI removed */ }

                // 进入批注模式时的全屏处理（仅当未应用过全屏处理时）
                if (Settings.Advanced.IsEnableAvoidFullScreenHelper && !isFullScreenApplied)
                {
                    // 设置为画板模式，允许全屏操作
                    AvoidFullScreenHelper.SetBoardMode(true);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MainWindow.MoveWindow(new WindowInteropHelper(this).Handle, 0, 0,
                            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, true);
                    }), DispatcherPriority.ApplicationIdle);

                    isFullScreenApplied = true; // 标记已应用全屏处理
                }

                UpdateToolbarComponentVisibility();
                // 使用集中化的工具模式切换方法
                SetCurrentToolMode(InkCanvasEditingMode.Ink);

                UpdateCurrentToolMode("pen");

                // 注意：快捷调色盘的可见性和显示模式现在完全由工具栏系统管理
                // 不需要手动设置，UpdateToolbarComponentVisibility 会处理好

                SetFloatingBarHighlightPosition("pen");

                UpdateBoardPenIconColor();

                forceEraser = false;
                forcePointEraser = false;
                drawingShapeMode = 0;

                // 保持之前的笔类型状态，而不是强制重置
                if (!wasHighlighter && penType != 2)
                {
                    penType = 0;
                    drawingAttributes.IsHighlighter = false;
                    drawingAttributes.StylusTip = StylusTip.Ellipse;
                    Settings.Canvas.EnableInkFade = false;
                    if (_inkFadeManager != null)
                        _inkFadeManager.IsEnabled = false;
                }
                else if (penType == 1)
                {
                    drawingAttributes.IsHighlighter = !Settings.Canvas.HighlighterOverlapEnabled;
                    drawingAttributes.StylusTip = StylusTip.Rectangle;
                    drawingAttributes.Width = Settings.Canvas.HighlighterWidth / 2;
                    drawingAttributes.Height = Settings.Canvas.HighlighterWidth;
                    Settings.Canvas.EnableInkFade = false;
                    if (_inkFadeManager != null)
                        _inkFadeManager.IsEnabled = false;
                }
                // 如果之前是激光笔模式，则保持激光笔属性
                else if (penType == 2)
                {
                    ApplyLaserPenModeCore(refreshUi: false, updateIndicators: false);
                }

                ColorSwitchCheck();
                HideSubPanels("pen", true);
            }
            else
            {
                if (wasInInkMode)
                {
                    if (forceEraser)
                    {
                        // 从橡皮擦模式切换过来，保持之前的笔类型状态
                        forceEraser = false;
                        forcePointEraser = false;
                        drawingShapeMode = 0;

                        // 保持之前的笔类型状态，而不是强制重置
                        if (!wasHighlighter && penType != 2)
                        {
                            penType = 0;
                            drawingAttributes.IsHighlighter = false;
                            drawingAttributes.StylusTip = StylusTip.Ellipse;
                            Settings.Canvas.EnableInkFade = false;
                            if (_inkFadeManager != null)
                                _inkFadeManager.IsEnabled = false;
                        }
                        else if (penType == 1)
                        {
                            drawingAttributes.IsHighlighter = !Settings.Canvas.HighlighterOverlapEnabled;
                            drawingAttributes.StylusTip = StylusTip.Rectangle;
                            drawingAttributes.Width = Settings.Canvas.HighlighterWidth / 2;
                            drawingAttributes.Height = Settings.Canvas.HighlighterWidth;
                            Settings.Canvas.EnableInkFade = false;
                            if (_inkFadeManager != null)
                                _inkFadeManager.IsEnabled = false;
                        }
                        // 如果之前是激光笔模式，则保持激光笔属性
                        else if (penType == 2)
                        {
                            ApplyLaserPenModeCore(refreshUi: false, updateIndicators: false);
                        }

                        // 在非白板模式下，从线擦切换到批注时不直接弹出子面板
                        if (currentMode != 1)
                        {
                            HideSubPanels("pen", true);
                            return;
                        }
                    }

                    if (PenPalette.IsOpen || BoardPenPalette.IsOpen)
                    {
                        AnimationsHelper.HidePopupWithSlideAndFade(PenPalette);
                        AnimationsHelper.HidePopupWithSlideAndFade(BoardPenPalette);
                    }
                    else if (suppressNextPenPaletteOpen)
                    {
                        suppressNextPenPaletteOpen = false;
                        HideSubPanels("pen", true);
                    }
                    else
                    {
                        HideSubPanels();
                        if (currentMode == 0)
                        {
                            AnimationsHelper.ShowPopupWithSlideAndFade(PenPalette);
                            _popupManager?.BringToFront(PenPalette);
                        }
                        else
                        {
                            AnimationsHelper.ShowPopupWithSlideAndFade(BoardPenPalette);
                            _popupManager?.BringToFront(BoardPenPalette);
                        }
                    }
                }
                else
                {
                    // 切换到批注模式时，确保保存当前图片信息
                    if (currentMode != 0)
                    {
                        SaveStrokes();
                    }
                    // 使用集中化的工具模式切换方法
                    SetCurrentToolMode(InkCanvasEditingMode.Ink);

                    // 更新模式缓存
                    UpdateCurrentToolMode("pen");

                    UpdateBoardPenIconColor();

                    forceEraser = false;
                    forcePointEraser = false;
                    drawingShapeMode = 0;

                    // 保持之前的笔类型状态，而不是强制重置
                    if (!wasHighlighter && penType != 2)
                    {
                        penType = 0;
                        drawingAttributes.IsHighlighter = false;
                        drawingAttributes.StylusTip = StylusTip.Ellipse;
                        Settings.Canvas.EnableInkFade = false;
                        if (_inkFadeManager != null)
                            _inkFadeManager.IsEnabled = false;
                    }
                    else if (penType == 1)
                    {
                        drawingAttributes.IsHighlighter = !Settings.Canvas.HighlighterOverlapEnabled;
                        drawingAttributes.StylusTip = StylusTip.Rectangle;
                        drawingAttributes.Width = Settings.Canvas.HighlighterWidth / 2;
                        drawingAttributes.Height = Settings.Canvas.HighlighterWidth;
                        Settings.Canvas.EnableInkFade = false;
                        if (_inkFadeManager != null)
                            _inkFadeManager.IsEnabled = false;
                    }
                    // 如果之前是激光笔模式，则保持激光笔属性
                    else if (penType == 2)
                    {
                        drawingAttributes.IsHighlighter = false;
                        drawingAttributes.StylusTip = StylusTip.Ellipse;
                        drawingAttributes.Width = Settings.Canvas.LaserPenWidth;
                        drawingAttributes.Height = Settings.Canvas.LaserPenWidth;
                        Settings.Canvas.EnableInkFade = true;
                        if (_inkFadeManager != null)
                        {
                            _inkFadeManager.IsEnabled = true;
                            _inkFadeManager.UpdateFadeTime(Settings.Canvas.InkFadeTime);
                            _inkFadeManager.UpdateFadeSpeedMultiplier(Settings.Canvas.InkFadeSpeedMultiplier);
                        }
                    }

                    ColorSwitchCheck();
                    HideSubPanels("pen", true);
                }
            }


            forceEraser = false;
            forcePointEraser = false;
            drawingShapeMode = 0;
            EnsureRealtimeStylusPipelineBinding();
        }

        /// <summary>
        /// 颜色主题切换鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void ColorThemeSwitch_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isUselightThemeColor = !isUselightThemeColor;
            if (currentMode == 0) isDesktopUselightThemeColor = isUselightThemeColor;
            CheckColorTheme();
        }

        /// <summary>
        /// 橡皮擦图标点击事件处理，用于切换到橡皮擦模式或显示橡皮擦尺寸面板
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal void EraserIcon_Click(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到橡皮擦")) return;

            bool isAlreadyEraser = inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
            forceEraser = false;
            forcePointEraser = true;
            drawingShapeMode = 0;

            // 如果当前不在批注模式，先切换到批注模式
            if (!IsAnnotating)
            {
                PenIcon_Click(sender, e);
            }

            // 切换到橡皮擦模式时，确保保存当前图片信息
            if (!isAlreadyEraser && currentMode != 0)
            {
                SaveStrokes();
            }

            if (!isAlreadyEraser)
            {
                ResetTouchStates();
            }

            // 启用新的高级橡皮擦系统
            EnableEraserOverlay();

            // 使用新的高级橡皮擦系统
            // 使用集中化的工具模式切换方法
            SetCurrentToolMode(InkCanvasEditingMode.EraseByPoint);

            // 更新模式缓存
            UpdateCurrentToolMode("eraser");

            ApplyAdvancedEraserShape(); // 使用新的橡皮擦形状应用方法
            SetCursorBasedOnEditingMode(inkCanvas);
            HideSubPanels("eraser"); // 高亮橡皮按钮
            Trace.WriteLine($"Eraser: Eraser button clicked, current size: {eraserWidth}, circle: {isEraserCircleShape}");

            // 如果启用了橡皮擦自动切换功能，停止之前的计时器（如果正在运行）
            if (Settings.Canvas.EnableEraserAutoSwitchBack)
            {
                StopEraserAutoSwitchBackTimer();
            }

            if (isAlreadyEraser)
            {
                if (EraserSizePanel.IsOpen == false && BoardEraserSizePanel?.IsOpen != true)
                {
                    if (currentMode == 0)
                    {
                        AnimationsHelper.ShowPopupWithSlideAndFade(EraserSizePanel);
                        _popupManager?.BringToFront(EraserSizePanel);
                    }
                    else
                    {
                        AnimationsHelper.ShowPopupWithSlideAndFade(BoardEraserSizePanel);
                        _popupManager?.BringToFront(BoardEraserSizePanel);
                    }
                }
                else
                {
                    AnimationsHelper.HidePopupWithSlideAndFade(EraserSizePanel);
                    if (BoardEraserSizePanel != null)
                        AnimationsHelper.HidePopupWithSlideAndFade(BoardEraserSizePanel);
                }
            }
        }

        /// <summary>
        /// 白板模式下的橡皮擦图标点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void BoardEraserIcon_Click(object sender, RoutedEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到橡皮擦")) return;

            bool isAlreadyEraser = inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
            forceEraser = false;
            forcePointEraser = true;
            drawingShapeMode = 0;

            // 启用新的高级橡皮擦系统
            EnableEraserOverlay();

            // 使用新的高级橡皮擦系统
            // 使用集中化的工具模式切换方法
            SetCurrentToolMode(InkCanvasEditingMode.EraseByPoint);

            // 更新模式缓存
            UpdateCurrentToolMode("eraser");

            ApplyAdvancedEraserShape(); // 使用新的橡皮擦形状应用方法
            SetCursorBasedOnEditingMode(inkCanvas);
            HideSubPanels("eraser"); // 高亮橡皮按钮

            // 如果启用了橡皮擦自动切换功能，停止之前的计时器（如果正在运行）
            if (Settings.Canvas.EnableEraserAutoSwitchBack)
            {
                StopEraserAutoSwitchBackTimer();
            }

            if (isAlreadyEraser)
            {
                if (BoardEraserSizePanel?.IsOpen != true && EraserSizePanel.IsOpen == false)
                {
                    if (currentMode == 0)
                    {
                        AnimationsHelper.ShowPopupWithSlideAndFade(EraserSizePanel);
                        _popupManager?.BringToFront(EraserSizePanel);
                    }
                    else
                    {
                        AnimationsHelper.ShowPopupWithSlideAndFade(BoardEraserSizePanel);
                        _popupManager?.BringToFront(BoardEraserSizePanel);
                    }
                }
                else
                {
                    if (BoardEraserSizePanel != null)
                        AnimationsHelper.HidePopupWithSlideAndFade(BoardEraserSizePanel);
                    AnimationsHelper.HidePopupWithSlideAndFade(EraserSizePanel);
                }
            }
        }

        /// <summary>
        /// 墨迹擦除图标点击事件处理，用于切换到按笔画擦除模式
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal void EraserIconByStrokes_Click(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到线擦")) return;

            // 如果当前不在批注模式，先切换到批注模式
            if (!IsAnnotating)
            {
                PenIcon_Click(sender, e);
            }

            // 禁用高级橡皮擦系统
            DisableEraserOverlay();

            forceEraser = true;
            forcePointEraser = false;

            inkCanvas.EraserShape = new EllipseStylusShape(5, 5);
            // 使用集中化的工具模式切换方法
            SetCurrentToolMode(InkCanvasEditingMode.EraseByStroke);

            // 更新模式缓存
            UpdateCurrentToolMode("eraserByStrokes");

            drawingShapeMode = 0;

            // 这样从线擦切换回批注时，可以恢复之前的荧光笔状态
            // penType 和 drawingAttributes 的状态将在 PenIcon_Click 中根据 wasHighlighter 来恢复

            inkCanvas_EditingModeChanged(inkCanvas, null);
            CancelSingleFingerDragMode();

            HideSubPanels("eraserByStrokes");

        }

        /// <summary>
        /// 白板模式下的墨迹擦除图标点击事件处理，用于切换到按笔画擦除模式
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal void BoardEraserIconByStrokes_Click(object sender, MouseButtonEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到线擦")) return;

            // 禁用高级橡皮擦系统
            DisableEraserOverlay();

            forceEraser = true;
            forcePointEraser = false;

            inkCanvas.EraserShape = new EllipseStylusShape(5, 5);
            // 使用集中化的工具模式切换方法
            SetCurrentToolMode(InkCanvasEditingMode.EraseByStroke);

            // 更新模式缓存
            UpdateCurrentToolMode("eraserByStrokes");

            drawingShapeMode = 0;

            // 这样从线擦切换回批注时，可以恢复之前的荧光笔状态
            // penType 和 drawingAttributes 的状态将在 PenIcon_Click 中根据 wasHighlighter 来恢复

            inkCanvas_EditingModeChanged(inkCanvas, null);
            CancelSingleFingerDragMode();

            HideSubPanels("eraserByStrokes");
        }

        /// <summary>
        /// 光标删除图标点击事件处理，用于删除选中内容并切换到光标模式
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal void CursorWithDelIcon_Click(object sender, MouseButtonEventArgs e)
        {
            SymbolIconDelete_MouseUp(sender, null);
            CursorIcon_Click(null, null);
        }

        /// <summary>
        /// 选择工具图标鼠标释放事件处理，用于切换到选择模式或选择所有墨迹
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void SelectIcon_MouseUp(object sender, RoutedEventArgs e)
        {
            if (TryBlockFrozenPageMutation("切换到选择工具")) return;

            // 禁用高级橡皮擦系统
            DisableEraserOverlay();

            forceEraser = true;
            drawingShapeMode = 0;
            inkCanvas.IsManipulationEnabled = false;
            if (inkCanvas.EditingMode == InkCanvasEditingMode.Select)
            {
                var selectedStrokes = new StrokeCollection();
                foreach (var stroke in inkCanvas.Strokes)
                    if (stroke.GetBounds().Width > 0 && stroke.GetBounds().Height > 0)
                        selectedStrokes.Add(stroke);
                inkCanvas.Select(selectedStrokes);
            }
            else
            {
                // 使用集中化的工具模式切换方法
                SetCurrentToolMode(InkCanvasEditingMode.Select);
            }
        }

        /// <summary>
        /// 从图形绘制模式切换到画笔模式的提示处理
        /// </summary>
        private void DrawShapePromptToPen()
        {
            if (isLongPressSelected)
            {
                // 如果是长按选中的状态，只隐藏面板，不切换到笔模式
                HideSubPanels("shape");
            }
            else
            {
                if (IsAnnotating)
                    HideSubPanels("pen");
                else
                    HideSubPanels("cursor");
            }
        }

        /// <summary>
        /// 关闭工具面板鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        private void CloseBordertools_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideSubPanels();
        }

        private void CloseBordertools_Click(object sender, RoutedEventArgs e)
        {
            HideSubPanels();
        }

        #region Left Side Panel

        /// <summary>
        /// 手指拖动模式切换按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        public void ToggleFingerDragMode(object sender, RoutedEventArgs e)
        {
            isSingleFingerDragMode = !isSingleFingerDragMode;
        }

        /// <summary>
        /// 撤销按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }

            var item = timeMachine.Undo();
            ApplyHistoryToCanvas(item);
        }

        /// <summary>
        /// 重做按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (inkCanvas.GetSelectedStrokes().Count != 0)
            {
                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;
                inkCanvas.Select(new StrokeCollection());
            }

            var item = timeMachine.Redo();
            ApplyHistoryToCanvas(item);
        }

        /// <summary>
        /// 按钮启用状态变更事件处理，用于更新按钮内容的透明度
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">依赖属性变更事件参数</param>
        private void Btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!isLoaded) return;
            try
            {
                if (((Button)sender).IsEnabled)
                    ((UIElement)((Button)sender).Content).Opacity = 1;
                else
                    ((UIElement)((Button)sender).Content).Opacity = 0.25;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }

        #endregion Left Side Panel

        #region Right Side Panel

        public static bool CloseIsFromButton;

        /// <summary>
        /// 退出按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        public void ExitApplication(object sender, RoutedEventArgs e)
        {
            App.IsAppExitByUser = true;
            _forceCloseFromExitOrRestartButton = false;
            // 通过主窗口 Close 进入统一的 Closing 验证流程。
            // Window_Closed 中再显式关闭 Application，确保托盘和隐藏窗口一并退出。
            Close();
        }

        /// <summary>
        /// 重启按钮点击事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        public void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Advanced.IsSecondConfirmWhenShutdownApp)
            {
                if (MessageBox.Show(Properties.MainWindowStrings.Main_CloseConfirm_Level1, "InkCanvasForClass",
                        MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.Cancel) return;
                if (MessageBox.Show(Properties.MainWindowStrings.Main_CloseConfirm_Level2, "InkCanvasForClass",
                        MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel) return;
                if (MessageBox.Show(Properties.MainWindowStrings.Main_CloseConfirm_Level3, "InkCanvasForClass",
                        MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) return;
            }

            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");
            _forceCloseFromExitOrRestartButton = true;
            App.IsAppExitByUser = true;
            CloseIsFromButton = true;
            Close();
        }

        /// <summary>
        /// 切换并打开设置面板；在需要时先进行安全密码校验，然后显示设置面板并启动打开动画，同时根据设置暂时调整无焦点模式与遮罩交互状态。
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">路由事件参数</param>
        internal async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow != null)
            {
                if (_settingsWindow.WindowState == System.Windows.WindowState.Minimized)
                    _settingsWindow.WindowState = System.Windows.WindowState.Normal;
                _settingsWindow.Activate();
                _settingsWindow.Focus();
                return;
            }

            try
            {
                if (Ink_Canvas.Helpers.SecurityManager.IsPasswordRequiredForEnterSettings(Settings))
                {
                    bool ok = await Ink_Canvas.Helpers.SecurityManager.PromptAndVerifyPasswordOrTotpAsync(Settings, this, Properties.MainWindowStrings.Main_EnterSettings, Properties.MainWindowStrings.Main_EnterSettings_Message);
                    if (!ok) return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"安全密码校验失败: {ex}", LogHelper.LogType.Error);
                return;
            }

            HideSubPanels();
            // 打开设置前退出白板模式并切换到鼠标模式
            if (currentMode != 0) CloseWhiteboardImmediately();
            CursorIcon_Click(null, null);
            _settingsWindow = new Windows.SettingsViews.SettingsWindow();
            _settingsWindow.Owner = this;
            _settingsWindow.Topmost = this.Topmost;
            _settingsWindow.Closed += (s, args) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        private bool forceEraser;


        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            if (TryBlockFrozenPageMutation("清空冻结页面内容")) return;
            forceEraser = false;
            //BorderClearInDelete.Visibility = Visibility.Collapsed;

            if (currentMode == 0)
            {
                // 先回到画笔再清屏，避免 TimeMachine 的相关 bug 影响
                if (Pen_Icon.Background == null && IsAnnotating)
                    PenIcon_Click(null, null);
            }
            else
            {
                if (Pen_Icon.Background == null) PenIcon_Click(null, null);
            }

            if (inkCanvas.Strokes.Count != 0)
            {
                // 注入历史记录以便支持撤销
                var whiteboardIndex = CurrentWhiteboardIndex;
                if (currentMode == 0) whiteboardIndex = 0;
            }

            ClearStrokes(false);
            // 保存非笔画元素（如图片）
            var preservedElements = PreserveNonStrokeElements();
            inkCanvas.Children.Clear();
            // 恢复非笔画元素
            RestoreNonStrokeElements(preservedElements);

            if (Settings.Canvas.ClearCanvasAndClearTimeMachine) timeMachine.ClearStrokeHistory();

            CancelSingleFingerDragMode();

        }

        private bool lastIsInMultiTouchMode;

        private void CancelSingleFingerDragMode()
        {
            if (ToggleSwitchDrawShapeBorderAutoHide.IsOn) CollapseBorderDrawShape();

            GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

            if (isSingleFingerDragMode) ToggleFingerDragMode(null, null);
            isLongPressSelected = false;
        }

        /// <summary>
        /// 重置所有触摸相关状态，
        /// </summary>
        private void ResetTouchStates()
        {
            try
            {
                // 清空触摸点计数器
                dec.Clear();

                if (isPalmEraserActive)
                    isPalmEraserActive = false;

                // 确保触摸事件能正常响应
                inkCanvas.IsHitTestVisible = true;
                inkCanvas.IsManipulationEnabled = true;

                // 释放所有触摸捕获
                inkCanvas.ReleaseAllTouchCaptures();

                // 恢复UI元素的触摸响应
                ViewboxFloatingBar.IsHitTestVisible = true;
                BlackboardUIGridForInkReplay.IsHitTestVisible = true;


            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"重置触摸状态失败: {ex.Message}", LogHelper.LogType.Error);
            }
        }


        // 退出批注模式时的全屏还原处理
        private void RestoreFullScreenOnExitAnnotationMode()
        {
            if (Settings.Advanced.IsEnableAvoidFullScreenHelper &&
                isFullScreenApplied &&
                currentMode == 0 && // 不在白板模式
                !IsInPPTPresentationMode) // 不在PPT放映模式
            {
                // 恢复为非画板模式，重新启用全屏限制
                AvoidFullScreenHelper.SetBoardMode(false);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 退出批注模式，恢复到工作区域大小
                    var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                    MainWindow.MoveWindow(new WindowInteropHelper(this).Handle,
                        workingArea.Left, workingArea.Top,
                        workingArea.Width, workingArea.Height, true);
                }), DispatcherPriority.ApplicationIdle);

                isFullScreenApplied = false; // 标记全屏处理已还原
            }
        }

        /// <summary>
        /// 在屏幕模式、白板与黑板模式之间切换并同步相关的 UI 状态与资源处理。
        /// </summary>
        /// <remarks>
        /// 切换过程中会保存/清理/恢复画笔轨迹，显示或隐藏白板/黑板面板、手势面板与 PPT 控件，调整主题与悬浮工具栏可见性，处理全屏/工作区尺寸恢复或进入全屏，以及在进入白板时检查剪贴板并显示粘贴提示。该方法还会触发隐藏/显示墨迹画布的逻辑（通过调用 BtnHideInkCanvas_Click）。
        /// </remarks>
        private void SwitchBackground(object sender, RoutedEventArgs e)
        {
            if (GridTransparencyFakeBackground.Background == Brushes.Transparent)
            {
                if (currentMode == 0)
                {
                    currentMode++;
                    AutomationBootstrap.Monitor?.NotifyInternalStateChanged();
                    GridBackgroundCover.Visibility = Visibility.Collapsed;
                    AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                    AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                    AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);

                    // 在PPT模式下隐藏手势面板和手势按钮
                    AnimationsHelper.HideWithSlideAndFade(TwoFingerGestureBorder);
                    AnimationsHelper.HideWithSlideAndFade(BoardTwoFingerGestureBorder);
                    UpdateToolbarComponentVisibility();

                    SaveStrokes(true);
                    ClearStrokes(true);
                    RestoreStrokes(true);


                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                    {
                        { /* Old UI removed */ }
                        { /* Old UI removed */ }
                    }
                    else
                    {
                        { /* Old UI removed */ }
                        if (isPresentationHaveBlackSpace)
                        {
                            { /* Old UI removed */ }
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            { /* Old UI removed */ }
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }
                    }

                    { /* Old UI removed */ }

                    CheckClipboardImageAndShowPasteNotificationWhenEnteringBoard();
                }

                Topmost = true;
                BtnHideInkCanvas_Click(null, e);
            }
            else
            {
                switch (++currentMode % 2)
                {
                    case 0: //屏幕模式
                        VideoPresenter_OnExitWhiteboardMode();
                        currentMode = 0;

                        // 退出白板的公共兜底：所有入口（浮动栏按钮、白板工具栏、热键、自动收纳、IPC）
                        // 都汇聚到这里，统一按当前放映状态重新评估翻页条可见性。
                        // 真实 PPT 与外部演示源（插件注册的 PDF 等）都能恢复，非放映场景维持隐藏。
                        _pptUIManager?.UpdateNavigationPanelsVisibility();

                        AutomationBootstrap.Monitor?.NotifyInternalStateChanged();
                        GridBackgroundCover.Visibility = Visibility.Collapsed;
                        AnimationsHelper.HideWithSlideAndFade(BlackboardLeftSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardCenterSide);
                        AnimationsHelper.HideWithSlideAndFade(BlackboardRightSide);

                        // 在PPT模式下隐藏手势面板和手势按钮
                        AnimationsHelper.HideWithSlideAndFade(TwoFingerGestureBorder);
                        AnimationsHelper.HideWithSlideAndFade(BoardTwoFingerGestureBorder);
                        UpdateToolbarComponentVisibility();

                        SaveStrokes();
                        ClearStrokes(true);
                        RestoreStrokes(true);

                        // 退出白板模式时取消全屏（仅在非PPT模式下）
                        if (Settings.Advanced.IsEnableAvoidFullScreenHelper &&
                            !IsInPPTPresentationMode) // 不在PPT放映模式
                        {
                            // 恢复为非画板模式，重新启用全屏限制
                            AvoidFullScreenHelper.SetBoardMode(false);

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                // 退出白板模式，恢复到工作区域大小
                                var workingArea = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
                                MainWindow.MoveWindow(new WindowInteropHelper(this).Handle,
                                    workingArea.Left, workingArea.Top,
                                    workingArea.Width, workingArea.Height, true);
                            }), DispatcherPriority.ApplicationIdle);

                            isFullScreenApplied = false; // 标记全屏处理已还原
                        }

                        // 在屏幕模式下恢复基础浮动栏的显示
                        ViewboxFloatingBar.Visibility = Visibility.Visible;

                        // 退出白板时自动收纳功能 - 等待浮动栏完全展开后再收纳
                        // 当处于PPT放映模式时，不自动收纳
                        if (Settings.Automation.IsAutoFoldWhenExitWhiteboard && !isFloatingBarFolded &&
                            !IsInPPTPresentationMode)
                        {
                            // 使用异步延迟，等待浮动栏展开动画完成后再收纳
                            Task.Run(async () =>
                            {
                                await Task.Delay(700);
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    FoldFloatingBar_MouseUp(new object(), null);
                                });
                            });
                        }

                        if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                        {
                            { /* Old UI removed */ }
                            { /* Old UI removed */ }
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            { /* Old UI removed */ }
                            if (isPresentationHaveBlackSpace)
                            {
                                { /* Old UI removed */ }
                                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                            }
                            else
                            {
                                { /* Old UI removed */ }
                                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                            }
                        }

                        { /* Old UI removed */ }
                        Topmost = true;
                        break;
                    case 1: //黑板或白板模式
                        currentMode = 1;
                        AutomationBootstrap.Monitor?.NotifyInternalStateChanged();
                        GridBackgroundCover.Visibility = Visibility.Visible;
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardLeftSide);
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardCenterSide);
                        AnimationsHelper.ShowWithSlideFromBottomAndFade(BlackboardRightSide);

                        SaveStrokes(true);
                        ClearStrokes(true);

                        RestoreStrokes();

                        // 进入白板模式时全屏（仅在非PPT模式下）
                        if (Settings.Advanced.IsEnableAvoidFullScreenHelper &&
                            !IsInPPTPresentationMode) // 不在PPT放映模式
                        {
                            // 设置为画板模式，允许全屏操作
                            AvoidFullScreenHelper.SetBoardMode(true);
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MainWindow.MoveWindow(new WindowInteropHelper(this).Handle, 0, 0,
                                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                                    System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, true);
                            }), DispatcherPriority.ApplicationIdle);

                            isFullScreenApplied = true; // 标记已应用全屏处理
                        }

                        ViewboxFloatingBar.Visibility = Visibility.Collapsed;

                        { /* Old UI removed */ }
                        if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                        {
                            { /* Old UI removed */ }
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                        }
                        else
                        {
                            { /* Old UI removed */ }
                            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                        }

                        if (Settings.Canvas.UsingWhiteboard)
                        {
                            // 如果有自定义背景色并且是白板模式，应用自定义背景色
                            if (CustomBackgroundColor.HasValue)
                            {
                                GridBackgroundCover.Background = new SolidColorBrush(CustomBackgroundColor.Value);
                            }
                            // 白板模式下设置墨迹颜色为黑色
                            CheckLastColor(0);
                            forceEraser = false;
                            ColorSwitchCheck();
                        }
                        else
                        {
                            // 黑板模式下设置墨迹颜色为白色
                            CheckLastColor(5);
                            forceEraser = false;
                            ColorSwitchCheck();
                        }

                        { /* Old UI removed */ }

                        if (Settings.Advanced.EnableUIAccessTopMost)
                        {
                            Topmost = true;
                        }
                        else
                        {
                            Topmost = false;
                        }

                        CheckClipboardImageAndShowPasteNotificationWhenEnteringBoard();
                        break;
                }
            }

            SyncPdfPageSidebarWithCanvas();
        }

        public int BoundsWidth = 5;
        private bool _isToolbarOnRightSide = true;

        private void BtnHideInkCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (GridTransparencyFakeBackground.Background == Brushes.Transparent)
            {
                // 进入批注模式
                GridTransparencyFakeBackground.Opacity = 1;
                GridTransparencyFakeBackground.Background = new SolidColorBrush(StringToColor("#01FFFFFF"));
                SetTransparentNotHitThrough();
                inkCanvas.IsHitTestVisible = true;
                inkCanvas.Visibility = Visibility.Visible;

                GridBackgroundCoverHolder.Visibility = Visibility.Visible;

                GridInkCanvasSelectionCover.Visibility = Visibility.Collapsed;

                if (GridBackgroundCover.Visibility == Visibility.Collapsed)
                {
                    if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                    { /* Old UI removed */ }
                    else
                    { /* Old UI removed */ }
                    { /* Old UI removed */ }
                }
                else
                {
                    { /* Old UI removed */ }
                    { /* Old UI removed */ }
                }

                { /* Old UI removed */ }

                // 进入批注模式时的全屏处理（仅当未应用过全屏处理时）
                if (Settings.Advanced.IsEnableAvoidFullScreenHelper && !isFullScreenApplied)
                {
                    // 设置为画板模式，允许全屏操作
                    AvoidFullScreenHelper.SetBoardMode(true);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MainWindow.MoveWindow(new WindowInteropHelper(this).Handle, 0, 0,
                            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                            System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, true);
                    }), DispatcherPriority.ApplicationIdle);

                    isFullScreenApplied = true; // 标记已应用全屏处理
                }
            }
            else
            {
                // Auto-clear Strokes 要等待截图完成再清理笔记
                if (!IsInPPTPresentationMode)
                {
                    if (isLoaded && Settings.Automation.IsAutoClearWhenExitingWritingMode)
                        if (inkCanvas.Strokes.Count > 0)
                        {
                            if (Settings.Automation.IsAutoSaveScreenshotAtClear && inkCanvas.Strokes.Count >
                                Settings.Automation.MinimumAutomationStrokeNumber)
                                CaptureAndEnqueueScreenshotSave(true);

                            //BtnClear_Click(null, null);
                        }

                    inkCanvas.IsHitTestVisible = true;
                    inkCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    if (isLoaded && Settings.Automation.IsAutoClearWhenExitingWritingMode &&
                        !Settings.PowerPointSettings.IsNoClearStrokeOnSelectWhenInPowerPoint)
                        if (inkCanvas.Strokes.Count > 0)
                        {
                            if (Settings.Automation.IsAutoSaveScreenshotAtClear && inkCanvas.Strokes.Count >
                                Settings.Automation.MinimumAutomationStrokeNumber)
                                CaptureAndEnqueueScreenshotSave(true);

                            //BtnClear_Click(null, null);
                        }


                    if (Settings.PowerPointSettings.IsShowStrokeOnSelectInPowerPoint)
                    {
                        inkCanvas.Visibility = Visibility.Visible;
                        inkCanvas.IsHitTestVisible = true;
                    }
                    else
                    {
                        inkCanvas.IsHitTestVisible = true;
                        inkCanvas.Visibility = Visibility.Visible;
                    }
                }

                GridTransparencyFakeBackground.Opacity = 0;
                GridTransparencyFakeBackground.Background = Brushes.Transparent;
                SetTransparentHitThrough();

                GridBackgroundCoverHolder.Visibility = Visibility.Collapsed;

                // 退出批注模式时的全屏还原
                RestoreFullScreenOnExitAnnotationMode();

                if (currentMode != 0)
                {
                    SaveStrokes();
                    RestoreStrokes(true);
                }

                if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Dark)
                { /* Old UI removed */ }
                else
                { /* Old UI removed */ }

                { /* Old UI removed */ }
                { /* Old UI removed */ }
            }

            if (GridTransparencyFakeBackground.Background == Brushes.Transparent)
            {
                UpdateToolbarComponentVisibility();
                HideSubPanels("cursor");

                if (currentMode == 0)
                {
                    ViewboxFloatingBar.Visibility = Visibility.Visible;
                }
            }
            else
            {
                UpdateToolbarComponentVisibility();

                if (currentMode == 0)
                {
                    ViewboxFloatingBar.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnSwitchSide_Click(object sender, RoutedEventArgs e)
        {
            if (_isToolbarOnRightSide)
            {
                { /* Old UI removed */ }
                { /* Old UI removed */ }
            }
            else
            {
                { /* Old UI removed */ }
                { /* Old UI removed */ }
            }
        }

        private void StackPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((StackPanel)sender).Visibility == Visibility.Visible)
            { /* Old UI removed */ }
            else
            { /* Old UI removed */ }
        }

        #endregion

        /// <summary>
        /// 强制禁用所有双指手势功能（当多指书写模式启用时）
        /// </summary>
        private void ForceDisableTwoFingerGestures()
        {
            // 强制关闭所有双指手势设置
            Settings.Gesture.IsEnableTwoFingerTranslate = false;
            Settings.Gesture.IsEnableTwoFingerZoom = false;
            Settings.Gesture.IsEnableTwoFingerRotation = false;

            // 更新UI开关状态
            if (ToggleSwitchEnableTwoFingerTranslate != null)
                ToggleSwitchEnableTwoFingerTranslate.IsOn = false;
            if (ToggleSwitchEnableTwoFingerZoom != null)
                ToggleSwitchEnableTwoFingerZoom.IsOn = false;
            if (ToggleSwitchEnableTwoFingerRotation != null)
                ToggleSwitchEnableTwoFingerRotation.IsOn = false;

            // 更新设置窗口中的开关状态
            if (BoardToggleSwitchEnableTwoFingerTranslate != null)
                BoardToggleSwitchEnableTwoFingerTranslate.IsOn = false;
            if (BoardToggleSwitchEnableTwoFingerZoom != null)
                BoardToggleSwitchEnableTwoFingerZoom.IsOn = false;
            if (BoardToggleSwitchEnableTwoFingerRotation != null)
                BoardToggleSwitchEnableTwoFingerRotation.IsOn = false;
        }

    }
}
