using Ink_Canvas;
using Ink_Canvas.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Ink_Canvas.Windows.SettingsViews.Pages;
using System.Windows.Media;
using Ink_Canvas.Windows.SettingsViews.Helpers;
using Ink_Canvas.Windows.SettingsViews;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

internal static class WindowSmoke
{
    private static void CheckMaterialHome(string directory, int theme)
    {
        foreach (int width in new[] { 900, 360 })
        {
            var home = new HomePage { Width = width, Height = 700 };
            home.Measure(new Size(width, 700));
            home.Arrange(new Rect(0, 0, width, 700));
            home.UpdateLayout();
            var exitButton = (Button)home.FindName("BtnExit");
            if (Grid.GetRow(exitButton) != (width < 560 ? 2 : 0))
                throw new Exception("首页操作区未适配窗口宽度");
            var bitmap = new RenderTargetBitmap(width, 700, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(home);
            var corner = new byte[4];
            bitmap.CopyPixels(new Int32Rect(0, 0, 1, 1), corner, 4, 0);
            if (corner[3] != 255) throw new Exception("首页必须绘制主题背景，不能透明");
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(Path.Combine(directory, $"material-home-{theme}-{width}.png"));
            encoder.Save(output);
        }
    }

    private static void CheckMaterialPopups(string directory, int theme)
    {
        var pen = new PenPalettePopupContent();
        var tabs = (Panel)pen.TabBar.FindName("TabsPanel");
        int tabChanges = 0;
        pen.TabBar.SelectedIndexChanged += (_, _) => tabChanges++;
        foreach (int index in new[] { 1, 2, 0 })
        {
            ((Button)tabs.Children[index]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            if (pen.SelectedTabIndex != index ||
                pen.HighlighterOverlapPanel.Visibility != (index == 1 ? Visibility.Visible : Visibility.Collapsed))
                throw new Exception("Material 标签按钮未同步画笔类型与重叠加深面板");
        }
        if (tabChanges != 3) throw new Exception("画笔类型切换必须每次仅触发一次事件");

        var tools = new FloatingBarToolsPopupContent();
        int closes = 0;
        foreach (var close in new[] { pen.CloseButtonControl, tools.CloseButtonControl })
        {
            close.Click += (_, _) => closes++;
            close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        if (closes != 2) throw new Exception("弹窗关闭按钮事件不可访问");

        foreach (var popup in new UserControl[] { pen, tools })
        {
            int width = popup == pen ? 420 : 240;
            popup.Width = width;
            popup.Measure(new Size(width, double.PositiveInfinity));
            popup.Arrange(new Rect(popup.DesiredSize));
            popup.UpdateLayout();
            var bitmap = new RenderTargetBitmap(width, (int)Math.Ceiling(popup.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(popup);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var output = File.Create(Path.Combine(directory, $"material-popup-{theme}-{popup.GetType().Name}.png"));
            encoder.Save(output);
        }
        var expectedForeground = (SolidColorBrush)Application.Current.FindResource("MaterialDesign.Brush.Foreground");
        if (((SolidColorBrush)pen.PenStyleComboBox.Foreground).Color != expectedForeground.Color)
            throw new Exception("画笔下拉框文字未跟随 Material 主题");
        if (tools.SaveBtn.ActualHeight < 60) throw new Exception("工具按钮内容被 Material 默认高度裁切");
        int pointerUps = 0;
        tools.SaveBtn.ButtonMouseUp += (_, _) => pointerUps++;
        var quick = new QuickPanelButton();
        quick.ButtonMouseUp += (_, _) => pointerUps++;
        foreach (var control in new UserControl[] { tools.SaveBtn, quick })
        {
            ((UIElement)control.FindName("ButtonPanel")).RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
                { RoutedEvent = System.Windows.Input.Mouse.MouseUpEvent });
        }
        if (pointerUps != 2) throw new Exception("工具与侧栏按钮必须保留原有鼠标事件");
    }

    private static async System.Threading.Tasks.Task CheckMaterialSettingsWindow(SettingsWindow settings, MainWindow main, string directory)
    {
        settings.SuppressInitialNavigation = true;
        settings.Show();
        var frame = (Frame)settings.FindName("rootFrame");
        foreach (int theme in new[] { 0, 1 })
        {
            typeof(MainWindow).GetMethod("ApplyTheme", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(main, new object[] { theme });
            settings.NavigateToPage("HomePage");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            settings.NavigateToPage("StartupPage");
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (frame.Content is not StartupPage || !frame.CanGoBack)
                throw new Exception("Material 导航必须保留目标页面与返回历史");
            frame.GoBack();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (frame.Content is not HomePage || settings.BuildSettingsUri() != "icc://settings/HomePage")
                throw new Exception("返回后页面与设置 URI 未同步");
            settings.controlsSearchBox.Text = Ink_Canvas.Properties.NavStrings.Nav_Startup;
            typeof(SettingsWindow).GetMethod("NavigateToSearchText", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(settings, new object[] { settings.controlsSearchBox.Text });
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            if (frame.Content is not StartupPage) throw new Exception("Material 搜索未导航到设置页");
            settings.controlsSearchBox.Clear();
            foreach (string pageTag in new[] { "StartupPage", "AppearancePage" })
            {
                settings.NavigateToPage(pageTag);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                foreach (int width in new[] { 1138, 360 })
                {
                    settings.Width = width;
                    settings.Height = 700;
                    await System.Threading.Tasks.Task.Delay(400); // Wait for the drawer transition before capturing a still image.
                    await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                    settings.UpdateLayout();
                    var drawer = (DrawerHost)settings.FindName("SettingsDrawer");
                    if (drawer.OpenMode != (width < 840 ? DrawerHostOpenMode.Modal : DrawerHostOpenMode.Standard) ||
                        drawer.IsLeftDrawerOpen != (width >= 840))
                        throw new Exception("Material 导航未适配窗口宽度");
                    var scrollBar = FindVisualChild<ScrollBar>((DependencyObject)frame.Content);
                    if (scrollBar == null || ScrollBarAssist.GetButtonsVisibility(scrollBar) != Visibility.Collapsed ||
                        ScrollBarAssist.GetThumbWidth(scrollBar) != 4)
                        throw new Exception("设置页面未使用与导航一致的精简滚动条");
                    var firstCard = FindVisualChild<iNKORE.UI.WPF.Modern.Controls.SettingsCard>((DependencyObject)frame.Content);
                    var contentHost = (FrameworkElement)firstCard.Template.FindName("ContentHost", firstCard);
                    if (Grid.GetRow(contentHost) != (width < 840 ? 1 : 0))
                        throw new Exception("Material 设置卡片未在窄窗口切换为纵向布局");
                    var bitmap = new RenderTargetBitmap((int)Math.Ceiling(drawer.ActualWidth), (int)Math.Ceiling(drawer.ActualHeight), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(settings);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var output = File.Create(Path.Combine(directory, $"material-settings-{theme}-{width}-{pageTag}.png"));
                    encoder.Save(output);
                }
            }
        }
    }

    private static void CheckMaterialSwitch()
    {
        var card = new LabeledSettingsCard { Header = "Material switch smoke" };
        card.Measure(new Size(600, 100));
        card.Arrange(new Rect(0, 0, 600, 100));
        card.UpdateLayout();
        if (FindVisualChild<Card>(card) == null) throw new Exception("设置卡片仍在使用 Fluent 模板");
        var group = new iNKORE.UI.WPF.Modern.Controls.SettingsExpander { Header = "Material group smoke", IsExpanded = true };
        group.Items.Add(new iNKORE.UI.WPF.Modern.Controls.SettingsCard { Header = "Child" });
        group.Measure(new Size(600, double.PositiveInfinity));
        group.Arrange(new Rect(group.DesiredSize));
        group.UpdateLayout();
        if (FindVisualChild<Expander>(group) == null || FindVisualChild<iNKORE.UI.WPF.Modern.Controls.SettingsCard>(group) == null)
            throw new Exception("Material 展开组未显示子设置项");
        var toggle = FindVisualChild<ToggleButton>(card);
        if (toggle == null) throw new Exception("设置卡片未使用 Material 开关模板");
        int events = 0;
        card.Toggled += (_, _) => events++;
        toggle.IsChecked = true;
        if (!card.IsOn || !card.ToggleSwitchControl.IsOn || events != 1)
            throw new Exception("Material 开关必须保留 IsOn 双向绑定与单次 Toggled 事件");
        card.IsOn = false;
        if (toggle.IsChecked != false || events != 2)
            throw new Exception("程序更新设置必须同步 Material 开关");
    }

    private static T FindVisualChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) return match;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }
        return null;
    }

    // 单独进程运行：加载真实 XAML/工具栏和窗口生命周期，但不注册文件关联。
    public static void Run()
    {
        var originalDirectory = Environment.CurrentDirectory;
        var root = Path.Combine(Path.GetTempPath(), "icc-window-smoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Configs"));
        Directory.CreateDirectory(Path.Combine(root, "Saves"));
        Environment.CurrentDirectory = root;
        App.RootPath = root + Path.DirectorySeparatorChar;
        try
        {
            var settings = new Settings();
            settings.Appearance.IsColorfulViewboxFloatingBar = false;
            settings.Startup.HasShownOobe = true;
            settings.Startup.CrashAction = 1;
            settings.Advanced.IsAlwaysOnTop = false;
            settings.Advanced.IsNoFocusMode = false;
            settings.Advanced.IsAutoBackupEnabled = false;
            settings.PowerPointSettings.PowerPointSupport = false;
            settings.InkToShape.IsInkToShapeEnabled = false;
            settings.Automation.AutoSavedStrokesLocation = Path.Combine(root, "Saves");
            settings.Automation.IsEnableAutoSaveStrokes = true;
            settings.Automation.AutoSaveStrokesIntervalMinutes = 2;
            MainWindow.Settings = settings;
            var path = Path.Combine(root, SettingsManager.SettingsFileName);
            var startupJson = JsonConvert.SerializeObject(settings);
            File.WriteAllText(path, startupJson);
            typeof(App).GetMethod("UpdateCachedSettingsJson", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { startupJson });

            var app = new App();
            var startup = typeof(App).GetMethod("App_Startup", BindingFlags.Instance | BindingFlags.NonPublic);
            app.Startup -= (StartupEventHandler)startup.CreateDelegate(typeof(StartupEventHandler), app);
            app.InitializeComponent();
            Exception failure = null;
            app.DispatcherUnhandledException += (_, e) =>
            {
                failure = e.Exception;
                e.Handled = true;
                app.Shutdown(1);
            };
            var window = new MainWindow();
            bool checkedWindow = false;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += async (_, _) =>
            {
                timer.Stop();
                try
                {
                    if (!window.IsLoaded) throw new Exception("窗口未完成加载");
                    // 保留旧启动缓存，修改磁盘配置：重载必须使用新文件而非缓存。
                    settings.Automation.AutoSaveStrokesIntervalMinutes = 3;
                    File.WriteAllText(path, JsonConvert.SerializeObject(settings));
                    window.ReloadSettingsFromFile();
                    var autoSave = (DispatcherTimer)typeof(MainWindow)
                        .GetField("autoSaveStrokesTimer", BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(window);
                    if (!autoSave.IsEnabled || autoSave.Interval != TimeSpan.FromMinutes(3))
                        throw new Exception("配置重载未应用新的自动保存间隔");
                    foreach (var theme in new[] { 0, 1 })
                    {
                        typeof(MainWindow).GetMethod("ApplyTheme", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(window, new object[] { theme });
                        if (!ReferenceEquals(app.TryFindResource("FloatingBarBackgroundBrush"), app.TryFindResource("FloatBarBackground")))
                            throw new Exception("内置工具栏外观未跟随深浅色主题");
                        var materialTheme = new PaletteHelper().GetTheme();
                        if (materialTheme.GetBaseTheme() != (theme == 0 ? BaseTheme.Light : BaseTheme.Dark) ||
                            materialTheme.PrimaryMid.Color != (Color)app.FindResource("MaterialPrimaryColor"))
                            throw new Exception("Material 主题未跟随应用主题");
                        CheckMaterialSwitch();
                        CheckMaterialHome(root, theme);
                        CheckMaterialPopups(root, theme);
                    }
                    // 真实抓屏入口：只写临时 PNG，验证后立即删除，不写用户桌面。
                    var screenshotPath = Path.Combine(root, "Saves", "smoke.png");
                    try
                    {
                        typeof(MainWindow).GetMethod("CaptureAndSaveScreenshot", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(window, new object[] { screenshotPath, true });
                        using var screenshot = System.Drawing.Image.FromFile(screenshotPath);
                        if (screenshot.RawFormat.Guid != System.Drawing.Imaging.ImageFormat.Png.Guid ||
                            screenshot.Size != System.Windows.Forms.SystemInformation.VirtualScreen.Size)
                            throw new Exception("桌面截图必须保留 PNG 格式与虚拟屏幕原始尺寸");
                    }
                    finally
                    {
                        File.Delete(screenshotPath);
                    }
                    var settingsWindow = new SettingsWindow();
                    var pages = (Dictionary<string, Type>)typeof(SettingsWindow)
                        .GetField("_pageTypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settingsWindow);
                    foreach (var removed in new[] { "CloudStoragePage", "PrivacyPage", "PerformancePage",
                        "AnnouncementCenterPage", "FriendlyLinksPage", "FloatingBarThemePage", "FloatingBarThemeMarketPage", "UpdatePage" })
                        if (pages.ContainsKey(removed)) throw new Exception("已删除设置页仍被注册：" + removed);
                    foreach (var retained in new[] { "NotificationPage", "StoragePage", "BackupPage", "AutomationWorkflowPage",
                        "ToolbarPage", "BoardToolbarPage", "PowerPointPage", "AboutPage", "SecurityPage" })
                        if (!pages.ContainsKey(retained)) throw new Exception("应保留的设置页丢失：" + retained);
                    // 直接构造本次涉及的页面，校验 XAML 与资源引用；不触发其他页面的预加载副作用。
                    foreach (var page in new[] { "HomePage", "NotificationPage", "StoragePage", "BackupPage", "AboutPage", "ToolbarAppearancePage" })
                        Activator.CreateInstance(pages[page]);
                    await CheckMaterialSettingsWindow(settingsWindow, window, root);
                    settingsWindow.Close();
                    await WindowPersistenceChecks.CheckSaveApisAsync(window);
                    checkedWindow = true;
                    WindowPersistenceChecks.CheckCloseWait(window, ex => failure = ex);
                }
                catch (Exception ex)
                {
                    failure = ex;
                    app.Shutdown(1);
                }
            };
            window.Loaded += (_, _) => timer.Start();
            app.Run(window);
            if (failure != null) throw new Exception("窗口冒烟测试失败", failure);
            if (!checkedWindow || !App.IsAppExitByUser) throw new Exception("窗口未正常结束应用生命周期");
            Console.WriteLine("Window load/settings reload/normal shutdown smoke check passed.");
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
            // 失败时保留日志，便于定位 XAML 或生命周期问题。
            Console.WriteLine("Window smoke logs: " + root);
        }
    }
}
