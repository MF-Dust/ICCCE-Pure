using Ink_Canvas.Controls;
using Ink_Canvas.Controls.Toolbar.FloatingToolbar;
using Ink_Canvas.Helpers;
using Ink_Canvas.WorkflowAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Application = System.Windows.Application;
namespace Ink_Canvas
{
    public partial class MainWindow
    {
        internal ToolbarImageButton SymbolIconDelete { get; private set; }
        internal ToolbarImageButton Eraser_Icon { get; private set; }
        internal ToolbarImageButton EraserByStrokes_Icon { get; private set; }
        internal ToolbarImageButton SymbolIconSelect { get; private set; }
        internal ToolbarImageButton ShapeDrawFloatingBarBtn { get; private set; }
        internal ToolbarImageButton SymbolIconUndo { get; private set; }
        internal ToolbarImageButton SymbolIconRedo { get; private set; }
        internal ToolbarImageButton CursorWithDelFloatingBarBtn { get; private set; }
        internal ToolbarImageButton WhiteboardFloatingBarBtn { get; private set; }
        internal ToolbarImageButton ToolsFloatingBarBtn { get; private set; }
        internal ToolbarImageButton Fold_Icon { get; private set; }
        internal ToolbarImageButton Freeze_Icon { get; private set; }
        internal ToolbarImageButton Gesture_Icon { get; private set; }
        internal ToolbarImageButton Exit_Icon { get; private set; }

        internal Panel FloatingBarRootPanel => StackPanelFloatingBarRoot;

        internal double FloatingBarSelectionBGLeft => GetSelectionBGLeft();
        internal bool FloatingBarSelectionBGIsHidden
        {
            get
            {
                var (selectionBG, _, _) = GetFirstContentBorderElements();
                return selectionBG == null || selectionBG.Visibility != Visibility.Visible;
            }
        }
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchDrawShapeBorderAutoHide { get; } =
            new iNKORE.UI.WPF.Modern.Controls.ToggleSwitch { IsOn = true };

        internal GeometryButton ImageDrawLine => ShapeDrawPopupContent?.DrawLineBtn;
        internal GeometryButton ImageDrawDashedLine => ShapeDrawPopupContent?.DrawDashedLineBtn;
        internal GeometryButton ImageDrawDotLine => ShapeDrawPopupContent?.DrawDotLineBtn;
        internal GeometryButton ImageDrawArrow => ShapeDrawPopupContent?.DrawArrowBtn;
        internal GeometryButton ImageDrawParallelLine => ShapeDrawPopupContent?.DrawParallelLineBtn;

        internal GeometryButton BoardImageDrawLine => BoardShapeDrawPopupContent?.DrawLineBtn;
        internal GeometryButton BoardImageDrawDashedLine => BoardShapeDrawPopupContent?.DrawDashedLineBtn;
        internal GeometryButton BoardImageDrawDotLine => BoardShapeDrawPopupContent?.DrawDotLineBtn;
        internal GeometryButton BoardImageDrawArrow => BoardShapeDrawPopupContent?.DrawArrowBtn;
        internal GeometryButton BoardImageDrawParallelLine => BoardShapeDrawPopupContent?.DrawParallelLineBtn;

        internal void AttachCursorIconView(ToolbarImageButton btn) => Cursor_Icon = btn;
        internal void AttachPenIconView(ToolbarImageButton btn) { Pen_Icon = btn; PenPalette.PlacementTarget = btn; }
        internal void AttachSymbolIconDelete(ToolbarImageButton btn) => SymbolIconDelete = btn;
        internal void AttachEraserIcon(ToolbarImageButton btn) { Eraser_Icon = btn; EraserSizePanel.PlacementTarget = btn; }
        internal void AttachEraserByStrokesIcon(ToolbarImageButton btn) => EraserByStrokes_Icon = btn;
        internal void AttachSymbolIconSelect(ToolbarImageButton btn) => SymbolIconSelect = btn;
        internal void AttachShapeDrawBtn(ToolbarImageButton btn)
        {
            ShapeDrawFloatingBarBtn = btn;
            BorderDrawShape.PlacementTarget = btn;
        }
        internal void AttachSymbolIconUndo(ToolbarImageButton btn) => SymbolIconUndo = btn;
        internal void AttachSymbolIconRedo(ToolbarImageButton btn) => SymbolIconRedo = btn;
        internal void AttachCursorWithDelBtn(ToolbarImageButton btn) => CursorWithDelFloatingBarBtn = btn;
        internal void AttachWhiteboardBtn(ToolbarImageButton btn) => WhiteboardFloatingBarBtn = btn;
        internal void AttachToolsBtn(ToolbarImageButton btn)
        {
            ToolsFloatingBarBtn = btn;
            BorderTools.PlacementTarget = btn;
        }
        internal void AttachFoldIcon(ToolbarImageButton btn) => Fold_Icon = btn;
        internal void AttachGestureBtn(ToolbarImageButton btn) { Gesture_Icon = btn; TwoFingerGestureBorder.PlacementTarget = btn; }
        internal void AttachExitBtn(ToolbarImageButton btn) => Exit_Icon = btn;

        #region PenPalette property mappings
        internal ComboBox ComboBoxPenStyle => PenPalettePopupContent?.PenStyleComboBox ?? BoardPenPalettePopupContent?.PenStyleComboBox;
        internal ComboBox BoardComboBoxPenStyle => BoardPenPalettePopupContent?.PenStyleComboBox;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchEnableNibMode => PenPalettePopupContent?.NibModeToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableNibMode => BoardPenPalettePopupContent?.NibModeToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch FloatingBarToggleSwitchEnableInkToShape => PenPalettePopupContent?.InkToShapeToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableInkToShape => BoardPenPalettePopupContent?.InkToShapeToggle;
        internal Slider PenWidthSlider => PenPalettePopupContent?.PenWidthSlider;
        internal Slider BoardPenWidthSlider => BoardPenPalettePopupContent?.PenWidthSlider;
        internal Slider PenAlphaSlider => PenPalettePopupContent?.PenAlphaSlider;
        internal Slider BoardPenAlphaSlider => BoardPenPalettePopupContent?.PenAlphaSlider;
        internal Slider LaserPenFadeTimeSlider => PenPalettePopupContent?.LaserPenFadeTimeSlider ?? BoardPenPalettePopupContent?.LaserPenFadeTimeSlider;
        internal Slider BoardLaserPenFadeTimeSlider => BoardPenPalettePopupContent?.LaserPenFadeTimeSlider;
        internal Slider LaserPenFadeSpeedSlider => PenPalettePopupContent?.LaserPenFadeSpeedSlider ?? BoardPenPalettePopupContent?.LaserPenFadeSpeedSlider;
        internal Slider BoardLaserPenFadeSpeedSlider => BoardPenPalettePopupContent?.LaserPenFadeSpeedSlider;
        internal TextBlock PenWidthText => PenPalettePopupContent?.PenWidthText ?? BoardPenPalettePopupContent?.PenWidthText;
        internal TextBlock BoardPenWidthText => BoardPenPalettePopupContent?.PenWidthText;
        internal TextBlock PenAlphaText => PenPalettePopupContent?.PenAlphaText ?? BoardPenPalettePopupContent?.PenAlphaText;
        internal TextBlock BoardPenAlphaText => BoardPenPalettePopupContent?.PenAlphaText;
        internal TextBlock LaserPenFadeTimeText => PenPalettePopupContent?.LaserPenFadeTimeText ?? BoardPenPalettePopupContent?.LaserPenFadeTimeText;
        internal TextBlock BoardLaserPenFadeTimeText => BoardPenPalettePopupContent?.LaserPenFadeTimeText;
        internal TextBlock LaserPenFadeSpeedText => PenPalettePopupContent?.LaserPenFadeSpeedText ?? BoardPenPalettePopupContent?.LaserPenFadeSpeedText;
        internal TextBlock BoardLaserPenFadeSpeedText => BoardPenPalettePopupContent?.LaserPenFadeSpeedText;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch HighlighterOverlapToggle => PenPalettePopupContent?.HighlighterOverlapToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardHighlighterOverlapToggle => BoardPenPalettePopupContent?.HighlighterOverlapToggle;

        internal PopupTabTitleBar PenTabTitleBar => PenPalettePopupContent?.TabBar ?? BoardPenPalettePopupContent?.TabBar;
        internal PopupTabTitleBar BoardPenTabTitleBar => BoardPenPalettePopupContent?.TabBar;
        internal int PenSelectedTabIndex
        {
            get => PenPalettePopupContent?.SelectedTabIndex ?? BoardPenPalettePopupContent?.SelectedTabIndex ?? 0;
            set
            {
                if (PenPalettePopupContent != null) PenPalettePopupContent.SelectedTabIndex = value;
                if (BoardPenPalettePopupContent != null) BoardPenPalettePopupContent.SelectedTabIndex = value;
            }
        }
        internal int BoardPenSelectedTabIndex
        {
            get => BoardPenPalettePopupContent?.SelectedTabIndex ?? 0;
            set { if (BoardPenPalettePopupContent != null) BoardPenPalettePopupContent.SelectedTabIndex = value; }
        }

        internal FrameworkElement CommonPropsPanel => PenPalettePopupContent?.CommonPropsPanel ?? BoardPenPalettePopupContent?.CommonPropsPanel;
        internal FrameworkElement LaserPenFadePanel => PenPalettePopupContent?.LaserPenFadePanel ?? BoardPenPalettePopupContent?.LaserPenFadePanel;
        internal FrameworkElement LaserPenFadeSpeedPanel => PenPalettePopupContent?.LaserPenFadeSpeedPanel ?? BoardPenPalettePopupContent?.LaserPenFadeSpeedPanel;
        internal FrameworkElement InkToShapePanel => PenPalettePopupContent?.InkToShapePanel ?? BoardPenPalettePopupContent?.InkToShapePanel;
        internal FrameworkElement HighlighterOverlapPanel => PenPalettePopupContent?.HighlighterOverlapPanel ?? BoardPenPalettePopupContent?.HighlighterOverlapPanel;
        internal FrameworkElement DefaultPenColorsPanel => PenPalettePopupContent?.DefaultPenColorsPanel ?? BoardPenPalettePopupContent?.DefaultPenColorsPanel;
        internal FrameworkElement HighlighterPenColorsPanel => PenPalettePopupContent?.HighlighterPenColorsPanel ?? BoardPenPalettePopupContent?.HighlighterPenColorsPanel;
        internal FrameworkElement LaserPenColorsPanel => PenPalettePopupContent?.LaserPenColorsPanel ?? BoardPenPalettePopupContent?.LaserPenColorsPanel;

        internal FrameworkElement BoardCommonPropsPanel => BoardPenPalettePopupContent?.CommonPropsPanel;
        internal FrameworkElement BoardLaserPenFadePanel => BoardPenPalettePopupContent?.LaserPenFadePanel;
        internal FrameworkElement BoardLaserPenFadeSpeedPanel => BoardPenPalettePopupContent?.LaserPenFadeSpeedPanel;
        internal FrameworkElement BoardInkToShapePanel => BoardPenPalettePopupContent?.InkToShapePanel;
        internal FrameworkElement BoardHighlighterOverlapPanel => BoardPenPalettePopupContent?.HighlighterOverlapPanel;
        internal FrameworkElement BoardDefaultPenColorsPanel => BoardPenPalettePopupContent?.DefaultPenColorsPanel;
        internal FrameworkElement BoardHighlighterPenColorsPanel => BoardPenPalettePopupContent?.HighlighterPenColorsPanel;
        internal FrameworkElement BoardLaserPenColorsPanel => BoardPenPalettePopupContent?.LaserPenColorsPanel;



        internal PenColorButton BorderPenColorBlack => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorBlack;
        internal PenColorButton BorderPenColorWhite => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorWhite;
        internal PenColorButton BorderPenColorRed => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorRed;
        internal PenColorButton BorderPenColorYellow => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorYellow;
        internal PenColorButton BorderPenColorGreen => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorGreen;
        internal PenColorButton BorderPenColorBlue => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorBlue;
        internal PenColorButton BorderPenColorPink => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorPink;
        internal PenColorButton BorderPenColorTeal => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorTeal;
        internal PenColorButton BorderPenColorOrange => (PenPalettePopupContent ?? BoardPenPalettePopupContent)?.DefaultPenColorOrange;

        internal PenColorButton BoardBorderPenColorBlack => BoardPenPalettePopupContent?.DefaultPenColorBlack;
        internal PenColorButton BoardBorderPenColorWhite => BoardPenPalettePopupContent?.DefaultPenColorWhite;
        internal PenColorButton BoardBorderPenColorRed => BoardPenPalettePopupContent?.DefaultPenColorRed;
        internal PenColorButton BoardBorderPenColorYellow => BoardPenPalettePopupContent?.DefaultPenColorYellow;
        internal PenColorButton BoardBorderPenColorGreen => BoardPenPalettePopupContent?.DefaultPenColorGreen;
        internal PenColorButton BoardBorderPenColorBlue => BoardPenPalettePopupContent?.DefaultPenColorBlue;
        internal PenColorButton BoardBorderPenColorPink => BoardPenPalettePopupContent?.DefaultPenColorPink;
        internal PenColorButton BoardBorderPenColorTeal => BoardPenPalettePopupContent?.DefaultPenColorTeal;
        internal PenColorButton BoardBorderPenColorOrange => BoardPenPalettePopupContent?.DefaultPenColorOrange;

        internal PenColorButton HighlighterPenColorBlack => PenPalettePopupContent?.HighlighterPenColorBlack ?? BoardPenPalettePopupContent?.HighlighterPenColorBlack;
        internal PenColorButton HighlighterPenColorWhite => PenPalettePopupContent?.HighlighterPenColorWhite ?? BoardPenPalettePopupContent?.HighlighterPenColorWhite;
        internal PenColorButton HighlighterPenColorRed => PenPalettePopupContent?.HighlighterPenColorRed ?? BoardPenPalettePopupContent?.HighlighterPenColorRed;
        internal PenColorButton HighlighterPenColorYellow => PenPalettePopupContent?.HighlighterPenColorYellow ?? BoardPenPalettePopupContent?.HighlighterPenColorYellow;
        internal PenColorButton HighlighterPenColorGreen => PenPalettePopupContent?.HighlighterPenColorGreen ?? BoardPenPalettePopupContent?.HighlighterPenColorGreen;
        internal PenColorButton HighlighterPenColorZinc => PenPalettePopupContent?.HighlighterPenColorZinc ?? BoardPenPalettePopupContent?.HighlighterPenColorZinc;
        internal PenColorButton HighlighterPenColorBlue => PenPalettePopupContent?.HighlighterPenColorBlue ?? BoardPenPalettePopupContent?.HighlighterPenColorBlue;
        internal PenColorButton HighlighterPenPenColorPurple => PenPalettePopupContent?.HighlighterPenColorPurple ?? BoardPenPalettePopupContent?.HighlighterPenColorPurple;
        internal PenColorButton HighlighterPenColorTeal => PenPalettePopupContent?.HighlighterPenColorTeal ?? BoardPenPalettePopupContent?.HighlighterPenColorTeal;
        internal PenColorButton HighlighterPenColorOrange => PenPalettePopupContent?.HighlighterPenColorOrange ?? BoardPenPalettePopupContent?.HighlighterPenColorOrange;

        internal PenColorButton BoardHighlighterPenColorBlack => BoardPenPalettePopupContent?.HighlighterPenColorBlack;
        internal PenColorButton BoardHighlighterPenColorWhite => BoardPenPalettePopupContent?.HighlighterPenColorWhite;
        internal PenColorButton BoardHighlighterPenColorRed => BoardPenPalettePopupContent?.HighlighterPenColorRed;
        internal PenColorButton BoardHighlighterPenColorYellow => BoardPenPalettePopupContent?.HighlighterPenColorYellow;
        internal PenColorButton BoardHighlighterPenColorGreen => BoardPenPalettePopupContent?.HighlighterPenColorGreen;
        internal PenColorButton BoardHighlighterPenColorZinc => BoardPenPalettePopupContent?.HighlighterPenColorZinc;
        internal PenColorButton BoardHighlighterPenColorBlue => BoardPenPalettePopupContent?.HighlighterPenColorBlue;
        internal PenColorButton BoardHighlighterPenPenColorPurple => BoardPenPalettePopupContent?.HighlighterPenColorPurple;
        internal PenColorButton BoardHighlighterPenColorTeal => BoardPenPalettePopupContent?.HighlighterPenColorTeal;
        internal PenColorButton BoardHighlighterPenColorOrange => BoardPenPalettePopupContent?.HighlighterPenColorOrange;

        internal PenColorButton LaserPenColorBlack => PenPalettePopupContent?.LaserPenColorBlack ?? BoardPenPalettePopupContent?.LaserPenColorBlack;
        internal PenColorButton LaserPenColorWhite => PenPalettePopupContent?.LaserPenColorWhite ?? BoardPenPalettePopupContent?.LaserPenColorWhite;
        internal PenColorButton LaserPenColorRed => PenPalettePopupContent?.LaserPenColorRed ?? BoardPenPalettePopupContent?.LaserPenColorRed;
        internal PenColorButton LaserPenColorYellow => PenPalettePopupContent?.LaserPenColorYellow ?? BoardPenPalettePopupContent?.LaserPenColorYellow;
        internal PenColorButton LaserPenColorGreen => PenPalettePopupContent?.LaserPenColorGreen ?? BoardPenPalettePopupContent?.LaserPenColorGreen;
        internal PenColorButton LaserPenColorBlue => PenPalettePopupContent?.LaserPenColorBlue ?? BoardPenPalettePopupContent?.LaserPenColorBlue;
        internal PenColorButton LaserPenColorPink => PenPalettePopupContent?.LaserPenColorPink ?? BoardPenPalettePopupContent?.LaserPenColorPink;
        internal PenColorButton LaserPenColorTeal => PenPalettePopupContent?.LaserPenColorTeal ?? BoardPenPalettePopupContent?.LaserPenColorTeal;
        internal PenColorButton LaserPenColorOrange => PenPalettePopupContent?.LaserPenColorOrange ?? BoardPenPalettePopupContent?.LaserPenColorOrange;

        internal PenColorButton BoardLaserPenColorBlack => BoardPenPalettePopupContent?.LaserPenColorBlack;
        internal PenColorButton BoardLaserPenColorWhite => BoardPenPalettePopupContent?.LaserPenColorWhite;
        internal PenColorButton BoardLaserPenColorRed => BoardPenPalettePopupContent?.LaserPenColorRed;
        internal PenColorButton BoardLaserPenColorYellow => BoardPenPalettePopupContent?.LaserPenColorYellow;
        internal PenColorButton BoardLaserPenColorGreen => BoardPenPalettePopupContent?.LaserPenColorGreen;
        internal PenColorButton BoardLaserPenColorBlue => BoardPenPalettePopupContent?.LaserPenColorBlue;
        internal PenColorButton BoardLaserPenColorPink => BoardPenPalettePopupContent?.LaserPenColorPink;
        internal PenColorButton BoardLaserPenColorTeal => BoardPenPalettePopupContent?.LaserPenColorTeal;
        internal PenColorButton BoardLaserPenColorOrange => BoardPenPalettePopupContent?.LaserPenColorOrange;

        internal Border ColorThemeSwitch => PenPalettePopupContent?.ColorThemeSwitch ?? BoardPenPalettePopupContent?.ColorThemeSwitch;
        internal Image ColorThemeSwitchIcon => PenPalettePopupContent?.ColorThemeSwitchIcon ?? BoardPenPalettePopupContent?.ColorThemeSwitchIcon;
        internal TextBlock ColorThemeSwitchTextBlock => PenPalettePopupContent?.ColorThemeSwitchText ?? BoardPenPalettePopupContent?.ColorThemeSwitchText;
        internal Border BoardColorThemeSwitch => BoardPenPalettePopupContent?.ColorThemeSwitch;
        internal Image BoardColorThemeSwitchIcon => BoardPenPalettePopupContent?.ColorThemeSwitchIcon;
        internal TextBlock BoardColorThemeSwitchTextBlock => BoardPenPalettePopupContent?.ColorThemeSwitchText;
        internal Border LaserPenColorThemeSwitch => PenPalettePopupContent?.LaserPenColorThemeSwitch ?? BoardPenPalettePopupContent?.LaserPenColorThemeSwitch;
        internal Image LaserPenColorThemeSwitchIcon => PenPalettePopupContent?.LaserPenColorThemeSwitchIcon ?? BoardPenPalettePopupContent?.LaserPenColorThemeSwitchIcon;
        internal TextBlock LaserPenColorThemeSwitchTextBlock => PenPalettePopupContent?.LaserPenColorThemeSwitchText ?? BoardPenPalettePopupContent?.LaserPenColorThemeSwitchText;
        internal Border BoardLaserPenColorThemeSwitch => BoardPenPalettePopupContent?.LaserPenColorThemeSwitch;
        internal Image BoardLaserPenColorThemeSwitchIcon => BoardPenPalettePopupContent?.LaserPenColorThemeSwitchIcon;
        internal TextBlock BoardLaserPenColorThemeSwitchTextBlock => BoardPenPalettePopupContent?.LaserPenColorThemeSwitchText;

        internal FrameworkElement NibModeSimpleStackPanel => PenPalettePopupContent?.NibModePanel ?? BoardPenPalettePopupContent?.NibModePanel;
        internal FrameworkElement BoardNibModeSimpleStackPanel => BoardPenPalettePopupContent?.NibModePanel;
        #endregion

        #region Eraser property mappings
        internal ComboBox ComboBoxEraserSizeFloatingBar => EraserPopupContent?.EraserSizeComboBox ?? BoardEraserPopupContent?.EraserSizeComboBox;
        internal ComboBox BoardComboBoxEraserSize => BoardEraserPopupContent?.EraserSizeComboBox;
        internal TabControl EraserTypeTab => EraserPopupContent?.EraserTypeTab ?? BoardEraserPopupContent?.EraserTypeTab;
        internal TabControl BoardEraserTypeTab => BoardEraserPopupContent?.EraserTypeTab;
        #endregion

        #region Gesture property mappings
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchEnableMultiTouchMode => FloatingBarGesturePopupContent?.MultiTouchToggle ?? BoardGesturePopupContent?.MultiTouchToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableMultiTouchMode => BoardGesturePopupContent?.MultiTouchToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchEnableTwoFingerTranslate => FloatingBarGesturePopupContent?.TwoFingerTranslateToggle ?? BoardGesturePopupContent?.TwoFingerTranslateToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableTwoFingerTranslate => BoardGesturePopupContent?.TwoFingerTranslateToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchEnableTwoFingerZoom => FloatingBarGesturePopupContent?.TwoFingerZoomToggle ?? BoardGesturePopupContent?.TwoFingerZoomToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableTwoFingerZoom => BoardGesturePopupContent?.TwoFingerZoomToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch ToggleSwitchEnableTwoFingerRotation => FloatingBarGesturePopupContent?.TwoFingerRotationToggle ?? BoardGesturePopupContent?.TwoFingerRotationToggle;
        internal iNKORE.UI.WPF.Modern.Controls.ToggleSwitch BoardToggleSwitchEnableTwoFingerRotation => BoardGesturePopupContent?.TwoFingerRotationToggle;
        internal FrameworkElement TwoFingerGestureSimpleStackPanel => FloatingBarGesturePopupContent?.TwoFingerGestureSimpleStackPanel;
        #endregion

        #region BackgroundPalette property mappings
        internal Slider BackgroundRSlider => BackgroundPalettePopupContent?.RSlider;
        internal Slider BackgroundGSlider => BackgroundPalettePopupContent?.GSlider;
        internal Slider BackgroundBSlider => BackgroundPalettePopupContent?.BSlider;
        internal TextBlock BackgroundRValue => BackgroundPalettePopupContent?.RValue;
        internal TextBlock BackgroundGValue => BackgroundPalettePopupContent?.GValue;
        internal TextBlock BackgroundBValue => BackgroundPalettePopupContent?.BValue;
        internal Border BackgroundColorPreview => BackgroundPalettePopupContent?.ColorPreview;
        internal Button ApplyBackgroundColorBtn => BackgroundPalettePopupContent?.ApplyBtn;
        internal Border WhiteboardModeBtn => BackgroundPalettePopupContent?.WhiteboardBtn;
        internal Border BlackboardModeBtn => BackgroundPalettePopupContent?.BlackboardBtn;
        internal Border DarkModeBtn => BackgroundPalettePopupContent?.DarkModeBtnControl;
        #endregion

        #region QuickColorPalette property mappings
        private QuickColorPaletteControl _quickColorPalette;

        internal QuickColorPaletteControl QuickColorPalette
        {
            get
            {
                if (_quickColorPalette != null) return _quickColorPalette;
                if (ToolbarHost != null)
                {
                    _quickColorPalette = ToolbarHost.FindView("builtin.quickColorPalette") as QuickColorPaletteControl;
                    if (_quickColorPalette != null) return _quickColorPalette;
                }
                if (StackPanelFloatingBarRoot != null)
                {
                    _quickColorPalette = FindDescendant<QuickColorPaletteControl>(StackPanelFloatingBarRoot);
                }
                return _quickColorPalette;
            }
        }

        private static T FindDescendant<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            var childrenCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result) return result;
                var descendant = FindDescendant<T>(child);
                if (descendant != null) return descendant;
            }
            return null;
        }
        #endregion

        internal void InitializeToolbars()
        {
            try
            {
                ToolbarRegistry.EnsureDefaultConfigExists();
                ToolbarHost = new ToolbarHost(this);
                var layout = ToolbarRegistry.LoadActiveConfig();

                // 根据设置确定工具栏方向
                var position = Settings.Appearance.ToolbarPosition;
                var orientation = (position == ToolbarPosition.Top || position == ToolbarPosition.Bottom)
                    ? Orientation.Vertical
                    : Orientation.Horizontal;

                // 设置根面板的方向和尺寸
                if (StackPanelFloatingBarRoot != null)
                {
                    StackPanelFloatingBarRoot.Orientation = orientation;
                    UpdateToolbarDimensions(orientation);
                }

                // 填充工具栏组件
                ToolbarRegistry.Populate(ToolbarHost, StackPanelFloatingBarRoot, layout, orientation);

                // 根据位置设置拖动图标的位置
                SetToolbarHeadPosition(position);

                ApplyHideFloatingBarBorder(Settings.Appearance.HideFloatingBarBorder);
                ApplyFloatingBarBorderColor();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"MW_Toolbar: InitializeToolbars 异常: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", LogHelper.LogType.Error);
            }
        }

        private void SetToolbarHeadPosition(ToolbarPosition position)
        {
            if (FloatingBarRootPanel == null) return;

            var rootChildren = FloatingBarRootPanel.Children;
            var rootList = rootChildren.OfType<FrameworkElement>().ToList();
            var dragElement = FindDragHandleInRoot();
            var otherElements = rootList.Where(c => c != dragElement).ToList();

            rootChildren.Clear();

            var reverseContent = Settings.Appearance.ReverseToolbarContent;

            switch (position)
            {
                case ToolbarPosition.Right:
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                    // 根据用户设置决定是否翻转内容面板
                    if (reverseContent)
                        ReverseAllContentPanels();
                    else
                        RestoreAllContentPanels();
                    isFloatingBarHeadOnRight = false;
                    isFloatingBarHeadOnBottom = false;
                    break;

                case ToolbarPosition.Left:
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(3, 0, 0, 0);
                        rootChildren.Add(dragElement);
                    }
                    // 根据用户设置决定是否翻转内容面板（注意：这里默认是翻转的，所以用户设置要反过来）
                    if (reverseContent)
                        RestoreAllContentPanels();
                    else
                        ReverseAllContentPanels();
                    isFloatingBarHeadOnRight = true;
                    isFloatingBarHeadOnBottom = false;
                    break;

                case ToolbarPosition.Top:
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0, 3, 0, 0);
                        rootChildren.Add(dragElement);
                    }
                    // 根据用户设置决定是否翻转内容面板（注意：这里默认是翻转的，所以用户设置要反过来）
                    if (reverseContent)
                        RestoreAllContentPanels();
                    else
                        ReverseAllContentPanels();
                    isFloatingBarHeadOnRight = false;
                    isFloatingBarHeadOnBottom = true;
                    break;

                case ToolbarPosition.Bottom:
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                    // 根据用户设置决定是否翻转内容面板
                    if (reverseContent)
                        ReverseAllContentPanels();
                    else
                        RestoreAllContentPanels();
                    isFloatingBarHeadOnRight = false;
                    isFloatingBarHeadOnBottom = false;
                    break;
            }

            SetFloatingBarHighlightPosition(_currentToolMode);
        }

        internal void RebuildToolbar()
        {
            LogHelper.WriteLogToFile("MW_Toolbar: RebuildToolbar 开始", LogHelper.LogType.Info);
            try
            {
                _lastHighlightButton = null;
                _quickColorPalette = null;
                ToolbarRegistry.ClearInjected(StackPanelFloatingBarRoot);
                InitializeToolbars();
                UpdateToolbarComponentVisibility();
                ApplyFloatingBarIconHighlightImmediate(_currentToolMode);
                RefreshFloatingBarButtonColors();
                RefreshGestureButtonIcon();
                SetFloatingBarHighlightPosition(_currentToolMode);
                ApplyCompactFloatingBarMode(Settings.Appearance.CompactFloatingBar);
                ApplyHideFloatingBarBorder(Settings.Appearance.HideFloatingBarBorder);
                ApplyFloatingBarBorderColor();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateQuickColorPaletteIndicator(inkCanvas.DefaultDrawingAttributes.Color);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
                LogHelper.WriteLogToFile("MW_Toolbar: RebuildToolbar 完成", LogHelper.LogType.Info);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"MW_Toolbar: RebuildToolbar 异常: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", LogHelper.LogType.Error);
            }
        }

        /// <summary>
        /// 紧凑模式浮动栏整体缩放倍率（相对用户设置的倍率再缩小至此比例，保持纵横比）。
        /// </summary>
        public const double CompactFloatingBarScaleFactor = 0.85;

        /// <summary>
        /// 应用紧凑浮动栏模式：遍历浮动栏中的所有 ToolbarImageButton，
        /// 开启时隐藏常驻文字标签并让图标按纵横比拉伸填满，同时整体等比缩小，关闭时恢复默认外观。
        /// </summary>
        internal void ApplyCompactFloatingBarMode(bool compact)
        {
            if (StackPanelFloatingBarRoot == null) return;
            foreach (var btn in FindVisualChildren<Controls.ToolbarImageButton>(StackPanelFloatingBarRoot))
            {
                btn.ApplyCompactMode(compact);
            }

            // 浮动栏整体等比缩小（保持纵横比）
            double baseScale = Settings.Appearance.ViewboxFloatingBarScaleTransformValue;
            if (Math.Abs(baseScale) < 0.01) baseScale = 1.0;
            double effectiveScale = compact ? baseScale * CompactFloatingBarScaleFactor : baseScale;
            ApplyRawFloatingBarScale(effectiveScale);
        }

        /// <summary>
        /// 直接设置浮动栏 ScaleTransform 的绝对值，不经过 Settings 保存。
        /// </summary>
        private void ApplyRawFloatingBarScale(double scale)
        {
            if (ViewboxFloatingBarScaleTransform == null) return;
            _userHasDraggedFloatingBar = false;
            pointDesktop = new Point(-1, -1);
            pointPPT = new Point(-1, -1);
            ViewboxFloatingBarScaleTransform.ScaleX = scale;
            ViewboxFloatingBarScaleTransform.ScaleY = scale;
            if (IsInPPTPresentationMode)
                ViewboxFloatingBarMarginAnimation(60);
            else
                ViewboxFloatingBarMarginAnimation(100, true);
        }

        /// <summary>
        /// 缓存浮动栏各 Border 的原始边框厚度，用于关闭无白边后恢复。
        /// </summary>
        private readonly System.Collections.Generic.Dictionary<System.Windows.Controls.Border, System.Windows.Thickness> _floatingBarBorderThicknessCache
            = new System.Collections.Generic.Dictionary<System.Windows.Controls.Border, System.Windows.Thickness>();

        /// <summary>
        /// 应用隐藏浮动栏边框：开启时只隐藏浮动栏外层容器边框（实现无白边），
        /// 关闭时从缓存恢复各容器 Border 原始厚度。
        /// </summary>
        internal void ApplyHideFloatingBarBorder(bool hide)
        {
            if (StackPanelFloatingBarRoot == null) return;
            foreach (var border in FindVisualChildren<System.Windows.Controls.Border>(StackPanelFloatingBarRoot))
            {
                if (border.Tag as string != Controls.Toolbar.FloatingToolbar.ToolbarRegistry.ContentBorderTag && border != BorderFloatingBarMoveControls) continue;

                if (hide)
                {
                    if (!_floatingBarBorderThicknessCache.ContainsKey(border))
                        _floatingBarBorderThicknessCache[border] = border.BorderThickness;
                    border.BorderThickness = new System.Windows.Thickness(0);
                }
                else if (_floatingBarBorderThicknessCache.TryGetValue(border, out var original))
                {
                    border.BorderThickness = original;
                }
            }
        }

        /// <summary>
        /// 边框颜色模式：0=默认（主题色），1=跟随背景颜色，2=自定义。
        /// </summary>
        private const int BorderColorMode_Default = 0;
        private const int BorderColorMode_FollowBackground = 1;
        private const int BorderColorMode_Custom = 2;

        /// <summary>
        /// 标记是否曾应用过非默认边框颜色，用于判断"默认"模式下是否需要恢复。
        /// 从未修改过时保持 false，"默认"模式完全不动 BorderBrush，与修改前行为完全一致。
        /// </summary>
        private bool _hasAppliedNonDefaultFloatingBarBorderColor = false;

        /// <summary>
        /// 应用浮动栏边框颜色：根据模式设置 BorderBrush。
        /// - 默认：若从未应用过非默认色则完全不操作（与修改前一致），否则恢复主题资源绑定 FloatBarBorderBrush
        /// - 跟随背景颜色：绑定 FloatBarBackground（亮色白/暗色深色），与背景融为一体
        /// - 自定义：直接使用用户保存的 hex 颜色
        /// 仅作用于浮动栏拖动图标 Border 及 ToolbarRegistry 创建的内容 Border，与 ApplyHideFloatingBarBorder 作用域一致。
        /// </summary>
        internal void ApplyFloatingBarBorderColor()
        {
            if (StackPanelFloatingBarRoot == null) return;

            int mode = Settings.Appearance.FloatingBarBorderColorMode;
            if (mode < 0 || mode > 2) mode = BorderColorMode_Default;

            // 默认模式下，若从未应用过非默认色，完全不操作 BorderBrush，保持修改前的原始行为
            if (mode == BorderColorMode_Default && !_hasAppliedNonDefaultFloatingBarBorderColor) return;

            _hasAppliedNonDefaultFloatingBarBorderColor = (mode != BorderColorMode_Default);

            foreach (var border in FindVisualChildren<System.Windows.Controls.Border>(StackPanelFloatingBarRoot))
            {
                if (border.Tag as string != Controls.Toolbar.FloatingToolbar.ToolbarRegistry.ContentBorderTag
                    && border != BorderFloatingBarMoveControls) continue;

                switch (mode)
                {
                    case BorderColorMode_FollowBackground:
                        border.SetResourceReference(
                            System.Windows.Controls.Border.BorderBrushProperty, "FloatingBarBackgroundBrush");
                        break;
                    case BorderColorMode_Custom:
                        var customColor = TryParseFloatingBarBorderColor(Settings.Appearance.FloatingBarBorderColor);
                        if (customColor.HasValue)
                            border.BorderBrush = new System.Windows.Media.SolidColorBrush(customColor.Value);
                        else
                            border.SetResourceReference(
                                System.Windows.Controls.Border.BorderBrushProperty, "FloatingBarBorderBrush");
                        break;
                    default:
                        border.SetResourceReference(
                            System.Windows.Controls.Border.BorderBrushProperty, "FloatingBarBorderBrush");
                        break;
                }
            }
        }

        /// <summary>
        /// 解析用户保存的浮动栏边框颜色 hex 字符串，失败返回 null。
        /// </summary>
        private static System.Windows.Media.Color? TryParseFloatingBarBorderColor(string saved)
        {
            if (string.IsNullOrWhiteSpace(saved)) return null;
            try
            {
                var text = saved.Trim();
                if (text.StartsWith("#")) text = text.Substring(1);
                if (text.Length == 6)
                    text = "FF" + text;
                return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#" + text);
            }
            catch
            {
                return null;
            }
        }

        internal bool IsAnnotating => _currentToolMode != "cursor";

        internal void UpdateToolbarComponentVisibility()
        {
            var isPPT = IsInPPTPresentationMode;
            ToolbarRegistry.UpdateVisibilityByMode(StackPanelFloatingBarRoot, IsAnnotating, isPPT);
        }

        private void UpdateToolbarDimensions(Orientation orientation)
        {
            if (StackPanelFloatingBarRoot == null) return;

            if (orientation == Orientation.Horizontal)
            {
                // 左右位置：高度固定，宽度自动
                StackPanelFloatingBarRoot.MaxHeight = 58;
                StackPanelFloatingBarRoot.ClearValue(System.Windows.FrameworkElement.MaxWidthProperty);
            }
            else
            {
                // 上下位置：宽度固定，高度自动
                StackPanelFloatingBarRoot.MaxWidth = 58;
                StackPanelFloatingBarRoot.ClearValue(System.Windows.FrameworkElement.MaxHeightProperty);
            }
        }

        /// <summary>
        /// 当前工具模式
        /// </summary>
        private string _currentToolMode = "cursor";

        #region 浮動工具欄的拖動實現

        /// <summary>
        /// 是否正在拖动浮动工具栏
        /// </summary>
        private bool isDragDropInEffect;
        /// <summary>
        /// 当前位置
        /// </summary>
        private Point pos;
        /// <summary>
        /// 按下鼠标时的位置
        /// </summary>
        private Point downPos;
        /// <summary>
        /// 用于记录上次在桌面时的坐标
        /// </summary>
        internal Point pointDesktop = new Point(-1, -1);
        /// <summary>
        /// 用于记录上次在PPT中的坐标
        /// </summary>
        internal Point pointPPT = new Point(-1, -1);
        internal bool _userHasDraggedFloatingBar;
        private DispatcherTimer _floatingBarScreenFollowTimer;
        private string _lastFloatingBarScreenDeviceName;
        private string _lastCanvasScreenDeviceName;
        private bool _isRebuildingCanvasForScreen;

        /// <summary>
        /// Popup 管理器（负责置顶和拖动跟随）
        /// </summary>
        private double _cachedFloatingBarWidth;
        private double _cachedFloatingBarHeadWidth;
        private double _cachedScreenWidth;
        private DateTime _lastFloatingBarSizeCacheTime;

        private void RefreshFloatingBarSizeCache(bool force = false)
        {
            var now = DateTime.Now;
            if (!force && (now - _lastFloatingBarSizeCacheTime).TotalMilliseconds < 100)
                return;

            var scale = GetFloatingBarScaleX();
            _cachedFloatingBarWidth = GetElementWidthForFloatingBar(ViewboxFloatingBar, 200) * scale;
            var dragElement = FindDragHandleInRoot();
            _cachedFloatingBarHeadWidth = GetElementWidthForFloatingBar(dragElement, 50) * scale;
            _cachedScreenWidth = GetFloatingBarScreenWidth(Settings.Advanced.IsEnableAvoidFullScreenHelper);
            _cachedFloatingBarHeight = GetElementHeightForFloatingBar(ViewboxFloatingBar, 58) * scale;
            _cachedFloatingBarHeadHeight = GetElementHeightForFloatingBar(dragElement, 50) * scale;
            _cachedScreenHeight = GetFloatingBarScreenHeight(Settings.Advanced.IsEnableAvoidFullScreenHelper);
            _lastFloatingBarSizeCacheTime = now;
        }

        private PopupManagerHelper _popupManager;

        /// <summary>
        /// 获取 PopupManagerHelper 实例，供插件等外部组件使用
        /// </summary>
        public PopupManagerHelper GetPopupManager() => _popupManager;

        /// <summary>
        /// 关闭所有已注册的 Popup 弹窗
        /// </summary>
        public void CloseAllPopups()
        {
            HideSubPanelsImmediately();
        }

        /// <summary>
        /// 浮动工具栏移动事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标事件参数</param>
        private void SymbolIconEmoji_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragDropInEffect)
            {
                var currentPos = e.GetPosition(null);
                var xPos = currentPos.X - pos.X + ViewboxFloatingBar.Margin.Left;
                var yPos = currentPos.Y - pos.Y + ViewboxFloatingBar.Margin.Top;
                ViewboxFloatingBar.Margin = new Thickness(xPos, yPos, -2000, -200);

                pos = currentPos;

                RefreshFloatingBarSizeCache();

                if (IsVerticalToolbar)
                {
                    if (Settings.Appearance.AutoFlipWhenSpaceInsufficient)
                    {
                        var headTop = ViewboxFloatingBar.Margin.Top + (isFloatingBarHeadOnBottom ? Math.Max(0, _cachedFloatingBarHeight - _cachedFloatingBarHeadHeight) : 0);

                        const double flipHysteresis = 20.0;
                        bool shouldFlip;
                        var vPosition = Settings.Appearance.ToolbarPosition;
                        if (vPosition == ToolbarPosition.Bottom)
                        {
                            if (!isFloatingBarHeadOnBottom && headTop + _cachedFloatingBarHeight > _cachedScreenHeight)
                                shouldFlip = true;
                            else if (isFloatingBarHeadOnBottom && headTop + _cachedFloatingBarHeight <= _cachedScreenHeight - flipHysteresis)
                                shouldFlip = false;
                            else
                                shouldFlip = isFloatingBarHeadOnBottom;
                        }
                        else
                        {
                            var toolsTopWhenUnflipped = headTop - Math.Max(0, _cachedFloatingBarHeight - _cachedFloatingBarHeadHeight);
                            if (isFloatingBarHeadOnBottom && toolsTopWhenUnflipped < 0)
                                shouldFlip = false;
                            else if (!isFloatingBarHeadOnBottom && toolsTopWhenUnflipped >= flipHysteresis)
                                shouldFlip = true;
                            else
                                shouldFlip = isFloatingBarHeadOnBottom;
                        }

                        if (shouldFlip != isFloatingBarHeadOnBottom)
                        {
                            var savedHeadTop = headTop;
                            SetFloatingBarHeadPlacementVertical(shouldFlip);

                            RefreshFloatingBarSizeCache(true);

                            double newTop;
                            if (shouldFlip)
                                newTop = savedHeadTop - Math.Max(0, _cachedFloatingBarHeight - _cachedFloatingBarHeadHeight);
                            else
                                newTop = savedHeadTop;

                            newTop = ClampFloatingBarTop(newTop, _cachedFloatingBarHeight, _cachedScreenHeight);
                            ViewboxFloatingBar.Margin = new Thickness(ViewboxFloatingBar.Margin.Left, newTop, -2000, -200);
                        }
                    }
                }
                else
                {
                    if (Settings.Appearance.AutoFlipWhenSpaceInsufficient)
                    {
                        var headLeft = ViewboxFloatingBar.Margin.Left + (isFloatingBarHeadOnRight ? Math.Max(0, _cachedFloatingBarWidth - _cachedFloatingBarHeadWidth) : 0);

                        const double flipHysteresis = 20.0;
                        bool shouldFlip;
                        var hPosition = Settings.Appearance.ToolbarPosition;
                        if (hPosition == ToolbarPosition.Right)
                        {
                            if (!isFloatingBarHeadOnRight && headLeft + _cachedFloatingBarWidth > _cachedScreenWidth)
                                shouldFlip = true;
                            else if (isFloatingBarHeadOnRight && headLeft + _cachedFloatingBarWidth <= _cachedScreenWidth - flipHysteresis)
                                shouldFlip = false;
                            else
                                shouldFlip = isFloatingBarHeadOnRight;
                        }
                        else
                        {
                            var toolsLeftWhenUnflipped = headLeft - Math.Max(0, _cachedFloatingBarWidth - _cachedFloatingBarHeadWidth);
                            if (isFloatingBarHeadOnRight && toolsLeftWhenUnflipped < 0)
                                shouldFlip = false;
                            else if (!isFloatingBarHeadOnRight && toolsLeftWhenUnflipped >= flipHysteresis)
                                shouldFlip = true;
                            else
                                shouldFlip = isFloatingBarHeadOnRight;
                        }

                        if (shouldFlip != isFloatingBarHeadOnRight)
                        {
                            var savedHeadLeft = headLeft;
                            SetFloatingBarHeadPlacement(shouldFlip);

                            RefreshFloatingBarSizeCache(true);

                            double newLeft;
                            if (shouldFlip)
                                newLeft = savedHeadLeft - Math.Max(0, _cachedFloatingBarWidth - _cachedFloatingBarHeadWidth);
                            else
                                newLeft = savedHeadLeft;

                            newLeft = ClampFloatingBarLeft(newLeft, _cachedFloatingBarWidth, _cachedScreenWidth);
                            ViewboxFloatingBar.Margin = new Thickness(newLeft, ViewboxFloatingBar.Margin.Top, -2000, -200);
                        }
                    }
                }

                var currentMargin = ViewboxFloatingBar.Margin;
                if (IsInPPTPresentationMode)
                    pointPPT = new Point(currentMargin.Left, currentMargin.Top);
                else
                    pointDesktop = new Point(currentMargin.Left, currentMargin.Top);
                _userHasDraggedFloatingBar = true;

                _popupManager?.MarkNeedsUpdate();

                if (BorderTools.IsOpen) _popupManager?.BringToFront(BorderTools);
                if (BoardBorderToolsPopup.IsOpen) _popupManager?.BringToFront(BoardBorderToolsPopup);
                if (BorderDrawShape.IsOpen) _popupManager?.BringToFront(BorderDrawShape);
                if (BoardBorderDrawShape.IsOpen) _popupManager?.BringToFront(BoardBorderDrawShape);
            }
        }

        /// <summary>
        /// 初始化 Popup 管理器（创建实例、注册 Popup、启动跟随系统）
        /// 在 Window_Loaded 中调用一次
        /// </summary>
        internal void InitializePopupManager()
        {
            try
            {
                _popupManager = new PopupManagerHelper();

                _popupManager.ShouldBeTopmost = () => Settings.Advanced.IsAlwaysOnTop;

                _popupManager.RegisterPopup(BorderTools);
                _popupManager.RegisterPopup(BoardBorderToolsPopup);
                _popupManager.RegisterPopup(BorderDrawShape);
                _popupManager.RegisterPopup(BoardBorderDrawShape);
                _popupManager.RegisterPopup(PenPalette);
                _popupManager.RegisterPopup(BoardPenPalette);
                _popupManager.RegisterPopup(EraserSizePanel);
                _popupManager.RegisterPopup(BoardEraserSizePanel);
                _popupManager.RegisterPopup(BoardImageOptionsPanel);
                _popupManager.RegisterPopup(TwoFingerGestureBorder);
                _popupManager.RegisterPopup(BoardTwoFingerGestureBorder);
                _popupManager.RegisterPopup(BoardRoamingPopup);
                _popupManager.RegisterPopup(BackgroundPalette);

                _popupManager.Initialize(this);

                System.Diagnostics.Debug.WriteLine("[PopupManager] Initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PopupManager] Initialize error: {ex.Message}");
            }
        }

        private void SymbolIconEmoji_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isViewboxFloatingBarMarginAnimationRunning)
            {
                ViewboxFloatingBar.BeginAnimation(MarginProperty, null);
                isViewboxFloatingBarMarginAnimationRunning = false;
            }

            isDragDropInEffect = true;
            pos = e.GetPosition(null);
            downPos = e.GetPosition(null);
            GridForFloatingBarDraging.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 浮动工具栏鼠标释放事件处理
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">鼠标按钮事件参数</param>
        internal void SymbolIconEmoji_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragDropInEffect = false;

            var isClick = e is null || (Math.Abs(downPos.X - e.GetPosition(null).X) <= 10 &&
                                        Math.Abs(downPos.Y - e.GetPosition(null).Y) <= 10);

            if (isClick)
            {
                var headPos = IsVerticalToolbar ? GetCurrentFloatingBarHeadTop() : GetCurrentFloatingBarHeadLeft();
                if (IsFloatingBarContentVisible())
                {
                    SetFloatingBarContentVisibility(false);
                    UpdateToolbarComponentVisibility();
                    PlaceFloatingBarAfterHeadToggle(headPos, false);
                }
                else
                {
                    SetFloatingBarContentVisibility(true);
                    UpdateToolbarComponentVisibility();
                    PlaceFloatingBarAfterHeadToggle(headPos, true);
                }
            }
            else
            {
                var headPos = IsVerticalToolbar ? GetCurrentFloatingBarHeadTop() : GetCurrentFloatingBarHeadLeft();
                PlaceFloatingBarAfterHeadToggle(
                    headPos,
                    IsFloatingBarContentVisible());
                _popupManager?.MarkNeedsUpdate();
            }

            // 每次点击或拖动结束后都重新定位高光
            SetFloatingBarHighlightPosition(_currentToolMode);

            GridForFloatingBarDraging.Visibility = Visibility.Collapsed;
        }

        #endregion 浮動工具欄的拖動實現

        /// <summary>
        /// 浮动工具栏边距动画是否正在运行
        /// </summary>
        private bool isViewboxFloatingBarMarginAnimationRunning;
        private bool isFloatingBarHeadOnRight;
        private bool isFloatingBarHeadOnBottom;
        private double _cachedFloatingBarHeight;
        private double _cachedFloatingBarHeadHeight;
        private double _cachedScreenHeight;

        private bool IsVerticalToolbar =>
            Settings.Appearance.ToolbarPosition == ToolbarPosition.Top ||
            Settings.Appearance.ToolbarPosition == ToolbarPosition.Bottom;

        private double GetFloatingBarScaleX()
        {
            var scale = ViewboxFloatingBarScaleTransform?.ScaleX ?? 1;
            return scale > 0 && !double.IsNaN(scale) && !double.IsInfinity(scale) ? scale : 1;
        }

        private double GetElementWidthForFloatingBar(FrameworkElement element, double fallbackWidth)
        {
            if (element == null) return fallbackWidth;

            var width = element.ActualWidth;
            if (width <= 0 || double.IsNaN(width)) width = element.DesiredSize.Width;
            if (width <= 0 || double.IsNaN(width)) width = element.RenderSize.Width;
            if (width <= 0 || double.IsNaN(width)) width = element.Width;

            return width > 0 && !double.IsNaN(width) && !double.IsInfinity(width) ? width : fallbackWidth;
        }

        private double GetElementHeightForFloatingBar(FrameworkElement element, double fallbackHeight)
        {
            if (element == null) return fallbackHeight;

            var height = element.ActualHeight;
            if (height <= 0 || double.IsNaN(height)) height = element.DesiredSize.Height;
            if (height <= 0 || double.IsNaN(height)) height = element.RenderSize.Height;
            if (height <= 0 || double.IsNaN(height)) height = element.Height;

            return height > 0 && !double.IsNaN(height) && !double.IsInfinity(height) ? height : fallbackHeight;
        }

        private double GetFloatingBarScaledWidth()
        {
            var baseWidth = GetElementWidthForFloatingBar(ViewboxFloatingBar, 200);
            return baseWidth * GetFloatingBarScaleX();
        }

        private double GetFloatingBarHeadScaledWidth()
        {
            var dragElement = FindDragHandleInRoot();
            return GetElementWidthForFloatingBar(dragElement, 50) * GetFloatingBarScaleX();
        }

        private double GetFloatingBarScaledHeight()
        {
            var baseHeight = GetElementHeightForFloatingBar(ViewboxFloatingBar, 58);
            return baseHeight * GetFloatingBarScaleX();
        }

        private double GetFloatingBarHeadScaledHeight()
        {
            var dragElement = FindDragHandleInRoot();
            return GetElementHeightForFloatingBar(dragElement, 50) * GetFloatingBarScaleX();
        }

        private double GetFloatingBarScreenHeight(bool useWorkingArea)
        {
            double dpiScaleY = 1;
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            var screen = GetFloatingBarTargetScreen();
            return (useWorkingArea ? screen.WorkingArea.Height : screen.Bounds.Height) / dpiScaleY;
        }

        private double GetSelectionBGLeft()
        {
            var (_, _, contentPanel) = GetFirstContentBorderElements();
            if (contentPanel == null) return 0;
            foreach (var border in FloatingBarRootPanel.Children.OfType<Border>())
            {
                if (border.Tag as string == ToolbarRegistry.ContentBorderTag && border.Child is Grid grid)
                {
                    foreach (var gridChild in grid.Children.OfType<System.Windows.Controls.Canvas>())
                    {
                        if (gridChild.Tag as string == ToolbarRegistry.SelectionCanvasTag)
                        {
                            foreach (var canvasChild in gridChild.Children.OfType<Border>())
                            {
                                if (canvasChild.Tag as string == ToolbarRegistry.SelectionBGTag)
                                {
                                    var left = System.Windows.Controls.Canvas.GetLeft(canvasChild);
                                    return double.IsNaN(left) ? 0 : left;
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private StackPanel GetFirstContentPanel()
        {
            if (FloatingBarRootPanel == null) return null;
            foreach (var border in FloatingBarRootPanel.Children.OfType<Border>())
            {
                if (border.Tag as string == ToolbarRegistry.ContentBorderTag && border.Child is Grid grid)
                {
                    foreach (var gridChild in grid.Children.OfType<StackPanel>())
                    {
                        if (gridChild.Tag as string == ToolbarRegistry.ContentPanelTag)
                            return gridChild;
                    }
                }
            }
            return null;
        }

        private FrameworkElement FindDragHandleInRoot()
        {
            if (FloatingBarRootPanel == null) return null;
            if (BorderFloatingBarMoveControls != null &&
                FloatingBarRootPanel.Children.Contains(BorderFloatingBarMoveControls))
                return BorderFloatingBarMoveControls;
            foreach (var child in FloatingBarRootPanel.Children.OfType<FrameworkElement>())
            {
                if (IsDragHandleElement(child))
                    return child;
            }
            return null;
        }

        private double GetFloatingBarScreenWidth(bool useWorkingArea)
        {
            double dpiScaleX = 1;
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
            }

            var screen = GetFloatingBarTargetScreen();
            return (useWorkingArea ? screen.WorkingArea.Width : screen.Bounds.Width) / dpiScaleX;
        }

        private void SetFloatingBarHeadPlacement(bool headOnRight)
        {
            if (FloatingBarRootPanel == null) return;

            if (IsVerticalToolbar)
            {
                SetFloatingBarHeadPlacementVertical(headOnRight);
                return;
            }

            if (isFloatingBarHeadOnRight == headOnRight) return;

            var rootChildren = FloatingBarRootPanel.Children;
            var rootList = rootChildren.OfType<FrameworkElement>().ToList();

            var dragElement = FindDragHandleInRoot();
            var otherElements = rootList.Where(c => c != dragElement).ToList();

            rootChildren.Clear();

            var flipContentOnAutoFlip = Settings.Appearance.FlipContentOnAutoFlip;

            if (headOnRight)
            {
                if (flipContentOnAutoFlip)
                {
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(3, 0, 0, 0);
                        rootChildren.Add(dragElement);
                    }
                }
                else
                {
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(3, 0, 0, 0);
                        rootChildren.Add(dragElement);
                    }

                    ReverseAllContentPanels();
                }
            }
            else
            {
                if (flipContentOnAutoFlip)
                {
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                }
                else
                {
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }

                    RestoreAllContentPanels();
                }
            }

            isFloatingBarHeadOnRight = headOnRight;

            SetFloatingBarHighlightPosition(_currentToolMode);
        }

        private void SetFloatingBarHeadPlacementVertical(bool headOnBottom)
        {
            if (FloatingBarRootPanel == null) return;
            if (isFloatingBarHeadOnBottom == headOnBottom) return;

            var rootChildren = FloatingBarRootPanel.Children;
            var rootList = rootChildren.OfType<FrameworkElement>().ToList();

            var dragElement = FindDragHandleInRoot();
            var otherElements = rootList.Where(c => c != dragElement).ToList();

            rootChildren.Clear();

            var flipContentOnAutoFlip = Settings.Appearance.FlipContentOnAutoFlip;

            if (headOnBottom)
            {
                if (flipContentOnAutoFlip)
                {
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0, 3, 0, 0);
                        rootChildren.Add(dragElement);
                    }
                }
                else
                {
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0, 3, 0, 0);
                        rootChildren.Add(dragElement);
                    }

                    ReverseAllContentPanels();
                }
            }
            else
            {
                if (flipContentOnAutoFlip)
                {
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements)
                    {
                        rootChildren.Add(elem);
                    }
                }
                else
                {
                    if (dragElement != null)
                    {
                        dragElement.Margin = new Thickness(0);
                        rootChildren.Add(dragElement);
                    }
                    foreach (var elem in otherElements.AsEnumerable().Reverse())
                    {
                        rootChildren.Add(elem);
                    }

                    RestoreAllContentPanels();
                }
            }

            isFloatingBarHeadOnBottom = headOnBottom;

            SetFloatingBarHighlightPosition(_currentToolMode);
        }

        internal void UpdateToolbarPosition()
        {
            _userHasDraggedFloatingBar = false;
            pointDesktop = new Point(-1, -1);
            pointPPT = new Point(-1, -1);

            RebuildToolbar();

            // 更新工具栏位置
            if (IsInPPTPresentationMode)
                ViewboxFloatingBarMarginAnimation(60);
            else
                PureViewboxFloatingBarMarginAnimationInDesktopMode();
        }

        private bool IsDragHandleElement(FrameworkElement element)
        {
            if (element is Border border)
            {
                if (border.Name == "BorderFloatingBarMoveControls") return true;
                var child = border.Child;
                if (child is Image) return true;
                if (child is StackPanel panel && panel.Children.Count > 0 && panel.Children[0] is Image)
                    return true;
            }
            return false;
        }

        private IEnumerable<StackPanel> GetAllPanelsWithContent(DependencyObject root)
        {
            if (root == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                if (child is StackPanel panel && (panel.Orientation == System.Windows.Controls.Orientation.Horizontal ||
                                                  panel.Orientation == System.Windows.Controls.Orientation.Vertical))
                {
                    yield return panel;
                }

                foreach (var nestedPanel in GetAllPanelsWithContent(child))
                {
                    yield return nestedPanel;
                }
            }
        }

        private Dictionary<StackPanel, List<FrameworkElement>> _normalContentOrders;

        private void ReverseAllContentPanels()
        {
            _normalContentOrders = new Dictionary<StackPanel, List<FrameworkElement>>();
            foreach (var panel in GetAllPanelsWithContent(FloatingBarRootPanel))
            {
                _normalContentOrders[panel] = panel.Children.OfType<FrameworkElement>().ToList();
                var reversed = panel.Children.OfType<FrameworkElement>().Reverse().ToList();
                panel.Children.Clear();
                foreach (var child in reversed)
                    panel.Children.Add(child);
            }
        }

        private void RestoreAllContentPanels()
        {
            if (_normalContentOrders == null) return;
            foreach (var kvp in _normalContentOrders)
            {
                var panel = kvp.Key;
                var normalOrder = kvp.Value;
                var current = panel.Children.OfType<FrameworkElement>().ToList();
                if (current.SequenceEqual(normalOrder)) continue;
                panel.Children.Clear();
                foreach (var child in normalOrder)
                {
                    if (child.Parent != null && child.Parent != panel)
                        continue;
                    if (!panel.Children.Contains(child))
                        panel.Children.Add(child);
                }
            }
            _normalContentOrders = null;
        }

        private bool IsFloatingBarContentVisible()
        {
            return !ToolbarRegistry.IsContentCollapsedByUser;
        }

        private void SetFloatingBarContentVisibility(bool visible)
        {
            ToolbarRegistry.IsContentCollapsedByUser = !visible;
            ToolbarRegistry.UpdateVisibilityByMode(
                FloatingBarRootPanel,
                IsAnnotating,
                IsInPPTPresentationMode);
        }

        private double ClampFloatingBarLeft(double left, double floatingBarWidth, double screenWidth)
        {
            var maxLeft = Math.Max(0, screenWidth - floatingBarWidth);
            return Math.Max(0, Math.Min(left, maxLeft));
        }

        private double ClampFloatingBarTop(double top, double floatingBarHeight, double screenHeight)
        {
            var maxTop = Math.Max(0, screenHeight - floatingBarHeight);
            return Math.Max(0, Math.Min(top, maxTop));
        }

        #region 动态按钮位置计算和高光显示

        /// <summary>
        /// 获取浮动栏中指定按钮的位置
        /// </summary>
        /// <param name="buttonName">按钮的名称</param>
        /// <returns>按钮在浮动栏中的相对位置</returns>
        private double GetFloatingBarButtonPosition(string buttonName)
        {
            try
            {
                // 获取浮动栏容器
                var floatingBarPanel = GetFirstContentPanel();
                if (floatingBarPanel == null) return 0;

                double currentPosition = 0;

                foreach (var child in floatingBarPanel.Children)
                {
                    if (child is UIElement element)
                    {
                        // 检查是否是我们要找的按钮
                        if (IsTargetButton(element, buttonName))
                        {
                            return currentPosition;
                        }

                        // 累加当前元素的位置
                        currentPosition += GetElementWidth(element);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"获取按钮位置失败: {ex.Message}", LogHelper.LogType.Error);
                return 0;
            }
        }

        /// <summary>
        /// 检查元素是否是目标按钮
        /// </summary>
        private bool IsTargetButton(UIElement element, string buttonName)
        {
            if (element is FrameworkElement fe)
            {
                return fe.Name == buttonName;
            }
            return false;
        }

        /// <summary>
        /// 获取元素的宽度
        /// </summary>
        private double GetElementWidth(UIElement element)
        {
            if (element is FrameworkElement fe)
            {
                return fe.ActualWidth > 0 ? fe.ActualWidth : 28;
            }
            return 28; // 默认宽度
        }

        /// <summary>
        /// 更新浮动栏批注图标颜色，使其反映当前画笔颜色（需开启 ShowPenColorOnFloatingBarIcon）
        /// </summary>
        internal void UpdatePenIconColor()
        {
            if (!Settings.Appearance.ShowPenColorOnFloatingBarIcon) return;
            if (Pen_Icon == null || Pen_Icon.Icon == null) return;
            if (_currentToolMode != "pen" && _currentToolMode != "color") return;

            try
            {
                var inkColor = inkCanvas.DefaultDrawingAttributes.Color;
                Pen_Icon.Icon.Brush = new SolidColorBrush(inkColor);
            }
            catch { }
        }

        /// <summary>
        /// 设置浮动栏高光显示位置
        /// </summary>
        /// <param name="mode">模式名称</param>
        private ToolbarImageButton _lastHighlightButton;
        private int _indicatorAnimationGeneration;
        private Storyboard _activeIndicatorStoryboard;
        private string _pendingHighlightMode;
        private int _highlightPositionVersion;
        private int _highlightLayoutRetryCount;

        private void SetFloatingBarHighlightPosition(string mode)
        {
            mode = NormalizeToolModeForFreeze(mode);

            ApplyFloatingBarIconHighlightImmediate(mode);

            _pendingHighlightMode = mode;
            _highlightLayoutRetryCount = 0;
            int version = ++_highlightPositionVersion;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_highlightPositionVersion != version) return;
                AnimateFloatingBarHighlightTo(mode);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void ApplyFloatingBarIconHighlightImmediate(string mode)
        {
            try
            {
                Color highlightBarColor;
                bool isDarkTheme = Settings.Appearance.Theme == 1 ||
                                   (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());

                if (isDarkTheme)
                    highlightBarColor = Color.FromRgb(102, 204, 255);
                else
                    highlightBarColor = Color.FromRgb(37, 99, 235);

                if (isFloatingBarFolded || (BorderFloatingBarMoveControls != null && BorderFloatingBarMoveControls.Visibility == Visibility.Collapsed))
                {
                    return;
                }

                var foregroundBrush = new SolidColorBrush(FloatBarForegroundColor);

                void ResetIcon(ToolbarImageButton button, string iconType)
                {
                    if (button == null) return;
                    if (!ToolbarRegistry.GetUseRedStyle(button))
                        button.Icon.Brush = foregroundBrush;
                    button.Icon.Geometry = Geometry.Parse(GetCorrectIcon(iconType, false));
                }

                ResetIcon(Cursor_Icon, "cursor");
                ResetIcon(Pen_Icon, "pen");
                ResetIcon(Eraser_Icon, "eraserCircle");
                ResetIcon(EraserByStrokes_Icon, "eraserStroke");
                ResetIcon(SymbolIconSelect, "lassoSelect");

                string targetIconType = null;
                ToolbarImageButton targetButton = null;

                switch (mode)
                {
                    case "cursor":
                        targetButton = Cursor_Icon;
                        targetIconType = "cursor";
                        break;
                    case "pen":
                    case "color":
                        targetButton = Pen_Icon;
                        targetIconType = "pen";
                        break;
                    case "eraser":
                        targetButton = Eraser_Icon;
                        targetIconType = "eraserCircle";
                        break;
                    case "eraserByStrokes":
                        targetButton = EraserByStrokes_Icon;
                        targetIconType = "eraserStroke";
                        break;
                    case "select":
                        targetButton = SymbolIconSelect;
                        targetIconType = "lassoSelect";
                        break;
                    case "shape":
                        targetButton = ShapeDrawFloatingBarBtn;
                        break;
                }

                if (targetButton != null && targetIconType != null)
                {
                    if (!ToolbarRegistry.GetUseRedStyle(targetButton))
                    {
                        if (Settings.Appearance.ShowPenColorOnFloatingBarIcon && targetButton == Pen_Icon)
                            targetButton.Icon.Brush = new SolidColorBrush(inkCanvas.DefaultDrawingAttributes.Color);
                        else
                            targetButton.Icon.Brush = new SolidColorBrush(highlightBarColor);
                    }
                    targetButton.Icon.Geometry = Geometry.Parse(GetCorrectIcon(targetIconType, true));
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "更新浮动栏图标高亮状态失败", LogHelper.LogType.Warning);
            }
        }

        private void AnimateFloatingBarHighlightTo(string mode)
        {
            try
            {
                var selectionBG = SelectionBGFloatingBar;
                var indicatorBar = IndicatorBarFloatingBar;
                var container = GridFloatingBarContainer;

                if (selectionBG == null || indicatorBar == null || container == null) return;

                ToolbarImageButton targetButton = null;

                switch (mode)
                {
                    case "cursor":
                        targetButton = Cursor_Icon;
                        break;
                    case "pen":
                    case "color":
                        targetButton = Pen_Icon;
                        break;
                    case "eraser":
                        targetButton = Eraser_Icon;
                        break;
                    case "eraserByStrokes":
                        targetButton = EraserByStrokes_Icon;
                        break;
                    case "select":
                        targetButton = SymbolIconSelect;
                        break;
                    case "shape":
                        targetButton = ShapeDrawFloatingBarBtn;
                        break;
                }

                if (targetButton == null || !IsElementVisibleInTree(targetButton))
                {
                    // 如果目标按钮不可见则隐藏高光
                    HideAllSelectionHighlights();
                    return;
                }

                Point nextButtonOrigin;
                try
                {
                    nextButtonOrigin = targetButton.TransformToAncestor(container).Transform(new Point(0, 0));
                }
                catch (InvalidOperationException)
                {
                    DeferFloatingBarHighlightIfLayoutPending(mode);
                    return;
                }

                double nextWidth = targetButton.ActualWidth > 0 ? targetButton.ActualWidth : 44;
                double nextHeight = targetButton.ActualHeight > 0 ? targetButton.ActualHeight : 44;
                double nextPos = nextButtonOrigin.X;
                double nextTop = nextButtonOrigin.Y;

                if (nextWidth <= 0)
                {
                    DeferFloatingBarHighlightIfLayoutPending(mode);
                    return;
                }

                Color highlightBackgroundColor;
                Color highlightBarColor;
                bool isDarkTheme = Settings.Appearance.Theme == 1 ||
                                   (Settings.Appearance.Theme == 2 && !ThemeHelper.IsSystemThemeLight());

                if (isDarkTheme)
                {
                    highlightBackgroundColor = Color.FromArgb(48, 102, 204, 255);
                    highlightBarColor = Color.FromRgb(102, 204, 255);
                }
                else
                {
                    highlightBackgroundColor = Color.FromArgb(48, 59, 130, 246);
                    highlightBarColor = Color.FromRgb(37, 99, 235);
                }

                selectionBG.Background = new SolidColorBrush(highlightBackgroundColor);
                indicatorBar.Background = new SolidColorBrush(highlightBarColor);

                bool isVertical = IsVerticalToolbar;
                double indicatorBarSize = 16;

                double nextBarLeft, nextBarTop;
                double selectionWidth, selectionHeight;

                if (isVertical)
                {
                    selectionWidth = 43;
                    selectionHeight = nextHeight;
                    nextBarLeft = nextPos + 2 + 43 + 2;
                    nextBarTop = nextTop + Math.Max(0, (nextHeight - indicatorBarSize) / 2);
                }
                else
                {
                    selectionWidth = nextWidth;
                    selectionHeight = 43;
                    nextBarLeft = nextPos + Math.Max(0, (nextWidth - indicatorBarSize) / 2);
                    nextBarTop = nextTop + 2 + 43 + 2;
                }

                bool isFirstShow = _lastHighlightButton == null;

                if (isFirstShow)
                {
                    selectionBG.Width = selectionWidth;
                    selectionBG.Height = selectionHeight;
                    System.Windows.Controls.Canvas.SetLeft(selectionBG, isVertical ? nextPos + 2 : nextPos);
                    System.Windows.Controls.Canvas.SetTop(selectionBG, isVertical ? nextTop : nextTop + 2);

                    _indicatorAnimationGeneration++;
                    indicatorBar.RenderTransform = null;
                    indicatorBar.Visibility = Visibility.Visible;
                    indicatorBar.Width = isVertical ? 3 : indicatorBarSize;
                    indicatorBar.Height = isVertical ? indicatorBarSize : 3;
                    indicatorBar.Opacity = 1.0;
                    System.Windows.Controls.Canvas.SetLeft(indicatorBar, nextBarLeft);
                    System.Windows.Controls.Canvas.SetTop(indicatorBar, nextBarTop);

                    selectionBG.Visibility = Visibility.Visible;
                    if (!Settings.Appearance.DisableToolbarAnimation)
                        targetButton.SetSelectedVisualOffset(true);
                    _lastHighlightButton = targetButton;
                    return;
                }

                double prevBarPos;
                if (_lastHighlightButton != null && IsElementVisibleInTree(_lastHighlightButton))
                {
                    try
                    {
                        var prevOrigin = _lastHighlightButton.TransformToAncestor(container).Transform(new Point(0, 0));
                        double prevSize = isVertical
                            ? (_lastHighlightButton.ActualHeight > 0 ? _lastHighlightButton.ActualHeight : 44)
                            : (_lastHighlightButton.ActualWidth > 0 ? _lastHighlightButton.ActualWidth : 44);
                        prevBarPos = isVertical
                            ? prevOrigin.Y + Math.Max(0, (prevSize - indicatorBarSize) / 2)
                            : prevOrigin.X + Math.Max(0, (prevSize - indicatorBarSize) / 2);
                    }
                    catch (InvalidOperationException)
                    {
                        prevBarPos = isVertical
                            ? (System.Windows.Controls.Canvas.GetTop(indicatorBar))
                            : (System.Windows.Controls.Canvas.GetLeft(indicatorBar));
                        if (double.IsNaN(prevBarPos)) prevBarPos = isVertical ? nextBarTop : nextBarLeft;
                    }
                }
                else
                {
                    prevBarPos = isVertical
                        ? (System.Windows.Controls.Canvas.GetTop(indicatorBar))
                        : (System.Windows.Controls.Canvas.GetLeft(indicatorBar));
                    if (double.IsNaN(prevBarPos)) prevBarPos = isVertical ? nextBarTop : nextBarLeft;
                }

                double nextBarPos = isVertical ? nextBarTop : nextBarLeft;

                var prevHighlightButton = _lastHighlightButton;
                _lastHighlightButton = targetButton;

                if (!Settings.Appearance.DisableToolbarAnimation)
                {
                    if (prevHighlightButton != null && prevHighlightButton != targetButton)
                        prevHighlightButton.SetSelectedVisualOffset(false);
                    targetButton.SetSelectedVisualOffset(true);
                }

                selectionBG.Width = selectionWidth;
                selectionBG.Height = selectionHeight;
                System.Windows.Controls.Canvas.SetLeft(selectionBG, isVertical ? nextPos + 2 : nextPos);
                System.Windows.Controls.Canvas.SetTop(selectionBG, isVertical ? nextTop : nextTop + 2);
                selectionBG.Visibility = Visibility.Visible;

                indicatorBar.Visibility = Visibility.Visible;

                double distance = Math.Abs(nextBarPos - prevBarPos);

                if (distance < 0.5)
                {
                    _indicatorAnimationGeneration++;
                    indicatorBar.RenderTransform = null;
                    indicatorBar.Width = isVertical ? 3 : indicatorBarSize;
                    indicatorBar.Height = isVertical ? indicatorBarSize : 3;
                    System.Windows.Controls.Canvas.SetLeft(indicatorBar, nextBarLeft);
                    System.Windows.Controls.Canvas.SetTop(indicatorBar, nextBarTop);
                    return;
                }

                if (_activeIndicatorStoryboard != null)
                {
                    var oldStoryboard = _activeIndicatorStoryboard;
                    _activeIndicatorStoryboard = null;
                    try { oldStoryboard.Stop(indicatorBar); } catch { }
                    indicatorBar.RenderTransform = null;
                    indicatorBar.Opacity = 1.0;
                }

                _indicatorAnimationGeneration++;
                indicatorBar.RenderTransform = null;

                double from = prevBarPos - nextBarPos;
                double to = 0;
                double dimension = indicatorBarSize;
                double stretchScale = distance / dimension + 1.0;

                System.Windows.Controls.Canvas.SetLeft(indicatorBar, nextBarLeft);
                System.Windows.Controls.Canvas.SetTop(indicatorBar, nextBarTop);
                indicatorBar.Width = isVertical ? 3 : indicatorBarSize;
                indicatorBar.Height = isVertical ? indicatorBarSize : 3;

                indicatorBar.RenderTransform = new TransformGroup
                {
                    Children =
                    {
                        new ScaleTransform(),
                        new TranslateTransform()
                    }
                };

                var storyboard = new Storyboard { FillBehavior = FillBehavior.Stop };

                var posAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(from < to ? from : (from + (dimension * (1.0 - 1))), KeyTime.FromPercent(0.0)),
                        new DiscreteDoubleKeyFrame(from < to ? (to + (dimension * (1.0 - 1))) : to, KeyTime.FromPercent(0.333)),
                    },
                    Duration = TimeSpan.FromMilliseconds(600)
                };

                var scaleAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(1.0, KeyTime.FromPercent(0.0)),
                        new SplineDoubleKeyFrame(stretchScale, KeyTime.FromPercent(0.333), new KeySpline(new Point(0.9, 0.1), new Point(1.0, 0.2))),
                        new SplineDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0), new KeySpline(new Point(0.1, 0.9), new Point(0.2, 1.0)))
                    },
                    Duration = TimeSpan.FromMilliseconds(600)
                };

                var centerAnim = new DoubleAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteDoubleKeyFrame(from < to ? 0.0 : dimension, KeyTime.FromPercent(0.0)),
                        new DiscreteDoubleKeyFrame(from < to ? dimension : 0.0, KeyTime.FromPercent(0.333)),
                    },
                    Duration = TimeSpan.FromMilliseconds(600)
                };

                Storyboard.SetTarget(posAnim, indicatorBar);
                Storyboard.SetTarget(scaleAnim, indicatorBar);
                Storyboard.SetTarget(centerAnim, indicatorBar);

                if (isVertical)
                {
                    Storyboard.SetTargetProperty(posAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
                    Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
                    Storyboard.SetTargetProperty(centerAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterY)"));
                }
                else
                {
                    Storyboard.SetTargetProperty(posAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
                    Storyboard.SetTargetProperty(scaleAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));
                    Storyboard.SetTargetProperty(centerAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.CenterX)"));
                }

                storyboard.Children.Add(posAnim);
                storyboard.Children.Add(scaleAnim);
                storyboard.Children.Add(centerAnim);

                int currentGeneration = _indicatorAnimationGeneration;
                _activeIndicatorStoryboard = storyboard;
                storyboard.Completed += (s, e) =>
                {
                    if (currentGeneration != _indicatorAnimationGeneration) return;
                    _activeIndicatorStoryboard = null;
                    indicatorBar.RenderTransform = null;
                };

                storyboard.Begin(indicatorBar, true);
                storyboard.Pause(indicatorBar);
                storyboard.SeekAlignedToLastTick(indicatorBar, TimeSpan.Zero, TimeSeekOrigin.BeginTime);
                Dispatcher.BeginInvoke(() =>
                {
                    storyboard.Resume(indicatorBar);
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"设置高光位置失败: {ex.Message}", LogHelper.LogType.Error);
            }
        }

        /// <summary>
        /// 通用子面板位置更新方法：根据触发按钮的位置，动态调整子面板的水平位置，
        /// 使面板水平中心对齐按钮中心。不改变面板大小，不改变上下边距。
        /// </summary>
        /// <param name="button">触发按钮元素</param>
        /// <param name="panel">需要定位的子面板</param>
        /// <param name="defaultPanelWidth">面板默认宽度（当无法从Margin计算时使用）</param>
        private void UpdateSubPanelPosition(FrameworkElement button, FrameworkElement panel, double defaultPanelWidth)
        {
            try
            {
                if (button == null || panel == null) return;

                if (panel is Popup popup)
                {
                    if (popup.PlacementTarget == null)
                    {
                        popup.PlacementTarget = button;
                    }

                    if (popup.IsOpen)
                    {
                        _popupManager?.UpdatePosition(popup);
                    }

                    return;
                }

                if (!(panel.Parent is FrameworkElement panelContainer)) return;

                var ancestor = StackPanelFloatingBarRoot;
                if (ancestor == null) return;

                var buttonTransform = button.TransformToAncestor(ancestor);
                var buttonOrigin = buttonTransform.Transform(new Point(0, 0));
                double buttonCenterX = buttonOrigin.X + button.ActualWidth / 2.0;

                var containerTransform = panelContainer.TransformToAncestor(ancestor);
                var containerOrigin = containerTransform.Transform(new Point(0, 0));
                double containerX = containerOrigin.X;

                // 计算当前面板宽度（保持不变）：panelWidth = -Margin.Left - Margin.Right
                double currentLeft = panel.Margin.Left;
                double currentRight = panel.Margin.Right;
                double panelWidth = -currentLeft - currentRight;
                if (panelWidth <= 0) panelWidth = defaultPanelWidth;

                // 计算新的左边距，使面板水平中心对齐按钮：
                //   panel_center = containerX + newLeft + panelWidth/2 = buttonCenterX
                //   => newLeft = buttonCenterX - containerX - panelWidth/2
                double newLeft = buttonCenterX - containerX - panelWidth / 2.0;

                // 保持面板宽度不变：-newLeft - newRight = panelWidth
                //   => newRight = -panelWidth - newLeft
                double newRight = -panelWidth - newLeft;

                // 清除可能残留的 Margin 动画（HoldEnd 会阻止本地值生效）
                panel.BeginAnimation(FrameworkElement.MarginProperty, null);

                // 更新边距，仅调整Left/Right，保持Top/Bottom不变
                var margin = panel.Margin;
                panel.Margin = new Thickness(newLeft, margin.Top, newRight, margin.Bottom);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"更新子面板位置失败: {ex.Message}", LogHelper.LogType.Error);
            }
        }

        /// <summary>
        /// 更新批注子面板（PenPalette）的弹出位置，使其水平中心对齐笔按钮。
        /// </summary>
        private void UpdatePenPalettePosition()
        {
            UpdateSubPanelPosition(Pen_Icon, PenPalette, 193);
        }

        /// <summary>
        /// 更新工具面板（BorderTools）的弹出位置，使其水平中心对齐工具按钮。
        /// </summary>
        private void UpdateBorderToolsPosition()
        {
            UpdateSubPanelPosition(ToolsFloatingBarBtn, BorderTools, 119);
        }

        /// <summary>
        /// 更新橡皮擦尺寸面板（EraserSizePanel）的弹出位置，使其水平中心对齐橡皮擦按钮。
        /// </summary>
        private void UpdateEraserSizePanelPosition()
        {
            UpdateSubPanelPosition(Eraser_Icon, EraserSizePanel, 120);
        }

        /// <summary>
        /// 更新手势面板（TwoFingerGestureBorder）的弹出位置，使其水平中心对齐手势按钮。
        /// </summary>
        private void UpdateTwoFingerGestureBorderPosition()
        {
            UpdateSubPanelPosition(Gesture_Icon, TwoFingerGestureBorder, 119);
        }

        /// <summary>
        /// 隐藏浮动栏高光显示
        /// </summary>
        private void HideFloatingBarHighlight()
        {
            HideAllSelectionHighlights();
            _lastHighlightButton = null;
        }

        private void DeferFloatingBarHighlightIfLayoutPending(string mode)
        {
            if (string.IsNullOrEmpty(mode) || _highlightLayoutRetryCount >= 3)
            {
                HideAllSelectionHighlights();
                return;
            }

            _highlightLayoutRetryCount++;
            int version = ++_highlightPositionVersion;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_highlightPositionVersion != version) return;
                AnimateFloatingBarHighlightTo(mode);
            }), DispatcherPriority.ContextIdle);
        }

        private void HideAllSelectionHighlights()
        {
            if (_lastHighlightButton != null)
                _lastHighlightButton.SetSelectedVisualOffset(false);
            if (SelectionBGFloatingBar != null)
            {
                SelectionBGFloatingBar.Visibility = Visibility.Hidden;
            }
            if (IndicatorBarFloatingBar != null)
            {
                IndicatorBarFloatingBar.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, null);
                IndicatorBarFloatingBar.RenderTransform = null;
                IndicatorBarFloatingBar.Visibility = Visibility.Hidden;
            }
            _lastHighlightButton = null;
        }

        private (Border selectionBG, Border indicatorBar, StackPanel contentPanel) FindSelectionElementsForMode(string mode)
        {
            ToolbarImageButton targetButton = null;
            switch (mode)
            {
                case "cursor": targetButton = Cursor_Icon; break;
                case "pen":
                case "color": targetButton = Pen_Icon; break;
                case "eraser": targetButton = Eraser_Icon; break;
                case "eraserByStrokes": targetButton = EraserByStrokes_Icon; break;
                case "select": targetButton = SymbolIconSelect; break;
                case "shape": targetButton = ShapeDrawFloatingBarBtn; break;
            }
            return FindSelectionElementsForButton(targetButton);
        }

        private (Border selectionBG, Border indicatorBar, StackPanel contentPanel) FindSelectionElementsForButton(ToolbarImageButton button)
        {
            if (button == null || FloatingBarRootPanel == null) return (null, null, null);

            foreach (var border in FloatingBarRootPanel.Children.OfType<Border>())
            {
                if (border.Tag as string != ToolbarRegistry.ContentBorderTag || !(border.Child is Grid grid)) continue;

                StackPanel contentPanel = null;
                System.Windows.Controls.Canvas selectionCanvas = null;

                foreach (var gridChild in grid.Children.OfType<FrameworkElement>())
                {
                    if (gridChild is StackPanel sp && sp.Tag as string == ToolbarRegistry.ContentPanelTag)
                        contentPanel = sp;
                    else if (gridChild is System.Windows.Controls.Canvas canvas && canvas.Tag as string == ToolbarRegistry.SelectionCanvasTag)
                        selectionCanvas = canvas;
                }

                if (contentPanel == null) continue;

                bool containsButton = ContainsButton(contentPanel, button);
                if (containsButton)
                {
                    Border selectionBG = null;
                    Border indicatorBar = null;
                    if (selectionCanvas != null)
                    {
                        foreach (var canvasChild in selectionCanvas.Children.OfType<Border>())
                        {
                            if (canvasChild.Tag as string == ToolbarRegistry.SelectionBGTag)
                                selectionBG = canvasChild;
                            else if (canvasChild.Tag as string == ToolbarRegistry.IndicatorBarTag)
                                indicatorBar = canvasChild;
                        }
                    }
                    return (selectionBG, indicatorBar, contentPanel);
                }
            }

            var firstResult = GetFirstContentBorderElements();
            return firstResult;
        }

        private static bool ContainsButton(Panel panel, ToolbarImageButton button)
        {
            foreach (var child in panel.Children)
            {
                if (child == button) return true;
                if (child is Panel innerPanel && ContainsButton(innerPanel, button)) return true;
                if (child is ContentControl cc && cc.Content == button) return true;
                if (child is Decorator decorator && decorator.Child == button) return true;
            }
            return false;
        }

        private StackPanel FindContentPanelForButton(ToolbarImageButton button)
        {
            if (button == null || FloatingBarRootPanel == null) return null;

            foreach (var border in FloatingBarRootPanel.Children.OfType<Border>())
            {
                if (border.Tag as string != ToolbarRegistry.ContentBorderTag || !(border.Child is Grid grid)) continue;

                foreach (var gridChild in grid.Children.OfType<StackPanel>())
                {
                    if (gridChild.Tag as string == ToolbarRegistry.ContentPanelTag && ContainsButton(gridChild, button))
                        return gridChild;
                }
            }
            return null;
        }

        private (Border, Border, StackPanel) GetFirstContentBorderElements()
        {
            if (FloatingBarRootPanel == null) return (null, null, null);

            foreach (var border in FloatingBarRootPanel.Children.OfType<Border>())
            {
                if (border.Tag as string != ToolbarRegistry.ContentBorderTag || !(border.Child is Grid grid)) continue;

                Border selectionBG = null;
                Border indicatorBar = null;
                StackPanel contentPanel = null;

                foreach (var gridChild in grid.Children.OfType<FrameworkElement>())
                {
                    if (gridChild is StackPanel sp && sp.Tag as string == ToolbarRegistry.ContentPanelTag)
                        contentPanel = sp;
                    else if (gridChild is System.Windows.Controls.Canvas canvas && canvas.Tag as string == ToolbarRegistry.SelectionCanvasTag)
                    {
                        foreach (var canvasChild in canvas.Children.OfType<Border>())
                        {
                            if (canvasChild.Tag as string == ToolbarRegistry.SelectionBGTag)
                                selectionBG = canvasChild;
                            else if (canvasChild.Tag as string == ToolbarRegistry.IndicatorBarTag)
                                indicatorBar = canvasChild;
                        }
                    }
                }

                if (contentPanel != null)
                    return (selectionBG, indicatorBar, contentPanel);
            }
            return (null, null, null);
        }

        private static bool IsDescendantOf(DependencyObject child, DependencyObject parent)
        {
            if (child == null || parent == null) return false;
            var current = LogicalTreeHelper.GetParent(child);
            while (current != null)
            {
                if (current == parent) return true;
                current = LogicalTreeHelper.GetParent(current);
            }
            current = System.Windows.Media.VisualTreeHelper.GetParent(child);
            while (current != null)
            {
                if (current == parent) return true;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        private bool IsElementVisibleInTree(FrameworkElement element)
        {
            if (element == null || element.Visibility != Visibility.Visible) return false;
            var parent = VisualTreeHelper.GetParent(element);
            while (parent != null)
            {
                if (parent is FrameworkElement fe && fe.Visibility != Visibility.Visible) return false;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return true;
        }

        /// <summary>
        /// 获取当前选中的模式
        /// </summary>
        /// <returns>当前选中的模式名称</returns>
        public string GetCurrentSelectedMode()
        {
            try
            {
                // 优先使用缓存的模式，避免在浮动栏刷新时返回过时的模式信息
                if (!string.IsNullOrEmpty(_currentToolMode))
                {
                    return _currentToolMode;
                }

                // 如果缓存为空，则从inkCanvas状态推断模式
                if (inkCanvas.EditingMode == InkCanvasEditingMode.Select)
                {
                    return "select";
                }

                if (inkCanvas.EditingMode == InkCanvasEditingMode.Ink)
                {
                    // 检查是否是荧光笔模式
                    if (drawingAttributes != null && drawingAttributes.IsHighlighter)
                    {
                        return "color";
                    }

                    return "pen";
                }

                if (inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint)
                {
                    // 检查是面积擦还是线擦
                    if (Eraser_Icon != null && Eraser_Icon.Visibility == Visibility.Visible)
                    {
                        return "eraser";
                    }

                    if (EraserByStrokes_Icon != null && EraserByStrokes_Icon.Visibility == Visibility.Visible)
                    {
                        return "eraserByStrokes";
                    }
                }
                else if (inkCanvas.EditingMode == InkCanvasEditingMode.None)
                {
                    // Native freehand keeps physical EditingMode at None while logical
                    // tool remains pen/color. Prefer cached mode; fall back to cursor.
                    if (!string.IsNullOrEmpty(_currentToolMode)
                        && !string.Equals(_currentToolMode, "cursor", StringComparison.OrdinalIgnoreCase))
                        return _currentToolMode;
                    return "cursor";
                }
                else if (drawingShapeMode != 0)
                {
                    return "shape";
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"获取当前选中模式失败: {ex.Message}", LogHelper.LogType.Error);
            }

            return "cursor"; // 默认返回鼠标模式
        }

        /// <summary>
        /// 更新当前工具模式缓存
        /// </summary>
        /// <param name="mode">模式名称</param>
        private void UpdateCurrentToolMode(string mode)
        {
            _currentToolMode = NormalizeToolModeForFreeze(mode);
            UpdateBoardRoamingButtonState();

            // Issue #285 更小批注栏：根据当前工具模式刷新迷你栏显示状态
            RefreshIdleMiniBarState();

            // 通知自动化系统：逻辑工具模式已变化。原生笔路径下物理 EditingMode 不变，
            // 触发器无法靠 EditingModeChanged 感知进/出批注，必须在此显式通知。
            AutomationBootstrap.Monitor?.NotifyInternalStateChanged();

            // 工具模式变化后同步刷新工具栏形态（批注/鼠标布局按 IsAnnotating 决定），
            // 否则会出现指示器已切到鼠标、形态仍停在批注的失步（退出白板时的两步走流程即如此）。
            UpdateToolbarComponentVisibility();
        }

        /// <summary>
        /// 自动化「切换批注模式」动作入口：进入/退出批注模式。
        /// 与画笔/光标按钮保持一致的 SetCurrentToolMode + UpdateCurrentToolMode 序列，
        /// 保证 _currentToolMode（逻辑工具）与原生湿墨迹管线同步。
        /// </summary>
        internal void SetAnnotationModeFromAutomation(bool enterAnnotation)
        {
            if (enterAnnotation)
            {
                if (SetCurrentToolMode(InkCanvasEditingMode.Ink))
                    UpdateCurrentToolMode("pen");
            }
            else
            {
                if (SetCurrentToolMode(InkCanvasEditingMode.None))
                    UpdateCurrentToolMode("cursor");
            }
        }

        #endregion

    }
}
