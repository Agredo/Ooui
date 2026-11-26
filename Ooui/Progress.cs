namespace Ooui
{
    public class Progress : Element
    {
        public double Maximum {
            get => GetAttribute ("max", 1.0);
            set => SetAttributeProperty ("max", value.ToString (System.Globalization.CultureInfo.InvariantCulture));
        }

        public string Value {
            get => GetStringAttribute ("value", "0");
            set => SetAttributeProperty ("value", value ?? "0");
        }

        public Progress () : base ("progress") { }
    }
}
