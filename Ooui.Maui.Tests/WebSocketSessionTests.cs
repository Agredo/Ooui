using System.Net.WebSockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Ooui.Maui.Tests;

[TestClass]
public sealed class WebSocketSessionTests
{
    // Holds every send open until the test releases the gate, so queued messages pile up behind an in-flight frame.
    sealed class GatedWebSocket : WebSocket
    {
        readonly TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int activeSends;
        int maxConcurrentSends;

        public readonly TaskCompletionSource FirstSendStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly List<string> Frames = new();

        public int MaxConcurrentSends => Volatile.Read(ref maxConcurrentSends);

        public void Release() => gate.TrySetResult();

        public List<int> SentValues()
        {
            lock (Frames)
            {
                return Frames
                    .SelectMany(frame => JArray.Parse(frame))
                    .Select(message => message["v"]![0]!.Value<int>())
                    .ToList();
            }
        }

        public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            var concurrent = Interlocked.Increment(ref activeSends);
            InterlockedMax(ref maxConcurrentSends, concurrent);
            FirstSendStarted.TrySetResult();
            try
            {
                await gate.Task.WaitAsync(cancellationToken);
                lock (Frames) Frames.Add(Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count));
            }
            finally
            {
                Interlocked.Decrement(ref activeSends);
            }
        }

        static void InterlockedMax(ref int target, int value)
        {
            int current;
            while (value > (current = Volatile.Read(ref target)) &&
                   Interlocked.CompareExchange(ref target, value, current) != current) { }
        }

        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => WebSocketState.Open;
        public override string? SubProtocol => null;
        public override void Abort() => gate.TrySetResult();
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override void Dispose() => gate.TrySetResult();
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    sealed class TestableSession : WebSocketSession
    {
        public TestableSession(WebSocket webSocket, Element element, List<Exception> errors, CancellationToken serverToken)
            : base(webSocket, element, false, 800, 600, (_, ex) => errors.Add(ex), serverToken)
        {
        }

        public void Enqueue(Message message) => QueueMessage(message);
    }

    [TestMethod]
    public async Task ConcurrentQueuedMessagesAreTransmittedSeriallyAndCompletely()
    {
        const int producers = 8;
        const int perProducer = 25;
        var socket = new GatedWebSocket();
        var errors = new List<Exception>();
        using var serverCts = new CancellationTokenSource();
        var session = new TestableSession(socket, new Div(), errors, serverCts.Token);

        session.Enqueue(Message.Call("document.body", "probe", -1));
        await socket.FirstSendStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));

        await Task.WhenAll(Enumerable.Range(0, producers).Select(producer => Task.Run(() =>
        {
            for (var i = 0; i < perProducer; i++)
                session.Enqueue(Message.Call("document.body", "probe", producer * perProducer + i));
        })));

        socket.Release();
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline && socket.SentValues().Count < producers * perProducer)
            await Task.Delay(20);

        Assert.AreEqual(1, socket.MaxConcurrentSends, "Sends must never overlap.");
        Assert.AreEqual(0, errors.Count, "No send errors were expected: " + string.Join(" | ", errors.Select(e => e.Message)));
        var values = socket.SentValues();
        Assert.AreEqual(producers * perProducer + 1, values.Count, "Every queued message must be transmitted exactly once.");
        CollectionAssert.AreEquivalent(Enumerable.Range(-1, producers * perProducer + 1).ToList(), values);
        Assert.AreEqual(-1, values[0], "The first message must not be reordered behind later ones.");
    }
}
