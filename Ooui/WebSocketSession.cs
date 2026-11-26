using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;

namespace Ooui
{
    public class WebSocketSession : Session
    {
        readonly WebSocket webSocket;
        readonly Action<Message> handleElementMessageSent;

        readonly CancellationTokenSource sessionCts = new CancellationTokenSource ();
        readonly CancellationTokenSource linkedCts;
        readonly CancellationToken token;

        readonly System.Timers.Timer sendThrottle;
        readonly SemaphoreSlim sendLock = new SemaphoreSlim (1, 1);
        int transmitting;
        DateTime lastTransmitTime = DateTime.MinValue;
        readonly TimeSpan throttleInterval = TimeSpan.FromSeconds (1.0 / UI.MaxFps);

        public WebSocketSession (WebSocket webSocket, Element element, bool disposeElementAfterSession, double initialWidth, double initialHeight, Action<string, Exception> errorLogger, CancellationToken serverToken)
            : base (element, disposeElementAfterSession, initialWidth, initialHeight, errorLogger)
        {
            this.webSocket = webSocket;

            //
            // Create a new session cancellation token that will trigger
            // automatically if the server shutsdown or the session shutsdown.
            //
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource (serverToken, sessionCts.Token);
            token = linkedCts.Token;

            //
            // Preparse handlers for the element
            //
            handleElementMessageSent = QueueMessage;

            //
            // Create a timer to use as a throttle when sending messages
            //
            sendThrottle = new System.Timers.Timer (throttleInterval.TotalMilliseconds);
            sendThrottle.Elapsed += (s, e) => {
                // System.Console.WriteLine ("TICK SEND THROTTLE FOR {0}", element);
                if ((e.SignalTime - lastTransmitTime) >= throttleInterval) {
                    sendThrottle.Enabled = false;
                    lastTransmitTime = e.SignalTime;
                    TransmitQueuedMessages ();
                }
            };
        }

        public async Task RunAsync ()
        {
            //
            // Start watching for changes in the element
            //
            element.MessageSent += handleElementMessageSent;

            try {
                //
                // Add it to the document body
                //
                if (element.WantsFullScreen) {
                    element.Style.Width = initialWidth;
                    element.Style.Height = initialHeight;
                }
                QueueMessage (Message.Call ("document.body", "appendChild", element));

                //
                // Start the Read Loop
                //
                var receiveBuffer = new byte[64 * 1024];

                while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested) {
                    var receiveResult = await webSocket.ReceiveAsync (new ArraySegment<byte> (receiveBuffer), token).ConfigureAwait (false);

                    if (receiveResult.MessageType == WebSocketMessageType.Close) {
                        await webSocket.CloseAsync (WebSocketCloseStatus.NormalClosure, "", token).ConfigureAwait (false);
                        sessionCts.Cancel ();
                    }
                    else if (receiveResult.MessageType == WebSocketMessageType.Binary) {
                        await webSocket.CloseAsync (WebSocketCloseStatus.InvalidMessageType, "Cannot accept binary frame", token).ConfigureAwait (false);
                        sessionCts.Cancel ();
                    }
                    else {
                        var size = receiveResult.Count;
                        while (!receiveResult.EndOfMessage) {
                            if (size >= receiveBuffer.Length) {
                                await webSocket.CloseAsync (WebSocketCloseStatus.MessageTooBig, "Message too big", token).ConfigureAwait (false);
                                return;
                            }
                            receiveResult = await webSocket.ReceiveAsync (new ArraySegment<byte> (receiveBuffer, size, receiveBuffer.Length - size), token).ConfigureAwait (false);
                            size += receiveResult.Count;
                        }
                        var receivedString = Encoding.UTF8.GetString (receiveBuffer, 0, size);

                        try {
                            // Console.WriteLine ("RECEIVED: {0}", receivedString);
                            var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message> (receivedString);
                            element.Receive (message);
                        }
                        catch (Exception ex) {
                            Error ("Failed to process received message", ex);
                        }
                    }
                }
            }
            finally {
                element.MessageSent -= handleElementMessageSent;

                if (disposeElementAfterSession && (element is IDisposable disposable)) {
                    try {
                        disposable.Dispose ();
                    }
                    catch (Exception ex) {
                        Error ("Failed to dispose of element", ex);
                    }
                }
            }
        }

        protected override void QueueMessage (Message message)
        {
            base.QueueMessage (message);
            sendThrottle.Enabled = true;
        }

        async void TransmitQueuedMessages ()
        {
            if (Interlocked.Exchange (ref transmitting, 1) != 0)
                return;
            try {
                await sendLock.WaitAsync (token).ConfigureAwait (false);
                try {
                    while (!token.IsCancellationRequested) {
                        List<Message> messagesToSend;
                        lock (queuedMessages) {
                            messagesToSend = new List<Message> (queuedMessages);
                            queuedMessages.Clear ();
                        }
                        if (messagesToSend.Count == 0)
                            return;

                        var json = Newtonsoft.Json.JsonConvert.SerializeObject (messagesToSend);
                        var outputBuffer = new ArraySegment<byte> (Encoding.UTF8.GetBytes (json));
                        await webSocket.SendAsync (outputBuffer, WebSocketMessageType.Text, true, token).ConfigureAwait (false);
                    }
                }
                finally {
                    sendLock.Release ();
                }
            }
            catch (Exception ex) {
                Error ("Failed to send queued messages, aborting session", ex);
                element.MessageSent -= handleElementMessageSent;
                sessionCts.Cancel ();
            }
            finally {
                Volatile.Write (ref transmitting, 0);
                lock (queuedMessages) {
                    if (queuedMessages.Count > 0 && !token.IsCancellationRequested)
                        sendThrottle.Enabled = true;
                }
            }
        }
    }
}
