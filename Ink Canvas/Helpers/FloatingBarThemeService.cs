using System;
using System.Windows;
using System.Windows.Media;

namespace Ink_Canvas.Helpers
{
    /// <summary>
    /// 应用内置浮动工具栏外观和可选的彩色背景。
    /// </summary>
    public sealed class FloatingBarThemeService
    {
        private readonly MainWindow _mainWindow;
        private ResourceDictionary _themeDictionary;
        private ResourceDictionary _colorfulOverlayDictionary;

        public FloatingBarThemeService(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ApplyBuiltInTheme()
        {
            try
            {
                var dictionary = CreateBuiltInThemeDictionary();
                var resources = Application.Current.Resources;
                if (_themeDictionary != null) resources.MergedDictionaries.Remove(_themeDictionary);
                _themeDictionary = dictionary;
                resources.MergedDictionaries.Add(dictionary);
                ApplyColorfulOverlay();
                _mainWindow.ApplyFloatingBarBorderColor();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"应用内置浮动栏外观失败: {ex.Message}", LogHelper.LogType.Warning);
            }
        }

        private ResourceDictionary CreateBuiltInThemeDictionary()
        {
            var dictionary = new ResourceDictionary();
            dictionary["FloatingBarBackgroundBrush"] = Application.Current.TryFindResource("FloatBarBackground") ?? new SolidColorBrush(Color.FromArgb(0xF2, 0x1A, 0x1C, 0x1E));
            dictionary["FloatingBarForegroundBrush"] = Application.Current.TryFindResource("FloatBarForeground") ?? Brushes.White;
            dictionary["FloatingBarBorderBrush"] = Application.Current.TryFindResource("FloatBarBorderBrush") ?? Brushes.White;
            dictionary["FloatingBarAccentBrush"] = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            dictionary["FloatingBarButtonHoverBrush"] = new SolidColorBrush(Color.FromArgb(0x22, 0x25, 0x63, 0xEB));
            dictionary["FloatingBarButtonPressedBrush"] = new SolidColorBrush(Color.FromArgb(0x44, 0x25, 0x63, 0xEB));
            dictionary["FloatingBarPopupBackgroundBrush"] = Application.Current.TryFindResource("ToolsPopupBackground") ?? dictionary["FloatingBarBackgroundBrush"];
            dictionary["FloatingBarPopupInnerBackgroundBrush"] = Application.Current.TryFindResource("ToolsPopupInnerBackground") ?? dictionary["FloatingBarBackgroundBrush"];
            dictionary["FloatingBarPopupInnerBorderBrush"] = Application.Current.TryFindResource("ToolsPopupInnerBorderBrush") ?? dictionary["FloatingBarBorderBrush"];
            dictionary["FloatingBarPopupTitleForegroundBrush"] = Application.Current.TryFindResource("ToolsPopupTitleForeground") ?? dictionary["FloatingBarForegroundBrush"];
            dictionary["FloatingBarPopupCloseBrush"] = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            return dictionary;
        }

        /// <summary>
        /// 根据 IsColorfulViewboxFloatingBar 设置覆盖背景；关闭时恢复内置背景。
        /// </summary>
        public void ApplyColorfulOverlay()
        {
            var resources = Application.Current.Resources;
            var enabled = MainWindow.Settings?.Appearance?.IsColorfulViewboxFloatingBar == true;

            if (!enabled)
            {
                if (_colorfulOverlayDictionary != null)
                {
                    resources.MergedDictionaries.Remove(_colorfulOverlayDictionary);
                    _colorfulOverlayDictionary = null;
                }
                return;
            }

            if (_colorfulOverlayDictionary != null)
                resources.MergedDictionaries.Remove(_colorfulOverlayDictionary);

            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0x95, 0x80, 0xB0, 0xFF), 0));
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0x95, 0xC0, 0xFF, 0xC0), 1));

            _colorfulOverlayDictionary = new ResourceDictionary
            {
                { "FloatingBarBackgroundBrush", gradientBrush }
            };
            // 追加到末尾，优先级高于内置深浅色背景。
            resources.MergedDictionaries.Add(_colorfulOverlayDictionary);
        }
    }
}
