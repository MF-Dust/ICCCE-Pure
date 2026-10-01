using Ink_Canvas;
using Ink_Canvas.Helpers;
using System;
using System.Reflection;
using System.Text;

internal static class ModernRuntimeChecks
{
    public static void Run()
    {
        // Fixed PBKDF2 vectors: UTF-8 password, salt 00..0f, 120,000 iterations, 32 bytes.
        // Keep both the current SHA256 format and automatic migration of legacy SHA1 hashes.
        const string password = "密码-Pass🔒";
        const string currentHash = "6C8viKgOcgk5/WgVcFRcYyDpKAzeygS4rBP93dQHU4Q=";
        const string legacyHash = "nyUr2bAScg7LGyQkwJ7vXa958RcVlgSTwuDN3s07J70=";
        var settings = new Settings();
        settings.Security.PasswordSalt = "AAECAwQFBgcICQoLDA0ODw==";
        foreach (var storedHash in new[] { currentHash, legacyHash })
        {
            settings.Security.PasswordHash = storedHash;
            Check(!(bool)Security("VerifyPassword", settings, "wrong"), "Wrong password must be rejected");
            Check(settings.Security.PasswordHash == storedHash, "Rejected passwords must not migrate a hash");
            Check((bool)Security("VerifyPassword", settings, password), "Stored password must remain valid");
            Check(settings.Security.PasswordHash == currentHash, "Legacy SHA1 must migrate to the same SHA256 hash");
            Check(!(bool)Security("VerifyPassword", settings, null), "Null password must be rejected");
        }

        settings.Security.PasswordHash = "not-base64";
        Check(!(bool)Security("VerifyPassword", settings, password), "Malformed hash must be rejected");
        Security("SetPassword", settings, password);
        Check(Convert.FromBase64String(settings.Security.PasswordSalt).Length == 16 &&
            Convert.FromBase64String(settings.Security.PasswordHash).Length == 32 &&
            (bool)Security("VerifyPassword", settings, password), "New password format must remain compatible");
        Check(((string)Security("GenerateTotpSecret")).Length == 32, "TOTP secret must remain 20-byte Base32");

        // RFC 6238 SHA256 vectors, reduced to the existing six-digit format.
        byte[] secret = Encoding.ASCII.GetBytes("12345678901234567890123456789012");
        foreach (var (seconds, expected) in new[] { (59L, "119246"), (1111111109L, "084774"), (20000000000L, "737706") })
            Check((string)Security("GenerateTotpCode", secret, seconds / 30) == expected,
                "TOTP big-endian counter and SHA256 algorithm must remain compatible");

        var hashMethod = typeof(App).Assembly.GetType("Ink_Canvas.Helpers.HashHelper", true)
            .GetMethod("GetFileHash", BindingFlags.Static | BindingFlags.Public);
        foreach (var (input, expected) in new[] { ("abc", "90015098"), ("课件/示例.pptx", "ED95A649"), ("", "unknown"), (null, "unknown") })
            Check((string)hashMethod.Invoke(null, new object[] { input }) == expected,
                "Persisted PPT path hashes must keep their uppercase MD5 prefix");

        Check(ShapeRecognitionRouter.ResolveUseWinRt(ShapeRecognitionEngineMode.Auto) ==
            OperatingSystem.IsWindowsVersionAtLeast(10), "Auto recognition must follow the built-in Windows guard");
        Check(!ShapeRecognitionRouter.ResolveUseWinRt(ShapeRecognitionEngineMode.IACore) &&
            ShapeRecognitionRouter.ResolveUseWinRt(ShapeRecognitionEngineMode.WinRT) &&
            !ShapeRecognitionRouter.ShouldRunShapeRecognition(false, ShapeRecognitionEngineMode.WinRT),
            "Explicit and disabled recognition modes must remain unchanged");

        using (var process = System.Diagnostics.Process.GetCurrentProcess())
        {
            Check(Environment.ProcessId == process.Id && string.Equals(Environment.ProcessPath,
                process.MainModule.FileName, StringComparison.OrdinalIgnoreCase),
                "Built-in process metadata must identify the same executable host");
        }
        const BindingFlags metadataFlags = BindingFlags.Static | BindingFlags.NonPublic;
        Check((int)typeof(App).GetField("currentProcessId", metadataFlags).GetValue(null) == Environment.ProcessId &&
            (string)typeof(App).GetField("watchdogExitSignalFile", metadataFlags).GetValue(null) ==
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "icc_watchdog_exit_" + Environment.ProcessId + ".flag"),
            "Crash handling and watchdog signal paths must retain the current PID");

        NetworkTimeChecks.RunAsync().GetAwaiter().GetResult();
        var report = new StringBuilder();
        typeof(App).Assembly.GetType("Ink_Canvas.Helpers.MemoryBreakdownHelper", true)
            .GetMethod("AppendGcSection", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { report, 0d });
        Check(report.ToString().Contains("Gen 0") && report.ToString().Contains("Total Allocated(FC)"),
            "GC diagnostics must handle GenerationInfo as a span");
        Console.WriteLine("Modern runtime/password/TOTP compatibility checks passed.");
    }

    private static object Security(string method, params object[] args)
        => typeof(App).Assembly.GetType("Ink_Canvas.Helpers.SecurityManager", true)
            .GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Invoke(null, args);

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
