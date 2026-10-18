using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

namespace ACHomebrew.OfflineTests
{
    /// <summary>
    /// Every string the mod registers during real initialization, in every supported locale:
    /// nonblank, no Chinese outside zhCN, and longer texts actually translated.
    /// </summary>
    [Collection(ACHomebrewGameCollection.Name)]
    public sealed class LocalizationTests
    {
        private readonly ACHomebrewGame fixture;
        public LocalizationTests(ACHomebrewGame fixture) => this.fixture = fixture;

        [Fact]
        public void EveryRegisteredStringIsTranslatedInEverySupportedLocale()
        {
            fixture.RequireGame();
            var core = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("ACHomebrew.Feats.Common", false)).First(t => t != null);
            var map = (IDictionary)core.GetField("LocalizedStrings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var locales = (Array)core.Assembly.GetType("ACHomebrew.Feats.LocalizedText").GetField("Locales").GetValue(null);
            Assert.True(map.Count > 500, "Expected the full feat catalog to register, got " + map.Count);
            Assert.Equal(8, locales.Length);

            var problems = new List<string>();
            foreach (DictionaryEntry entry in map)
            {
                var text = entry.Value.GetType().GetField("Item1").GetValue(entry.Value);
                var indexer = text.GetType().GetProperty("Item");
                var english = (string)text.GetType().GetProperty("English").GetValue(text);
                foreach (var locale in locales.Cast<object>())
                {
                    var value = (string)indexer.GetValue(text, new[] { locale });
                    var name = locale.ToString();
                    if (string.IsNullOrWhiteSpace(value)) problems.Add($"{entry.Key} {name}: blank");
                    else if (name != "zhCN" && Regex.IsMatch(value, @"[　-鿿]")) problems.Add($"{entry.Key} {name}: Chinese text");
                    else if (name != "enGB" && english.Length > 40 && value == english) problems.Add($"{entry.Key} {name}: untranslated");
                }
            }
            Assert.True(problems.Count == 0, string.Join("\n", problems.Take(20)));
        }
    }
}
