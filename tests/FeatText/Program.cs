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
        Equal("Titan's Apotheosis", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.enGB));
        Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));
        Equal("fallback", FeatTextCatalog.Get("Missing.Name", "fallback", Locale.zhCN));
        Equal("Titan's Apotheosis", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.Sound));
        Equal(null, FeatTextCatalog.Find("Missing.Name", Locale.deDE));

        var packs = LocalizedText.Locales.ToDictionary(locale => locale,
            locale => Embedded(FeatTextCatalog.ResourceName(locale)));
        var english = packs[Locale.enGB];
        var keys = english.Properties().Select(p => p.Name).ToArray();
        foreach (var (locale, pack) in packs)
        {
            Equal(string.Join(",", keys.Order()), string.Join(",", pack.Properties().Select(p => p.Name).Order()));
            foreach (var key in keys)
            {
                if (string.IsNullOrWhiteSpace((string)pack[key]) || !TextFormat.Compatible((string)english[key], (string)pack[key]))
                    throw new Exception($"Invalid {locale} placeholders, markup or blank text: {key}");
                checks++;
            }
        }
        checks += SourceChecks.Verify(english, packs.Where(p => p.Key != Locale.enGB).Select(p => p.Value));

        var menus = new[] { "Root", "MainAttribute", "Defensive", "Maneuver", "Skilled", "Arcane", "Stance", "Conditional", "WeaponInsight", "ExtendedReplacement", "GreaterSummoning", "SummonerSacrifice", "ReactiveArmor", "DerivedStat", "SpellSchool", "SpellDescriptor", "DistanceDamage", "WeaponDamage", "MainAttribute_Str", "MainAttribute_Dex", "MainAttribute_Con", "MainAttribute_Int", "MainAttribute_Wis", "MainAttribute_Cha", "CastingStat", "ResourceStat", "Retaliation", "Momentum", "Stealth", "Solo", "Growth", "Execution", "Arcana", "Summoner", "Survival", "Meme", "Penetration" };
        var attributes = new[] { "Strength", "Dexterity", "Constitution", "Intelligence", "Wisdom", "Charisma" };
        var newKeys = menus.Select(m => "ACHomebrewSelection_" + m)
            .Concat(attributes.Select(a => "WeaponDamage_" + a))
            .SelectMany(prefix => new[] { prefix + ".Name", prefix + ".Description" })
            .Concat(new[] { "Count", "Points", "CountFull", "PointsShort", "OverBudget" }.Select(k => "FeatBudget." + k).Append("FeatGroups.Member")).ToArray();
        foreach (var key in newKeys)
        {
            // Descriptions are full sentences, so no translation may be a copy of the English text.
            foreach (var (locale, pack) in packs.Where(p => p.Key != Locale.enGB))
            {
                if (pack[key] == null || (key.EndsWith(".Description") || locale == Locale.zhCN)
                    && (string)pack[key] == (string)english[key])
                    throw new Exception($"Untranslated {locale} menu/weapon/budget text: {key}");
                checks++;
            }
        }

        // Run the production registration and refresh methods against an in-memory
        // localization pack. BlueprintCore/Unity boundaries are doubles, not a game.
        LocalizationManager.CurrentLocale = Locale.enGB;
        foreach (var key in keys)
        {
            Common.L(key, "compiled fallback");
            Equal((string)english[key], LocalizationManager.CurrentPack.GetString(key));
        }
        Common.L("Missing.Composed", LocalizedText.Compose(locale => "composed " + locale));
        Common.L("Missing.EnglishOnly", "English only");
        Common.L("Missing.Tagged", LocalizedText.Compose(locale => $"<b>{locale}</b> {{0}}"), true);
        Equal("tag:<b>enGB</b> {0}", LocalizationManager.CurrentPack.GetString("Missing.Tagged"));

        // Locale changes replace the entire game pack. Previously returned keys
        // must be repopulated, including ones registered while English was active.
        foreach (var (locale, pack) in packs.Reverse())
        {
            LocalizationManager.CurrentPack = new LocalizationPack();
            LocalizationManager.CurrentLocale = locale;
            Common.RefreshLocale();
            foreach (var key in keys)
                Equal((string)pack[key], LocalizationManager.CurrentPack.GetString(key));
            Equal("composed " + locale, LocalizationManager.CurrentPack.GetString("Missing.Composed"));
            Equal("English only", LocalizationManager.CurrentPack.GetString("Missing.EnglishOnly"));
            Equal($"tag:<b>{locale}</b> {{0}}", LocalizationManager.CurrentPack.GetString("Missing.Tagged"));
        }
        Common.L("WeaponDamage_Wisdom.Name", "compiled fallback");
        Equal("Wisdom to Weapon Damage", LocalizationManager.CurrentPack.GetString("WeaponDamage_Wisdom.Name"));
        LocalizationManager.CurrentPack = new LocalizationPack();
        LocalizationManager.CurrentLocale = Locale.zhCN;
        Common.RefreshLocale();
        Equal("以感知计算武器伤害", LocalizationManager.CurrentPack.GetString("WeaponDamage_Wisdom.Name"));

        // Templates compose per locale; LocalizedText arguments resolve in the same locale.
        var family = LocalizedText.Text("ACHomebrewSelection_Arcana.Name", "fallback");
        var rule = LocalizedText.Format("Text.FeatBuilders.Desc.Text", "fallback", family, 3);
        foreach (var (locale, pack) in packs)
            Equal(string.Format((string)pack["Text.FeatBuilders.Desc.Text"], (string)pack["ACHomebrewSelection_Arcana.Name"], 3), rule[locale]);
        Equal(rule[Locale.enGB], rule[Locale.Sound]);

        // Localized lists keep the previous English and Chinese wording.
        Equal("", Common.JoinList(new string[0], Locale.enGB));
        Equal("Fire", Common.JoinList(new[] { "Fire" }, Locale.enGB));
        Equal("Fire and Cold", Common.JoinList(new[] { "Fire", "Cold" }, Locale.enGB));
        Equal("Fire, Cold, and Acid", Common.JoinList(new[] { "Fire", "Cold", "Acid" }, Locale.enGB));
        Equal("火焰、寒冷", Common.JoinList(new[] { "火焰", "寒冷" }, Locale.zhCN));
        Equal("火焰、寒冷、强酸", Common.JoinList(new[] { "火焰", "寒冷", "强酸" }, Locale.zhCN));

        // Runtime-built budget text follows the active locale without registration.
        LocalizationManager.CurrentLocale = Locale.zhCN;
        Equal("还差 {0} 点", Common.CurrentText("FeatBudget.PointsShort", "fallback"));
        LocalizationManager.CurrentLocale = Locale.deDE;
        Equal((string)packs[Locale.deDE]["FeatBudget.PointsShort"], Common.CurrentText("FeatBudget.PointsShort", "fallback"));
        LocalizationManager.CurrentLocale = Locale.enGB;
        Equal("Requires {0} more points", Common.CurrentText("FeatBudget.PointsShort", "fallback"));
        Equal("fallback", Common.CurrentText("FeatBudget.Missing", "fallback"));

        LocalizationManager.CurrentLocale = Locale.Sound;
        Common.RefreshLocale();
        Equal("Wisdom to Weapon Damage", LocalizationManager.CurrentPack.GetString("WeaponDamage_Wisdom.Name"));
        LocalizationManager.CurrentPack = null;
        Common.RefreshLocale(); // Startup without a pack is deliberately harmless.
        checks++;
        CheckExternalFiles(english, packs[Locale.zhCN], packs[Locale.deDE]);
        Console.WriteLine($"{checks} localization assertions passed: {keys.Length} resources in {packs.Count} languages, {newKeys.Length} menu/weapon/budget keys, locale round trip, composition, fallbacks and tagging (outside the game).");
    }

    private static JObject Embedded(string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
            ?? throw new Exception("Missing embedded language file " + name);
        using var reader = new StreamReader(stream);
        return JObject.Parse(reader.ReadToEnd());
    }

    private static void CheckExternalFiles(JObject english, JObject chinese, JObject german)
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
        void Write(string en, string zh, string de = null)
        {
            File.WriteAllText(Path.Combine(localization, "enGB.json"), en);
            File.WriteAllText(Path.Combine(localization, "zhCN.json"), zh);
            File.WriteAllText(Path.Combine(localization, "deDE.json"), de ?? german.ToString());
        }
        try
        {
            Load();
            foreach (var locale in LocalizedText.Locales) Logged(locale + ".json");
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));
            Directory.CreateDirectory(localization);
            var editedEnglish = (JObject)english.DeepClone();
            var editedChinese = (JObject)chinese.DeepClone();
            var editedGerman = (JObject)german.DeepClone();
            editedEnglish["MainAttr_Str.Name"] = "Edited external name";
            editedChinese["MainAttr_Str.Name"] = "外置译文";
            editedGerman["MainAttr_Str.Name"] = "Externer Name";
            // A template formerly compiled in a feat builder is also editable.
            const string template = "Text.FeatBuilders.Desc.Text";
            editedChinese[template] = "<i>{0}</i>\n\n<b>效果：</b>外置规则：{1}";
            Write(editedEnglish.ToString(), editedChinese.ToString(), editedGerman.ToString());
            Load();
            Equal("5", messages.Count.ToString()); // only the five untouched locale files are reported missing
            Equal("Edited external name", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.enGB));
            Equal("外置译文", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));
            Equal("Externer Name", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.deDE));
            LocalizationManager.CurrentPack = new LocalizationPack();
            LocalizationManager.CurrentLocale = Locale.zhCN;
            Common.L("MainAttr_Str.Name", "compiled English");
            Equal("外置译文", LocalizationManager.CurrentPack.GetString("MainAttr_Str.Name"));
            Equal("<i>家族</i>\n\n<b>效果：</b>外置规则：伤害+1",
                Common.Format(template, "fallback", Locale.zhCN, "家族", "伤害+1"));
            Common.L("External.Rule", LocalizedText.Format(template, "fallback",
                LocalizedText.Compose(locale => locale == Locale.zhCN ? "家族" : "Family"), "+1"), true);
            Equal("tag:<i>家族</i>\n\n<b>效果：</b>外置规则：+1",
                LocalizationManager.CurrentPack.GetString("External.Rule"));
            LocalizationManager.CurrentLocale = Locale.deDE;
            Common.RefreshLocale();
            Equal("Externer Name", LocalizationManager.CurrentPack.GetString("MainAttr_Str.Name"));
            LocalizationManager.CurrentLocale = Locale.enGB;
            Common.RefreshLocale();
            Equal("Edited external name", LocalizationManager.CurrentPack.GetString("MainAttr_Str.Name"));

            // User-edited translations from the old install keep working after the identity rename.
            var legacyEnglish = (JObject)english.DeepClone();
            legacyEnglish.Remove("ACHomebrewSelection_Root.Name");
            legacyEnglish["AttributeFeatsSelection_Root.Name"] = "My old translated menu";
            Write(legacyEnglish.ToString(), chinese.ToString());
            Load();
            Equal("My old translated menu", FeatTextCatalog.Get("ACHomebrewSelection_Root.Name", "fallback", Locale.enGB));
            Equal("5", messages.Count.ToString());

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
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));
            Equal("还差 {0} 点", FeatTextCatalog.Get("FeatBudget.PointsShort", "fallback", Locale.zhCN));
            Equal("以感知计算武器伤害", FeatTextCatalog.Get("WeaponDamage_Wisdom.Name", "fallback", Locale.zhCN));
            foreach (var invalid in new[] { "{", "[]", "{\"Key\":null}", "{\"Key\":\"one\",\"Key\":\"two\"}", "{} {}" })
            {
                Write(invalid, invalid, invalid);
                Load();
                Logged("cannot load localization");
                Equal("Titan's Apotheosis", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.enGB));
                Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));
                Equal((string)german["MainAttr_Str.Name"], FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.deDE));
            }
            Write(english.ToString(), chinese.ToString());
            File.Delete(Path.Combine(localization, "zhCN.json"));
            Load();
            Logged("zhCN.json");
            Equal("泰坦登阶", FeatTextCatalog.Get("MainAttr_Str.Name", "fallback", Locale.zhCN));

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
