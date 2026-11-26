namespace Ooui
{
    public class Window : EventTarget
    {
        string location = "";
        string hash = "";

        public string Location {
            get => location;
            set {
                if (string.IsNullOrEmpty (value) || location == value)
                    return;
                location = value;
                Send (Message.Set ("window", "location", value));
                OnPropertyChanged ("Location");
            }
        }

        /// <summary>
        /// The hash that was last synchronized with the browser.
        /// </summary>
        public string Hash => hash;

        /// <summary>
        /// Sets the browser hash, which adds an entry to the browser history.
        /// </summary>
        public void SetHash (string value) {
            value = value ?? "";
            if (hash == value)
                return;
            hash = value;
            Send (Message.Set ("window", "location.hash", value));
            OnPropertyChanged ("Hash");
        }

        /// <summary>
        /// Rewrites the browser hash without adding an entry to the browser history.
        /// </summary>
        public void ReplaceHash (string value) {
            value = value ?? "";
            if (hash == value)
                return;
            hash = value;
            Call ("history.replaceState", null, "", value.Length > 0 ? value : "#");
            OnPropertyChanged ("Hash");
        }

        public Window ()
            : base ("window")
        {
            Id = "window";
        }
    }
}
