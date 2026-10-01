using iNKORE.UI.WPF.Controls;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ink_Canvas.Controls
{
    public class PopupTabItem
    {
        public string Header { get; set; }
        public string IconSource { get; set; }
    }

    public partial class PopupTabTitleBar : UserControl
    {
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            nameof(SelectedIndex), typeof(int), typeof(PopupTabTitleBar),
            new PropertyMetadata(0, OnSelectedIndexChanged));

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PopupTabTitleBar)d;
            var newIndex = (int)e.NewValue;
            if (newIndex < 0 || newIndex >= control.Tabs.Count)
                return;
            control.UpdateTabVisuals();
            control.SelectedIndexChanged?.Invoke(control, newIndex);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public ObservableCollection<PopupTabItem> Tabs { get; }

        public Button CloseButtonControl => CloseButton;

        public event EventHandler<int> SelectedIndexChanged;

        public PopupTabTitleBar()
        {
            InitializeComponent();
            Tabs = new ObservableCollection<PopupTabItem>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;
        }

        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildTabs();
        }

        private void RebuildTabs()
        {
            TabsPanel.Children.Clear();
            for (int i = 0; i < Tabs.Count; i++)
            {
                var tabItem = Tabs[i];
                var tabBorder = CreateTabElement(tabItem, i);
                TabsPanel.Children.Add(tabBorder);
            }
            UpdateTabVisuals();
        }

        private Button CreateTabElement(PopupTabItem tabItem, int index)
        {
            var button = new Button
            {
                Height = 36,
                MinWidth = 0,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand
            };
            button.SetResourceReference(StyleProperty, "MaterialDesignFlatButton");
            System.Windows.Automation.AutomationProperties.SetName(button, tabItem.Header ?? string.Empty);
            button.Click += (s, e) =>
            {
                if (SelectedIndex != index)
                    SelectedIndex = index;
                e.Handled = true;
            };

            var border = new Border
            {
                Height = 36,
                CornerRadius = new CornerRadius(10),
                Background = Brushes.Transparent,
                Tag = index,
                Padding = new Thickness(8, 0, 8, 0)
            };
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var contentPanel = new SimpleStackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(tabItem.IconSource))
            {
                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(tabItem.IconSource, UriKind.RelativeOrAbsolute)),
                    Height = 16,
                    Width = 16
                };
                RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);
                contentPanel.Children.Add(icon);
            }

            var text = new TextBlock
            {
                FontWeight = FontWeights.Medium,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                Text = tabItem.Header ?? string.Empty,
                Margin = new Thickness(4, 0, 4, 0)
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "FloatingBarForegroundBrush");
            contentPanel.Children.Add(text);

            Grid.SetRow(contentPanel, 0);
            grid.Children.Add(contentPanel);

            var indicator = new Border
            {
                Height = 3,
                CornerRadius = new CornerRadius(1.5),
                Margin = new Thickness(12, 0, 12, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Visibility = Visibility.Collapsed
            };
            indicator.SetResourceReference(Border.BackgroundProperty, "FloatingBarAccentBrush");

            Grid.SetRow(indicator, 1);
            grid.Children.Add(indicator);
            border.Child = grid;
            button.Content = border;

            return button;
        }

        private void UpdateTabVisuals()
        {
            if (SelectedIndex < 0 || SelectedIndex >= TabsPanel.Children.Count)
                return;
            for (int i = 0; i < TabsPanel.Children.Count; i++)
            {
                if (!(TabsPanel.Children[i] is Button button)) continue;
                if (!(button.Content is Border border) || !(border.Child is Grid grid)) continue;

                bool isSelected = (i == SelectedIndex);

                if (isSelected)
                    border.SetResourceReference(Border.BackgroundProperty, "FloatingBarPopupHoverBrush");
                else
                    border.Background = Brushes.Transparent;

                if (grid.Children.Count >= 2)
                {
                    if (grid.Children[1] is Border indicator)
                    {
                        indicator.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
                    }

                    if (grid.Children[0] is SimpleStackPanel contentPanel)
                    {
                        foreach (var child in contentPanel.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.FontWeight = isSelected ? FontWeights.Bold : FontWeights.Medium;
                            }
                        }
                    }
                }
            }
        }
    }
}
