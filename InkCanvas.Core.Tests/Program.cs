using Ink_Canvas;
using Ink_Canvas.Controls.Toolbar;
using Ink_Canvas.Helpers;
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
        CheckLegacyToolsLayouts();
        Console.WriteLine("Core save/autosave/layout regression checks passed.");
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
