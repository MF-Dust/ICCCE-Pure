using Ink_Canvas;
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
    // 单独进程运行：加载真实 XAML/工具栏和窗口生命周期，但不注册文件关联或启动更新器。
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
            settings.Startup.IsAutoUpdate = false;
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
            timer.Tick += (_, _) =>
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
                    }
                    var settingsWindow = new SettingsWindow();
                    var pages = (Dictionary<string, Type>)typeof(SettingsWindow)
                        .GetField("_pageTypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settingsWindow);
                    foreach (var removed in new[] { "CloudStoragePage", "PrivacyPage", "PerformancePage",
                        "AnnouncementCenterPage", "FriendlyLinksPage", "FloatingBarThemePage", "FloatingBarThemeMarketPage" })
                        if (pages.ContainsKey(removed)) throw new Exception("已删除设置页仍被注册：" + removed);
                    foreach (var retained in new[] { "NotificationPage", "StoragePage", "BackupPage", "AutomationWorkflowPage",
                        "ToolbarPage", "BoardToolbarPage", "PowerPointPage", "UpdatePage", "SecurityPage" })
                        if (!pages.ContainsKey(retained)) throw new Exception("应保留的设置页丢失：" + retained);
                    settingsWindow.Close();
                    checkedWindow = true;
                    // 白板模式的第一次关闭仅退回批注模式；第二次走真实关闭流程。
                    window.Close();
                    if (window.IsLoaded) window.Close();
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
