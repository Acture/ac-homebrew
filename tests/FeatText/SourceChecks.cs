using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

internal static class SourceChecks
{
    private static readonly string[] Lookups = { "Common.Text", "Common.Format", "Common.CurrentText", "LocalizedText.Text", "LocalizedText.Format" };

    internal static int Verify(JObject english, IEnumerable<JObject> translations)
    {
        var root = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(root, "ACHomebrew.slnx")))
            root = Directory.GetParent(root)?.FullName ?? throw new Exception("Cannot locate repository root.");
        var checks = 0;
        foreach (var project in Directory.GetDirectories(Path.Combine(root, "src"), "ACHomebrew*"))
        {
            foreach (var file in Directory.GetFiles(project, "*.cs"))
            {
                var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(file),
                    new CSharpParseOptions(preprocessorSymbols: new[] { "MODMENU" })).GetRoot();
                foreach (var call in syntax.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(c => Lookups.Contains(c.Expression.ToString())))
                {
                    var args = call.ArgumentList.Arguments;
                    if (!(args[0].Expression is LiteralExpressionSyntax key)) continue;
                    var name = key.Token.ValueText;
                    if (english[name] == null || translations.Any(pack => pack[name] == null))
                        throw new Exception($"Missing resource {name}: {file}");
                    // Compiled fallbacks are English only; every other language lives in the catalog.
                    if (args[1].Expression is LiteralExpressionSyntax fallback && fallback.Token.ValueText != (string)english[name])
                        throw new Exception($"Compiled fallback differs from canonical English resource {name}: {file}");
                    checks++;
                }
                // Translations live only in the language files: no Chinese (or other CJK) text in source,
                // and no code path that special-cases one locale's wording.
                foreach (var literal in syntax.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression) && l.Token.ValueText.Any(c => c >= '\u3000' && c <= '\u9fff' || c >= '\uff00' && c <= '\uffef')))
                    throw new Exception($"Translated text belongs in the language files: {file}:{literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                foreach (var access in syntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                    .Where(a => a.ToString() == "Locale.zhCN" && Path.GetFileName(file) != "LocalizedText.cs"))
                    throw new Exception($"Locale-specific wording belongs in the language files: {file}:{access.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                checks++;
            }
        }
        return checks;
    }
}
