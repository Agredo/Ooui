using System.Globalization;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Resources;

public partial class AuroraStyles : ResourceDictionary
{
    public AuroraStyles()
    {
        Add("RelativeImageSource", new RelativeImageSourceConverter());
        this.LoadFromXaml(typeof(AuroraStyles));
    }
}
