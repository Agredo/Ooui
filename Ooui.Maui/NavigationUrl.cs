using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Ooui.Maui;

/// <summary>
/// Derives the URL hash of a navigation stack, for example <c>#/main/details</c>.
/// </summary>
public static class NavigationUrl
{
    /// <summary>
    /// Gets the hash segment of a page: its <c>StyleId</c> when set,
    /// otherwise the page type name without a trailing "Page", in kebab case.
    /// </summary>
    public static string GetSegment(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!string.IsNullOrWhiteSpace(page.StyleId))
            return page.StyleId;
        return ToKebabCase(GetShortTypeName(page.GetType().Name));
    }

    /// <summary>
    /// Gets the hash of the whole stack, for example <c>#/main/details</c>.
    /// </summary>
    public static string GetHash(IReadOnlyList<IView> stack) => GetHash(stack, stack.Count);

    /// <summary>
    /// Gets the hash of the first <paramref name="count"/> views of a stack.
    /// </summary>
    public static string GetHash(IReadOnlyList<IView> stack, int count)
    {
        ArgumentNullException.ThrowIfNull(stack);
        var builder = new StringBuilder();
        for (var i = 0; i < count && i < stack.Count; i++)
            builder.Append('/').Append(GetSegmentOf(stack[i]));
        return builder.Length == 0 ? "" : "#" + builder;
    }

    /// <summary>
    /// Brings a hash sent by the browser into the same shape <see cref="GetHash(IReadOnlyList{IView})"/> produces.
    /// </summary>
    public static string Normalize(string? hash)
    {
        if (string.IsNullOrEmpty(hash))
            return "";
        var value = hash[0] == '#' ? hash[1..] : hash;
        while (value.EndsWith('/'))
            value = value[..^1];
        return value.Length == 0 ? "" : "#" + (value[0] == '/' ? value : "/" + value);
    }

    static string GetSegmentOf(IView view) =>
        view is Page page ? GetSegment(page) : ToKebabCase(GetShortTypeName(view.GetType().Name));

    static string GetShortTypeName(string typeName) =>
        typeName.Length > 4 && typeName.EndsWith("Page", StringComparison.Ordinal)
            ? typeName[..^4]
            : typeName;

    static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                var startsWord = i > 0 &&
                    (char.IsLower(name[i - 1]) || char.IsDigit(name[i - 1]) ||
                     (i + 1 < name.Length && char.IsLower(name[i + 1])));
                if (startsWord && builder.Length > 0 && builder[builder.Length - 1] != '-')
                    builder.Append('-');
                builder.Append(char.ToLowerInvariant(c));
            }
            else if (c is '_' or ' ' || char.IsWhiteSpace(c))
            {
                if (builder.Length > 0 && builder[builder.Length - 1] != '-')
                    builder.Append('-');
            }
            else
            {
                builder.Append(char.ToLowerInvariant(c));
            }
        }
        return builder.ToString();
    }
}
