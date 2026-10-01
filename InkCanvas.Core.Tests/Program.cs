using Ink_Canvas;
using Ink_Canvas.Controls.Toolbar;
using Ink_Canvas.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--window-smoke"))
        {
            // WPF 的相对 pack URI 必须从主程序资源程序集解析。
            Assembly.SetEntryAssembly(typeof(App).Assembly);
            WindowSmoke.Run();
            return;
        }
        RunCoreChecks();
    }

    private static void RunCoreChecks()
    {
        // 只测试真实保存/定时器方法，不运行窗口构造中的热键、相机和启动向导。
        var window = (MainWindow)RuntimeHelpers.GetUninitializedObject(typeof(MainWindow));
        var canvas = new InkCanvas();
        SetField(window, "inkCanvas", canvas);
        SetField(window, "_currentWhiteboardIndex", 2);
        var histories = new TimeMachineHistory[101][];
        histories[1] = History(10);
        SetField(window, "TimeMachineHistories", histories);
        var undo = new TimeMachine();
        undo.CommitStrokeUserInputHistory(Strokes(20));
        SetField(window, "timeMachine", undo);
        canvas.Strokes = Strokes(20);

        var fresh = Snapshot(window, "GetWhiteboardStrokesForSave", 2);
        Check(fresh.Count == 1 && fresh[0].StylusPoints[0].X == 20, "新页必须保存实时墨迹");
        Check(!ReferenceEquals(fresh, canvas.Strokes), "保存快照不能引用实时集合");
        fresh.Clear();
        Check(canvas.Strokes.Count == 1 && undo.CanUndo, "保存不能改变实时墨迹或撤销历史");

        histories[2] = History(99);
        Check(Snapshot(window, "GetWhiteboardStrokesForSave", 2)[0].StylusPoints[0].X == 20,
            "当前页必须覆盖过期历史快照");
        Check(Snapshot(window, "GetWhiteboardStrokesForSave", 1)[0].StylusPoints[0].X == 10,
            "非当前页必须恢复保存的历史");
        canvas.Strokes.Clear();
        Check(Snapshot(window, "GetWhiteboardStrokesForSave", 2).Count == 0,
            "擦空当前页不能复活旧墨迹");
        Check(Snapshot(window, "GetPptStrokesForSave", 3, 3).Count == 0,
            "PPT 当前页也必须保存实时空状态");
        canvas.Strokes = Strokes(30);
        Check(Snapshot(window, "GetPptStrokesForSave", 3, 3)[0].StylusPoints[0].X == 30,
            "PPT 当前页必须保存最新墨迹");

        var timer = new DispatcherTimer();
        SetField(window, "autoSaveStrokesTimer", timer);
        MainWindow.Settings = new Settings();
        MainWindow.Settings.Automation.IsEnableAutoSaveStrokes = true;
        MainWindow.Settings.Automation.AutoSaveStrokesIntervalMinutes = 1;
        window.UpdateAutoSaveStrokesTimer();
        Check(timer.IsEnabled && timer.Interval == TimeSpan.FromMinutes(1), "应用加载的自动保存间隔");
        MainWindow.Settings.Automation.IsEnableAutoSaveStrokes = false;
        window.UpdateAutoSaveStrokesTimer();
        Check(!timer.IsEnabled, "关闭自动保存必须停止定时器");
        CheckSaveFileNames();
        CheckLegacyToolsLayouts();
        CheckRemovedFeatureSettings();
        CheckRemovedLiquidGlass();
        Console.WriteLine("Core save/autosave/layout/settings regression checks passed.");
    }

    private static void CheckRemovedLiquidGlass()
    {
        const string json = """
            {"theme":1,"enableLiquidGlassBar":true,"liquidGlassBarOpacity":0.8,"liquidGlassBarPositionX":100,"liquidGlassBarPositionY":200}
            """;
        var appearance = JsonConvert.DeserializeObject<Appearance>(json);
        Check(appearance.Theme == 1, "移除液态玻璃不能影响普通主题");
        var original = JObject.Parse(json);
        var saved = JObject.FromObject(appearance);
        var defaults = JObject.FromObject(new Appearance());
        foreach (var property in new[] { "EnableLiquidGlassBar", "LiquidGlassBarOpacity", "LiquidGlassBarPositionX", "LiquidGlassBarPositionY" })
        {
            var key = char.ToLowerInvariant(property[0]) + property.Substring(1);
            Check(typeof(Appearance).GetProperty(property) == null && defaults[key] == null,
                "液态玻璃设置不能参与运行或生成默认值：" + property);
            Check(JToken.DeepEquals(original[key], saved[key]), "旧配置仅作为扩展数据保留：" + key);
        }
        foreach (var name in new[] { "Ink_Canvas.LiquidGlassBarWindow", "Ink_Canvas.Helpers.LiquidGlassCapture",
            "Ink_Canvas.Helpers.LiquidGlassMagnifier", "Ink_Canvas.Shaders.LiquidGlassEffect" })
            Check(typeof(App).Assembly.GetType(name) == null, "液态玻璃实现必须移除：" + name);
        Check(!typeof(MainWindow).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Any(method => method.Name.Contains("LiquidGlass")), "液态玻璃宿主入口必须移除");
    }

    private static void CheckSaveFileNames()
    {
        var context = new SaveFileNameContext
        {
            Time = new DateTime(2026, 1, 2, 3, 4, 5, 6),
            Mode = "BlackBoard", Type = "Auto", Page = 2, Count = 3
        };
        foreach (var template in new[] { null, "", "   ", "...", " -_ " })
            Check(SaveFileNameHelper.Render(template, context) == "2026-01-02 03-04-05-006",
                "无效模板必须回退到时间戳");
        Check(SaveFileNameHelper.Render("{date}_{time}_{mode}_{type}_{page}_{count}", context) ==
            "2026-01-02_03-04-05_BlackBoard_Auto_2_3", "文件名占位符必须保持兼容");
        Check(SaveFileNameHelper.Render(" a<b>c: . ", context) == "a_b_c_", "非法字符和末尾点号必须清理");
        foreach (var name in new[] { "CON", "nul.txt", "CON.foo.bar", "LPT1.backup.png", "COM¹.txt", "LPT²", "COM³", "CON .txt" })
            Check(SaveFileNameHelper.Render(name, context) == "_" + name, "必须规避 Windows 保留名：" + name);
        foreach (var name in new[] { "COM10", "console.txt", "report.v1.png" })
            Check(SaveFileNameHelper.Render(name, context) == name, "普通文件名不能被误改：" + name);

        var sanitize = typeof(MainWindow).GetMethod("SanitizeScreenshotRelativePath", BindingFlags.Static | BindingFlags.NonPublic);
        Check((string)sanitize.Invoke(null, new object[] { @"课件/CON.foo.bar/第1页" }) ==
            Path.Combine("课件", "_CON.foo.bar", "第1页"), "截图子目录也必须规避保留名");
        Check((string)sanitize.Invoke(null, new object[] { @"../课件/../../第1页" }) ==
            Path.Combine("课件", "第1页"), "截图路径不能穿越目标目录");
    }

    private static void CheckRemovedFeatureSettings()
    {
        const string legacyJson = """
            {
              "dlass": { "userToken": "old-token", "webDavPassword": "old-password" },
              "upload": { "enabledProviders": ["Dlass", "WebDav"] },
              "performance": { "isMonitoringEnabled": true, "deviceScore": 42 },
              "appearance": { "floatingBarThemeId": "custom-skin", "theme": 1 },
              "startup": { "telemetryUploadLevel": 2, "hasAcceptedTelemetryPrivacy": true, "isAutoUpdate": true, "isAutoUpdateWithSilence": true, "isAutoUpdateWithSilenceStartTime": "06:00", "isAutoUpdateWithSilenceEndTime": "22:00", "updateChannel": 2, "updatePackageArchitecture": 1, "isSmartUpdate": true, "skippedVersion": "1.0.0", "autoUpdatePauseUntilDate": "2099-01-01" },
              "notification": { "isAnnouncementEnabled": true, "isForcePopupEnabled": true, "isDynamicNotificationEnabled": true, "updateDurationSeconds": 5 },
              "advanced": { "isAutoBackupBeforeUpdate": true, "isAutoBackupEnabled": true, "autoBackupIntervalDays": 3 },
              "automation": { "isEnableAutoSaveStrokes": true, "autoSaveStrokesIntervalMinutes": 3 }
            }
            """;
        var settings = JsonConvert.DeserializeObject<Settings>(legacyJson);
        Check(settings.Appearance.Theme == 1, "旧配置的内置主题必须保留");
        Check(settings.Automation.IsEnableAutoSaveStrokes && settings.Automation.AutoSaveStrokesIntervalMinutes == 3,
            "旧配置的本地自动保存必须保留");
        Check(settings.Notification.IsDynamicNotificationEnabled, "本地通知设置必须保留");
        Check(settings.Advanced.IsAutoBackupEnabled && settings.Advanced.AutoBackupIntervalDays == 3, "定期本地备份必须保留");
        foreach (var pair in new[] { (typeof(Settings), "Dlass"), (typeof(Settings), "Upload"),
            (typeof(Settings), "Performance"), (typeof(Startup), "TelemetryUploadLevel"),
            (typeof(Advanced), "IsAutoBackupBeforeUpdate"),
            (typeof(Appearance), "FloatingBarThemeId"), (typeof(NotificationSettings), "IsAnnouncementEnabled") })
            Check(pair.Item1.GetProperty(pair.Item2) == null, "已删除功能不能保留运行时设置入口：" + pair.Item2);

        var original = JObject.Parse(legacyJson);
        var roundTrip = JObject.FromObject(settings);
        foreach (var path in new[] { "dlass", "upload", "performance", "appearance.floatingBarThemeId",
            "startup.telemetryUploadLevel", "startup.hasAcceptedTelemetryPrivacy", "notification.isAnnouncementEnabled",
            "notification.isForcePopupEnabled", "advanced.isAutoBackupBeforeUpdate" })
            Check(JToken.DeepEquals(original.SelectToken(path), roundTrip.SelectToken(path)), "保存设置不能清理旧用户数据：" + path);
        var feedback = FeedbackSanitizer.BuildSanitizedSettingsJson(settings);
        Check(!feedback.Contains("old-token") && !feedback.Contains("old-password") && !feedback.Contains("deviceId"),
            "主动反馈不得泄漏旧凭据或设备标识");
        var defaults = JObject.FromObject(new Settings());
        Check(defaults["dlass"] == null && defaults["upload"] == null && defaults["performance"] == null &&
            defaults["advanced"]["isAutoBackupBeforeUpdate"] == null,
            "新配置不应生成已删除功能的配置");

        foreach (var pair in new[] { ("IsAutoUpdate", "isAutoUpdate"), ("IsAutoUpdateWithSilence", "isAutoUpdateWithSilence"),
            ("AutoUpdateWithSilenceStartTime", "isAutoUpdateWithSilenceStartTime"),
            ("AutoUpdateWithSilenceEndTime", "isAutoUpdateWithSilenceEndTime"), ("UpdateChannel", "updateChannel"),
            ("UpdatePackageArchitecture", "updatePackageArchitecture"), ("IsSmartUpdate", "isSmartUpdate"),
            ("SkippedVersion", "skippedVersion"), ("AutoUpdatePauseUntilDate", "autoUpdatePauseUntilDate") })
        {
            Check(typeof(Startup).GetProperty(pair.Item1) == null, "更新设置不能参与运行：" + pair.Item1);
            Check(defaults["startup"][pair.Item2] == null, "新配置不能生成更新设置：" + pair.Item2);
            Check(JToken.DeepEquals(original["startup"][pair.Item2], roundTrip["startup"][pair.Item2]),
                "旧更新配置只能作为原始数据保留：" + pair.Item2);
        }
        Check(typeof(NotificationSettings).GetProperty("UpdateDurationSeconds") == null &&
            defaults["notification"]["updateDurationSeconds"] == null &&
            JToken.DeepEquals(original["notification"]["updateDurationSeconds"], roundTrip["notification"]["updateDurationSeconds"]),
            "更新通知设置不参与运行，且不删除旧数据");
        Check(typeof(App).Assembly.GetType("Ink_Canvas.Helpers.AutoUpdateHelper") == null &&
            typeof(MainWindow).GetMethod("AutoUpdate") == null &&
            typeof(MainWindow).GetField("timerCheckAutoUpdateWithSilence", BindingFlags.Instance | BindingFlags.NonPublic) == null,
            "不能保留更新器或静默安装入口");
        Check((int)Ink_Canvas.Models.NotificationMessageType.Urgent == 1 &&
            (int)Ink_Canvas.Models.NotificationMessageType.Other == 4, "旧自动化工作流的通知类型序号不能改变");
    }

    private static void CheckLegacyToolsLayouts()
    {
        var originalRoot = App.RootPath;
        var root = Path.Combine(Path.GetTempPath(), "icc-core-tests-" + Guid.NewGuid().ToString("N"));
        try
        {
            App.RootPath = root;
            Directory.CreateDirectory(ToolsMenuRegistry.GetConfigDirectory());
            File.WriteAllText(ToolsMenuRegistry.GetFloatingBarConfigPath(),
                "{\"floatingBarItems\":[\"timer\",\"save\",\"randomDraw\",\"save\",\"open\"]}");
            Check(ToolsMenuRegistry.LoadFloatingBarConfig().FloatingBarItems.SequenceEqual(new[] { "save", "open" }),
                "旧布局必须移除已删除功能、去重并保留有效项顺序");
            File.WriteAllText(ToolsMenuRegistry.GetBoardConfigPath(),
                "{\"boardItems\":[\"timer\",\"singleDraw\"]}");
            Check(ToolsMenuRegistry.LoadBoardConfig().BoardItems.SequenceEqual(
                ToolsMenuRegistry.CreateDefaultBoardLayout().BoardItems), "仅含已删除功能时必须回退到可用菜单");
        }
        finally
        {
            App.RootPath = originalRoot;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static StrokeCollection Strokes(double x) => new StrokeCollection
    {
        new Stroke(new StylusPointCollection { new StylusPoint(x, 10), new StylusPoint(x + 1, 11) })
    };

    private static TimeMachineHistory[] History(double x) => new[]
    {
        new TimeMachineHistory(Strokes(x), TimeMachineHistoryType.UserInput, false)
    };

    private static StrokeCollection Snapshot(MainWindow window, string method, params object[] args)
        => (StrokeCollection)typeof(MainWindow).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(window, args);

    private static void SetField(MainWindow window, string name, object value)
        => typeof(MainWindow).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .SetValue(window, value);

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
