using System;
using System.Collections.Generic;
using System.Globalization;

namespace ACHomebrew.Feats
{
    internal static class TextFormat
    {
        internal static bool Compatible(string original, string translated)
            => Signature(original, out var expected) && Signature(translated, out var actual) && expected.SetEquals(actual);

        private static bool Signature(string text, out HashSet<string> signature)
        {
            signature = new HashSet<string>(StringComparer.Ordinal);
            var tags = new Stack<string>();
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '{')
                {
                    if (i + 1 < text.Length && text[i + 1] == '{') { i++; continue; }
                    var end = text.IndexOf('}', i + 1);
                    if (end < 0) return false;
                    var value = text.Substring(i + 1, end - i - 1);
                    if (value.StartsWith("g|", StringComparison.Ordinal) || value.StartsWith("d|", StringComparison.Ordinal))
                    {
                        signature.Add("link:" + value);
                        tags.Push("{" + value[0]);
                    }
                    else if (value == "/g" || value == "/d")
                    {
                        if (tags.Count == 0 || tags.Pop() != "{" + value[1]) return false;
                    }
                    else
                    {
                        var separator = value.IndexOfAny(new[] { ',', ':' });
                        var indexText = separator < 0 ? value : value.Substring(0, separator);
                        if (!int.TryParse(indexText, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                            || index < 0 || index >= 64) return false;
                        try { string.Format(CultureInfo.InvariantCulture, "{" + value + "}", new object[64]); }
                        catch (FormatException) { return false; }
                        signature.Add("argument:" + value);
                    }
                    i = end;
                }
                else if (c == '}')
                {
                    if (i + 1 >= text.Length || text[i + 1] != '}') return false;
                    i++;
                }
                else if (c == '<')
                {
                    var end = text.IndexOf('>', i + 1);
                    if (end < 0)
                    {
                        if (i + 1 < text.Length && char.IsLetter(text[i + 1])) return false;
                        continue;
                    }
                    var tag = text.Substring(i + 1, end - i - 1);
                    // Inequalities such as HP < 50% are prose, not rich-text tags.
                    if (tag.Length == 0 || !(char.IsLetter(tag[0]) || tag[0] == '/')) continue;
                    signature.Add("tag:" + tag);
                    if (tag[0] == '/')
                    {
                        if (tags.Count == 0 || tags.Pop() != tag.Substring(1)) return false;
                    }
                    else if (!tag.EndsWith("/", StringComparison.Ordinal))
                    {
                        var equals = tag.IndexOf('=');
                        tags.Push(equals < 0 ? tag : tag.Substring(0, equals));
                    }
                    i = end;
                }
            }
            return tags.Count == 0;
        }
    }
}
