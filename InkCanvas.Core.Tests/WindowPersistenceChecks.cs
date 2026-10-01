using Ink_Canvas;
using Ink_Canvas.Helpers.Persistence;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Ink;

internal static class WindowPersistenceChecks
{
    private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static async Task CheckSaveApisAsync(MainWindow window)
    {
        var canvas = (InkCanvas)typeof(MainWindow).GetField("inkCanvas", Instance).GetValue(window);
        var options = MainWindow.Settings.Automation;
        options.IsUseCustomSaveFileName = true;
        options.IsSaveStrokesAsXML = false;
        options.IsSaveStrokesAsUInK = false;
        options.IsSaveFullPageStrokes = false;
        options.CustomSaveFileNameTemplate = "api-manual";
        // Whiteboard's first Close only returns to annotation mode, as in the original smoke check.
        if ((int)typeof(MainWindow).GetProperty("currentMode", Instance).GetValue(window) != 0)
        {
            window.Close();
            if (!window.IsLoaded) throw new Exception("closing whiteboard must not shut down the main window");
        }
        canvas.Strokes = PersistenceChecks.Strokes(20);
        var queue = new SaveCoordinator();
        PersistenceChecks.Set(window, "_saveCoordinator", queue);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var blocker = queue.Submit(() => () =>
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(10))) throw new Exception("save API gate timeout");
        }, false);
        try
        {
            if (!entered.Wait(TimeSpan.FromSeconds(10))) throw new Exception("save API writer did not start");
            var asyncSave = (Task)typeof(MainWindow).GetMethod("SaveInkCanvasStrokesAsync", Instance)
                .Invoke(window, new object[] { false, true, false });
            if (asyncSave.IsCompleted) throw new Exception("manual API must await a busy writer");
            options.CustomSaveFileNameTemplate = "api-skipped-auto";
            await (Task)typeof(MainWindow).GetMethod("SaveInkCanvasStrokesAsync", Instance)
                .Invoke(window, new object[] { false, false, true });
            if (Directory.GetFiles(options.AutoSavedStrokesLocation, "api-skipped-auto*", SearchOption.AllDirectories).Length != 0)
                throw new Exception("busy auto API wrote a queued save");
            canvas.Strokes = PersistenceChecks.Strokes(90);
            options.CustomSaveFileNameTemplate = "api-sync";
            // Release from a worker while the sync API blocks the UI, proving no Dispatcher dependency in writers.
            var releaseTask = Task.Run(() => { Thread.Sleep(100); release.Set(); });
            window.SaveInkCanvasStrokes(false, true);
            var directory = Path.Combine(options.AutoSavedStrokesLocation, "User Saved - Annotation Strokes");
            using (var stream = File.OpenRead(Path.Combine(directory, "api-sync.icstk")))
                if (!PersistenceChecks.IsfCoordinateEquals(new StrokeCollection(stream)[0].StylusPoints[0].X, 90))
                    throw new Exception("sync save API returned before its captured output was committed");
            await asyncSave;
            await blocker;
            await releaseTask;
            using (var stream = File.OpenRead(Path.Combine(directory, "api-manual.icstk")))
                if (!PersistenceChecks.IsfCoordinateEquals(new StrokeCollection(stream)[0].StylusPoints[0].X, 20))
                    throw new Exception("queued async save read live strokes instead of its accepted snapshot");
        }
        finally { release.Set(); }
    }

    // The main window must remain alive while an accepted writer is held, then close after actual IO.
    public static void CheckCloseWait(MainWindow window, Action<Exception> fail)
    {
        MainWindow.Settings.Advanced.IsSecondConfirmWhenShutdownApp = false;
        MainWindow.Settings.Automation.CustomSaveFileNameTemplate = "close-wait";
        var queue = (SaveCoordinator)typeof(MainWindow).GetField("_saveCoordinator", Instance).GetValue(window);
        var snapshot = PersistenceChecks.Capture(window);
        var entered = new ManualResetEventSlim();
        var release = new ManualResetEventSlim();
        var write = queue.Submit(() => () =>
        {
            using (snapshot)
            {
                entered.Set();
                if (!release.Wait(TimeSpan.FromSeconds(10))) throw new Exception("close wait gate timeout");
                snapshot.Write();
            }
        }, false);
        if (!entered.Wait(TimeSpan.FromSeconds(10)))
        {
            release.Set();
            throw new Exception("close-wait writer did not start");
        }
        bool closed = false;
        window.Closed += (_, _) =>
        {
            closed = true;
            try
            {
                if (!write.IsCompletedSuccessfully || !File.Exists(snapshot.Path))
                    throw new Exception("window cleanup ran before accepted save IO completed");
                using var stream = File.OpenRead(snapshot.Path);
                if (new StrokeCollection(stream).Count != 1) throw new Exception("close-wait save failed to roundtrip");
            }
            catch (Exception ex) { fail(ex); }
            finally { entered.Dispose(); release.Dispose(); }
        };
        try
        {
            window.Close();
            if (closed || !window.IsLoaded) throw new Exception("close must be deferred while an accepted writer is active");
            window.Close();
            if (closed) throw new Exception("repeated closing bypassed the accepted-save wait");
        }
        finally { release.Set(); }
    }
}
