using System.Collections.Generic;
using System.Globalization;
using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;

namespace ACHomebrew.Feats
{
    internal static partial class Common
    {
        private static readonly Dictionary<string, (LocalizedText text, bool tag)> LocalizedStrings = new();

        /// <summary>Catalog text in <paramref name="locale"/>, then English, then the compiled English fallback.</summary>
        public static string Text(string key, string fallback, Locale locale)
            => FeatTextCatalog.Get(key, fallback, locale);

        public static string Format(string key, string fallback, Locale locale, params object[] arguments)
            => string.Format(CultureInfo.CurrentCulture, Text(key, fallback, locale), arguments);

        /// <summary>Text for runtime-built UI strings in the active game locale.</summary>
        public static string CurrentText(string key, string fallback)
            => FeatTextCatalog.Get(key, fallback, LocalizationManager.CurrentLocale);

        /// <summary>
        /// Registers a game string. A catalog entry under <paramref name="key"/> itself overrides the composed
        /// text in that locale, so translators can replace any generated name or description wholesale.
        /// </summary>
        public static LocalizedString L(string key, LocalizedText text, bool tagEncyclopediaEntries = false)
        {
            text = LocalizedText.Compose(locale => FeatTextCatalog.Find(key, locale) ?? text[locale]);
            LocalizedStrings[key] = (text, tagEncyclopediaEntries);
            return LocalizationTool.CreateString(key, text[LocalizationManager.CurrentLocale], tagEncyclopediaEntries);
        }

        /// <summary>Registers the catalog entry <paramref name="key"/>, with compiled English as the fallback.</summary>
        public static LocalizedString L(string key, string englishFallback, bool tagEncyclopediaEntries = false)
            => L(key, LocalizedText.Text(key, englishFallback), tagEncyclopediaEntries);

        /// <summary>Joins names as a localized list ("A, B and C", "A、B、C").</summary>
        public static string JoinList(IReadOnlyList<string> values, Locale locale)
        {
            if (values.Count == 0) return string.Empty;
            if (values.Count == 1) return values[0];
            if (values.Count == 2) return Format("Text.List.Pair", "{0} and {1}", locale, values[0], values[1]);
            var separator = Text("Text.List.Separator", ", ", locale);
            var head = values[0];
            for (var i = 1; i < values.Count - 1; i++) head += separator + values[i];
            return Format("Text.List.Last", "{0}, and {1}", locale, head, values[values.Count - 1]);
        }

        public static void RefreshLocale()
        {
            try
            {
                var pack = LocalizationManager.CurrentPack;
                if (pack == null) return;
                var locale = LocalizationManager.CurrentLocale;
                foreach (var entry in LocalizedStrings)
                {
                    var text = entry.Value.text[locale];
                    pack.PutString(entry.Key, entry.Value.tag ? EncyclopediaTool.TagEncyclopediaEntries(text) : text);
                }
            }
            catch (System.Exception ex)
            {
                Mod.Log?.Log("ACHomebrew: RefreshLocale error: " + ex);
            }
        }

        // OnLocaleChanged is a private method, not an event. BlueprintCore's own postfix there re-applies
        // the creation-time (single-locale) text, so ours must run after it to restore the selected locale.
        [HarmonyPatch(typeof(LocalizationManager), "OnLocaleChanged")]
        private static class LocalizationManager_OnLocaleChanged_Patch
        {
            [HarmonyPriority(Priority.Last)]
            [HarmonyPostfix]
            private static void Postfix() => RefreshLocale();
        }

    }
}
