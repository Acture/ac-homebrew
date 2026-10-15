// Only the external localization boundary is replaced. Tests compile the actual
// Common localization implementation and its embedded production catalog.
using System;
using System.Collections.Generic;

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class HarmonyPatch : Attribute { public HarmonyPatch(Type type, string method) { } }
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class HarmonyPriority : Attribute { public HarmonyPriority(int priority) { } }
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class HarmonyPostfix : Attribute { }
    internal static class Priority { public const int Last = 0; }
}

namespace Kingmaker.Localization.Shared
{
    internal enum Locale { enGB, ruRU, deDE, frFR, zhCN, esES, itIT, ptBR, Sound }
}

namespace Kingmaker.Localization
{
    internal sealed class LocalizedString { public string Key; }
    internal sealed class LocalizationPack
    {
        private readonly Dictionary<string, string> strings = new();
        public void PutString(string key, string value) => strings[key] = value;
        public string GetString(string key) => strings[key];
    }
    internal static class LocalizationManager
    {
        public static Shared.Locale CurrentLocale;
        public static LocalizationPack CurrentPack = new();
    }
}

namespace BlueprintCore.Utils
{
    internal static class EncyclopediaTool
    {
        public static string TagEncyclopediaEntries(string text) => "tag:" + text;
    }
    internal static class LocalizationTool
    {
        public static Kingmaker.Localization.LocalizedString CreateString(string key, string text, bool tag)
        {
            Kingmaker.Localization.LocalizationManager.CurrentPack.PutString(key,
                tag ? EncyclopediaTool.TagEncyclopediaEntries(text) : text);
            return new Kingmaker.Localization.LocalizedString { Key = key };
        }
    }
}
