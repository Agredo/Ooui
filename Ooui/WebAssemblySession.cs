using System;
using System.Collections.Generic;
using System.Text;

namespace Ooui
{
    public class WebAssemblySession : Session
    {
        readonly string id;
        readonly Action<Message> handleElementMessageSent;

        public WebAssemblySession (string id, Element element, bool disposeElementAfterSession, double initialWidth, double initialHeight, Action<string, Exception> errorLogger)
            : base (element, disposeElementAfterSession, initialWidth, initialHeight, errorLogger)
        {
            this.id = id;
            handleElementMessageSent = QueueMessage;
        }

        protected override void QueueMessage (Message message)
        {
            lock (queuedMessages) {
                QueueMessageLocked (message);
                var max = 1;
                var i = 0;
                while (i < queuedMessages.Count) {
                    TransmitQueuedMessagesLocked (queuedMessages, i, max);
                    i += max;
                }
                // WebAssembly.Runtime.InvokeJS("console.log('TRANSMITTED'," + queuedMessages.Count + ")");
                queuedMessages.Clear ();
            }
        }

        void TransmitQueuedMessagesLocked (List<Message> messagesToSend, int startIndex, int max)
        {
            if (messagesToSend.Count == 0)
                return;

            var sendMessages = WebAssemblyRuntime.SendMessages;
            if (sendMessages == null)
                throw new InvalidOperationException ("No web assembly bridge is configured. Set WebAssemblyRuntime.SendMessages before starting a web assembly session.");

            //
            // Now actually send the messages
            //
            var sb = new System.IO.StringWriter ();
            sb.Write ("[");
            var head = "";
            for (var i = startIndex; i < messagesToSend.Count && i - startIndex < max; i++) {
                sb.Write (head);
                messagesToSend[i].WriteJson (sb);
                head = ",";
            }
            sb.Write ("]");
            sendMessages (id, sb.ToString ());
        }

        public void ReceiveMessageJson (string json)
        {
            try {
                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message> (json);
                element.Receive (message);
            }
            catch (Exception ex) {
                Error ("Failed to process received message", ex);
            }
        }

        public void StartSession ()
        {
            //
            // Start watching for changes in the element
            //
            element.MessageSent += handleElementMessageSent;

            //
            // Add it to the document body
            //
            if (element.WantsFullScreen) {
                element.Style.Width = initialWidth;
                element.Style.Height = initialHeight;
            }
            QueueMessage (Message.Call ("document.body", "appendChild", element));
        }

        public void StopSession ()
        {
            element.MessageSent -= handleElementMessageSent;
        }
    }
}
