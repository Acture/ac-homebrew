using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ACHomebrew;
using ACHomebrew.Feats;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json.Linq;

namespace ACHomebrew
{
    internal static class Mod { internal static Logger Log = new Logger(); }
    internal sealed class Logger
    {
        internal readonly List<string> Messages = new();
        public void Log(string message) => Messages.Add(message);
    }
}

internal static class Program
{
    private static int checks;
    private static void Equal(string expected, string actual)
    {
        if (expected != actual) throw new Exception($"Expected '{expected}', got '{actual}'");
        checks++;
    }

    private static int Main()
    {
        try
        {
            RunChecks();
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void RunChecks()
    {
        Equal("Titan's Apotheosis", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", false));
        Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));
        Equal("fallback", FeatTextCatalog.Get("Missing.Name", "fallback", true));
        Equal("English", FeatTextCatalog.SelectText("English", null, "fallback", true));
        Equal("English", FeatTextCatalog.SelectText("English", "  ", "fallback", true));
        Equal("English", FeatTextCatalog.SelectText("English", "中文", "fallback", false));
        Equal("fallback", FeatTextCatalog.SelectText(null, null, "fallback", false));
        Equal("<i>文本</i>\n{0}", FeatTextCatalog.SelectText("<i>text</i>\n{0}", "<i>文本</i>\n{0}", "fallback", true));
        var english = Embedded("AttributeFeats.Localization.FeatText.json");
        var chinese = Embedded("AttributeFeats.Localization.zhCN.json");
        var entries = english.Properties().ToDictionary(e => e.Name,
            e => new JObject { ["enGB"] = e.Value.DeepClone(), ["zhCN"] = chinese[e.Name]?.DeepClone() });
        Equal(string.Join(",", english.Properties().Select(p => p.Name).Order()),
            string.Join(",", chinese.Properties().Select(p => p.Name).Order()));
        foreach (var entry in entries)
        {
            if (!TextFormat.Compatible((string)entry.Value["enGB"], (string)entry.Value["zhCN"]))
                throw new Exception("Invalid bilingual placeholders or markup: " + entry.Key);
            checks++;
        }
        checks += SourceChecks.Verify(english, chinese);
        var menus = new[] { "Root", "MainAttribute", "Defensive", "Maneuver", "Skilled", "Arcane", "Stance", "Conditional", "WeaponInsight", "ExtendedReplacement", "GreaterSummoning", "SummonerSacrifice", "ReactiveArmor", "DerivedStat", "SpellSchool", "SpellDescriptor", "DistanceDamage", "WeaponDamage", "MainAttribute_Str", "MainAttribute_Dex", "MainAttribute_Con", "MainAttribute_Int", "MainAttribute_Wis", "MainAttribute_Cha", "CastingStat", "ResourceStat", "Retaliation", "Momentum", "Stealth", "Solo", "Growth", "Execution", "Arcana", "Summoner", "Survival", "Meme", "Penetration" };
        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        var newKeys = menus.Select(m => "AttributeFeatsSelection_" + m)
            .Concat(attributes.Select(a => "WeaponDamage_" + a))
            .SelectMany(prefix => new[] { prefix + ".Name", prefix + ".Description" })
            .Concat(new[] { "Count", "Points", "CountFull", "PointsShort", "OverBudget" }.Select(k => "FeatBudget." + k).Append("FeatGroups.Member")).ToArray();
        foreach (var key in newKeys)
        {
            if (!entries.TryGetValue(key, out var entry)
                || string.IsNullOrWhiteSpace((string)entry["zhCN"])
                || (string)entry["zhCN"] == (string)entry["enGB"])
                throw new Exception("Missing bilingual menu/weapon/budget text: " + key);
            checks++;
        }

        // Run the production registration and refresh methods against an in-memory
        // localization pack. BlueprintCore/Unity boundaries are doubles, not a game.
        LocalizationManager.CurrentLocale = Locale.enGB;
        foreach (var entry in entries)
        {
            Common.L(entry.Key, "compiled fallback");
            Equal((string)entry.Value["enGB"], LocalizationManager.CurrentPack.GetString(entry.Key));
        }
        Common.L("Missing.Bilingual", "English fallback", "中文回退");
        Common.L("Missing.EnglishOnly", "English only");
        Common.L("Missing.Tagged", "<b>English</b> {0}", "<b>中文</b> {0}", true);
        Equal("tag:<b>English</b> {0}", LocalizationManager.CurrentPack.GetString("Missing.Tagged"));

        // Locale changes replace the entire game pack. Previously returned keys
        // must be repopulated, including ones registered while English was active.
        LocalizationManager.CurrentPack = new LocalizationPack();
        LocalizationManager.CurrentLocale = Locale.zhCN;
        Common.RefreshLocale();
        foreach (var entry in entries)
            Equal((string)entry.Value["zhCN"], LocalizationManager.CurrentPack.GetString(entry.Key));
        Equal("中文回退", LocalizationManager.CurrentPack.GetString("Missing.Bilingual"));
        Equal("English only", LocalizationManager.CurrentPack.GetString("Missing.EnglishOnly"));
        Equal("tag:<b>中文</b> {0}", LocalizationManager.CurrentPack.GetString("Missing.Tagged"));
        Common.L("WeaponDamage_Wisdom.Name", "compiled fallback");
        Equal("以感知计算武器伤害", LocalizationManager.CurrentPack.GetString("WeaponDamage_Wisdom.Name"));

        LocalizationManager.CurrentPack = new LocalizationPack();
        LocalizationManager.CurrentLocale = Locale.enGB;
        Common.RefreshLocale();
        foreach (var entry in entries)
            Equal((string)entry.Value["enGB"], LocalizationManager.CurrentPack.GetString(entry.Key));
        Equal("English fallback", LocalizationManager.CurrentPack.GetString("Missing.Bilingual"));
        Equal("tag:<b>English</b> {0}", LocalizationManager.CurrentPack.GetString("Missing.Tagged"));

        // Runtime-built budget text follows the active locale without registration.
        LocalizationManager.CurrentLocale = Locale.zhCN;
        Equal("还差 {0} 点", Common.CurrentText("FeatBudget.PointsShort", "fallback"));
        LocalizationManager.CurrentLocale = Locale.enGB;
        Equal("Requires {0} more points", Common.CurrentText("FeatBudget.PointsShort", "fallback"));
        Equal("fallback", Common.CurrentText("FeatBudget.Missing", "fallback"));

        LocalizationManager.CurrentLocale = Locale.Other;
        Common.RefreshLocale();
        Equal("Wisdom to Weapon Damage", LocalizationManager.CurrentPack.GetString("WeaponDamage_Wisdom.Name"));
        LocalizationManager.CurrentPack = null;
        Common.RefreshLocale(); // Startup without a pack is deliberately harmless.
        checks++;
        CheckExternalFiles(english, chinese);
        Console.WriteLine($"{checks} localization assertions passed: {entries.Count} resources, {newKeys.Length} menu/weapon/budget keys, EN -> zhCN -> EN, fallbacks and tagging (outside the game).");
    }

    private static JObject Embedded(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        using var reader = new StreamReader(stream);
        return JObject.Parse(reader.ReadToEnd());
    }

    private static void CheckExternalFiles(JObject english, JObject chinese)
    {
        var directory = Path.Combine(Path.GetTempPath(), "ACHomebrew-localization-" + Guid.NewGuid().ToString("N"));
        var localization = Path.Combine(directory, "Localization");
        var messages = new List<string>();
        void Load() { messages.Clear(); FeatTextCatalog.Initialize(directory, messages.Add); }
        void Logged(string text)
        {
            if (!messages.Any(m => m.Contains(text))) throw new Exception("Missing diagnostic: " + text);
            checks++;
        }
        void Write(string en, string zh)
        {
            File.WriteAllText(Path.Combine(localization, "enGB.json"), en);
            File.WriteAllText(Path.Combine(localization, "zhCN.json"), zh);
        }
        try
        {
            Load();
            Logged("enGB.json");
            Logged("zhCN.json");
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));
            Directory.CreateDirectory(localization);
            var editedEnglish = (JObject)english.DeepClone();
            var editedChinese = (JObject)chinese.DeepClone();
            editedEnglish["MainAttr_Str.Name"] = "Edited external name";
            editedChinese["MainAttr_Str.Name"] = "外置译文";
            // A template formerly compiled in a feat builder is also editable.
            const string template = "Text.FeatBuilders.Desc.Text";
            editedChinese[template] = "<i>{0}</i>\n\n<b>效果：</b>外置规则：{1}";
            Write(editedEnglish.ToString(), editedChinese.ToString());
            Load();
            Equal("0", messages.Count.ToString());
            Equal("Edited external name", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", false));
            Equal("外置译文", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));
            LocalizationManager.CurrentPack = new LocalizationPack();
            LocalizationManager.CurrentLocale = Locale.zhCN;
            Common.L("MainAttr_Str.Name", "compiled English", "compiled Chinese");
            Equal("外置译文", LocalizationManager.CurrentPack.GetString("MainAttr_Str.Name"));
            Equal("<i>家族</i>\n\n<b>效果：</b>外置规则：伤害+1",
                Common.Format(template, "fallback", true, "家族", "伤害+1"));
            Common.L("External.Rule", Common.Format(template, "fallback", false, "Family", "+1 damage"),
                Common.Format(template, "fallback", true, "家族", "伤害+1"), true);
            Equal("tag:<i>家族</i>\n\n<b>效果：</b>外置规则：伤害+1",
                LocalizationManager.CurrentPack.GetString("External.Rule"));
            LocalizationManager.CurrentLocale = Locale.enGB;
            Common.RefreshLocale();
            Equal("Edited external name", LocalizationManager.CurrentPack.GetString("MainAttr_Str.Name"));

            editedChinese.Remove("MainAttr_Str.Name");
            editedChinese["FeatBudget.PointsShort"] = "无效占位符 {9}";
            editedChinese["WeaponDamage_Wisdom.Name"] = " ";
            editedChinese["Unknown.Key"] = "unknown";
            Write(english.ToString(), editedChinese.ToString());
            Load();
            Logged("missing 1 translations");
            Logged("invalid translation FeatBudget.PointsShort");
            Logged("invalid translation WeaponDamage_Wisdom.Name");
            Logged("unknown localization key");
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));
            Equal("还差 {0} 点", FeatTextCatalog.Get("FeatBudget.PointsShort", "fallback", true));
            Equal("以感知计算武器伤害", FeatTextCatalog.Get("WeaponDamage_Wisdom.Name", "fallback", true));
            foreach (var invalid in new[] { "{", "[]", "{\"Key\":null}", "{\"Key\":\"one\",\"Key\":\"two\"}", "{} {}" })
            {
                Write(invalid, invalid);
                Load();
                Logged("cannot load localization");
                Equal("Titan's Apotheosis", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", false));
                Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));
            }
            Write(english.ToString(), chinese.ToString());
            File.Delete(Path.Combine(localization, "zhCN.json"));
            Load();
            Logged("zhCN.json");
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", true));

            Equal("True", TextFormat.Compatible("<b>{0}</b>\n{g|Encyclopedia:Armor}armor{/g}",
                "<b>{0}</b>\n{g|Encyclopedia:Armor}护甲{/g}").ToString());
            foreach (var invalid in new[] { "<b>{1}</b>", "<b>{0}</i>", "{0", "{g|Encyclopedia:Other}护甲{/g}" })
                Equal("False", TextFormat.Compatible("<b>{0}</b>\n{g|Encyclopedia:Armor}armor{/g}", invalid).ToString());
            Equal("True", TextFormat.Compatible("{0:0.00} {{literal}}", "{{literal}} {0:0.00}").ToString());
            Equal("False", TextFormat.Compatible("{0:0.00}", "{0}").ToString());
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            FeatTextCatalog.Initialize(null, Mod.Log.Log);
        }
    }
}
