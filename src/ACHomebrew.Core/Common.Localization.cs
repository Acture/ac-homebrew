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
        private static readonly Dictionary<string, (string en, string zh, bool tag)> LocalizedStrings = new();

        public static string Text(string key, string fallback, bool chinese = false)
            => FeatTextCatalog.Get(key, fallback, chinese);

        public static string Format(string key, string fallback, bool chinese, params object[] arguments)
            => string.Format(CultureInfo.CurrentCulture, Text(key, fallback, chinese), arguments);

        /// <summary>Text for runtime-built UI strings in the active game locale.</summary>
        public static string CurrentText(string key, string fallback)
            => FeatTextCatalog.Get(key, fallback, LocalizationManager.CurrentLocale == Locale.zhCN);

        public static LocalizedString L(string key, string enValue, string zhValue = null, bool tagEncyclopediaEntries = false)
        {
            enValue = FeatTextCatalog.Get(key, enValue, false);
            zhValue = FeatTextCatalog.Get(key, zhValue, true);
            LocalizedStrings[key] = (enValue, zhValue, tagEncyclopediaEntries);
            var isZh = LocalizationManager.CurrentLocale == Locale.zhCN;
            var text = (isZh && !string.IsNullOrEmpty(zhValue)) ? zhValue : enValue;
            return LocalizationTool.CreateString(key, text, tagEncyclopediaEntries);
        }

        public static LocalizedString L(string key, string value, bool tagEncyclopediaEntries)
            => L(key, value, null, tagEncyclopediaEntries);

        public static void RefreshLocale()
        {
            try
            {
                var pack = LocalizationManager.CurrentPack;
                if (pack == null) return;
                var isZh = LocalizationManager.CurrentLocale == Locale.zhCN;
                foreach (var entry in LocalizedStrings)
                {
                    var text = (isZh && !string.IsNullOrEmpty(entry.Value.zh)) ? entry.Value.zh : entry.Value.en;
                    pack.PutString(entry.Key, entry.Value.tag ? EncyclopediaTool.TagEncyclopediaEntries(text) : text);
                }
            }
            catch (System.Exception ex)
            {
                Mod.Log?.Log("AttributeFeats: RefreshLocale error: " + ex);
            }
        }

        // OnLocaleChanged is a private method, not an event. BlueprintCore's own postfix there re-applies
        // the creation-time (single-locale) text, so ours must run after it to restore the zhCN strings.
        [HarmonyPatch(typeof(LocalizationManager), "OnLocaleChanged")]
        private static class LocalizationManager_OnLocaleChanged_Patch
        {
            [HarmonyPriority(Priority.Last)]
            [HarmonyPostfix]
            private static void Postfix() => RefreshLocale();
        }

    }
}
