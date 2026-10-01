using Ink_Canvas.Windows.SettingsViews.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace Ink_Canvas.Windows.SettingsViews.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void HomePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateResponsiveLayout(e.NewSize.Width);
        }

        private void UpdateResponsiveLayout(double width)
        {
            if (MainContentGrid == null) return;
            bool compact = width < 560;
            MainContentGrid.Margin = compact ? new Thickness(16, 8, 16, 24) : new Thickness(36, 12, 36, 32);
            ActionsCol1.Width = ActionsCol2.Width = new GridLength(compact ? 0 : 1, GridUnitType.Star);
            var buttons = new[] { BtnRestart, BtnReset, BtnExit };
            for (int i = 0; i < buttons.Length; i++)
            {
                Grid.SetColumn(buttons[i], compact ? 0 : i);
                Grid.SetRow(buttons[i], compact ? i : 0);
            }
        }

        private void QuickNavCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string pageTag)
            {
                var settingsWindow = Window.GetWindow(this) as SettingsWindow;
                settingsWindow?.NavigateToPage(pageTag);
            }
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            SettingsActionHub.OnRestartApplication(sender, e);
        }

        private void BtnResetToSuggestion_Click(object sender, RoutedEventArgs e)
        {
            SettingsActionHub.OnResetToSuggestion(sender, e);
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            SettingsActionHub.OnExitApplication(sender, e);
        }
    }
}
