using Ink_Canvas;
using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal static class NetworkTimeChecks
{
    public static async Task RunAsync()
    {
        // Exercise the real socket path without external DNS/network availability.
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        int port = ((IPEndPoint)server.Client.LocalEndPoint).Port;
        var request = GetTime(port, deadline.Token);
        var received = await server.ReceiveAsync(deadline.Token);
        Check(received.Buffer.Length == 48 && received.Buffer[0] == 0x1B, "NTP request must remain compatible");
        var reply = new byte[48];
        var expected = new DateTime(2026, 1, 1, 12, 34, 56, DateTimeKind.Utc).AddMilliseconds(500);
        var epoch = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        BinaryPrimitives.WriteUInt32BigEndian(reply.AsSpan(40, 4), (uint)(expected - epoch).TotalSeconds);
        BinaryPrimitives.WriteUInt32BigEndian(reply.AsSpan(44, 4), 0x80000000);
        await server.SendAsync(reply, received.RemoteEndPoint, deadline.Token);
        Check(await request == expected.ToLocalTime(), "NTP seconds/fraction must decode in network byte order");

        var before = DateTime.Now;
        request = GetTime(port, deadline.Token);
        received = await server.ReceiveAsync(deadline.Token);
        await server.SendAsync(new byte[8], received.RemoteEndPoint, deadline.Token);
        var fallback = await request;
        Check(fallback >= before && fallback <= DateTime.Now, "Incomplete NTP replies must fall back to local time");

        using var timeout = new CancellationTokenSource();
        request = GetTime(port, timeout.Token);
        await server.ReceiveAsync(deadline.Token);
        timeout.Cancel(); // No reply: cancellation must end the actual receive, not just a waiting wrapper.
        try
        {
            await request.WaitAsync(deadline.Token);
            throw new Exception("NTP receive ignored cancellation");
        }
        catch (OperationCanceledException) when (!deadline.IsCancellationRequested)
        {
        }
        Console.WriteLine("Modern NTP socket/decoding/cancellation checks passed.");
    }

    private static Task<DateTime> GetTime(int port, CancellationToken cancellationToken)
        => (Task<DateTime>)typeof(MainWindow)
            .GetMethod("GetNetworkTimeAsync", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { "127.0.0.1", port, cancellationToken });

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}
