using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

internal static class SourceChecks
{
    internal static int Verify(JObject english, JObject chinese)
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
                    .Where(c => c.Expression.ToString() is "Common.Text" or "Common.Format" or "Common.CurrentText"))
                {
                    var args = call.ArgumentList.Arguments;
                    if (!(args[0].Expression is LiteralExpressionSyntax key)) continue;
                    var name = key.Token.ValueText;
                    if (english[name] == null || chinese[name] == null)
                        throw new Exception($"Missing resource {name}: {file}");
                    if (args[1].Expression is LiteralExpressionSyntax fallback)
                    {
                        var zh = args.Count >= 3 && args[2].Expression.ToString() == "true";
                        var resource = (string)(zh ? chinese : english)[name];
                        if (fallback.Token.ValueText != resource)
                            throw new Exception($"Compiled fallback differs from canonical resource {name}: {file}");
                    }
                    checks++;
                }
                // Feat and buff names, lore and rules must all use the shared catalog.
                // Logic labels and the separate settings UI are not feat descriptions.
                if (project.EndsWith("ACHomebrew") || project.EndsWith("ACHomebrew.Logic")) continue;
                foreach (var literal in syntax.DescendantNodes().OfType<LiteralExpressionSyntax>()
                    .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression) && l.Token.ValueText.Any(c => c >= '\u3400' && c <= '\u9fff')))
                {
                    if (literal.Token.ValueText == "、") continue;
                    if (!literal.Ancestors().OfType<InvocationExpressionSyntax>().Any(c => c.Expression.ToString() is "Common.Text" or "Common.Format"))
                        throw new Exception($"Player text remains outside localization: {file}:{literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                    checks++;
                }
            }
        }
        return checks;
    }
}
