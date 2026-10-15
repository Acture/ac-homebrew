#if MODMENU
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ACHomebrew.Feats;
using Kingmaker.Localization;
using ModMenu.Settings;
using UnityModManagerNet;
using MenuApi = ModMenu.ModMenu;

namespace ACHomebrew
{
    /// <summary>
    /// Optional ModMenu page that mirrors the UMM settings. ModMenu types are only touched
    /// when ModMenu is loaded; values are written back to the same settings file.
    /// </summary>
    internal static class ModMenuSettings
    {
        private const string Prefix = "achomebrew";
        private static readonly List<Action> PushCurrentValues = new();

        public static void TryRegister()
        {
            if (UnityModManager.FindMod("ModMenu")?.Active != true) return;
            try
            {
                Register();
                Main.Log?.Log("AC's Homebrew: settings added to ModMenu.");
            }
            catch (Exception error)
            {
                Main.Log?.Log("AC's Homebrew: ModMenu settings unavailable - " + error);
            }
        }

        private static string Key(string name) => $"{Prefix}.{name}";

        private static LocalizedString L(string name, string en, string zh) => Common.L($"ACHomebrew.Settings.{name}", en, zh);

        private static void Apply(Action change)
        {
            change();
            Main.Settings.Save(Main.Entry);
            FeatBudget.SyncVisibility();
        }

        private static Toggle Toggle(string name, bool defaultValue, Func<bool> read, Action<bool> write, string en, string zh)
        {
            var key = Key(name);
            PushCurrentValues.Add(() => MenuApi.SetSetting(key, read()));
            return ModMenu.Settings.Toggle.New(key, defaultValue, L(name, en, zh)).OnValueChanged(value => Apply(() => write(value)));
        }

        private static SliderInt Slider(string name, int defaultValue, Func<int> read, Action<int> write, string en, string zh)
        {
            var key = Key(name);
            PushCurrentValues.Add(() => MenuApi.SetSetting(key, read()));
            return SliderInt.New(key, defaultValue, L(name, en, zh), 0, FeatBudgetRules.MaxLimit)
                .OnValueChanged(value => Apply(() => write(value)));
        }

        private static DropdownList Choice(string name, int defaultValue, Func<int> read, Action<int> write, string en, string zh,
            params (string en, string zh)[] values)
        {
            var key = Key(name);
            PushCurrentValues.Add(() => MenuApi.SetSetting(key, read()));
            var labels = new List<LocalizedString>();
            for (var i = 0; i < values.Length; i++) labels.Add(L($"{name}.{i}", values[i].en, values[i].zh));
            return DropdownList.New(key, defaultValue, L(name, en, zh), labels).OnValueChanged(value => Apply(() => write(value)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Register()
        {
            var s = Main.Settings;
            var d = new ModSettings();
            var builder = SettingsBuilder.New(Prefix, L("Title", Common.Text("Text.ModMenuSettings.Register.Text", "AC's Homebrew", false), Common.Text("Text.ModMenuSettings.Register.Text", "AC 的房规手册", true)))
                .SetMod(Main.Entry, false)
                .AddSubHeader(L("Budget", Common.Text("Text.ModMenuSettings.Register.Text.2", "Feat budget (applies immediately)", false), Common.Text("Text.ModMenuSettings.Register.Text.2", "专长额度（即时生效）", true)), true)
                .AddToggle(Toggle("budget-count", d.EnableFeatCountLimit, () => s.EnableFeatCountLimit, v => s.EnableFeatCountLimit = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.3", "Limit the number of Homebrew feats per character", false), Common.Text("Text.ModMenuSettings.Register.Text.3", "限制每个角色的房规专长数量", true)))
                .AddSliderInt(Slider("budget-count-max", d.MaxFeatCount, () => s.MaxFeatCount, v => s.MaxFeatCount = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.4", "Maximum feats", false), Common.Text("Text.ModMenuSettings.Register.Text.4", "专长数量上限", true)))
                .AddToggle(Toggle("budget-points", d.EnableFeatPointLimit, () => s.EnableFeatPointLimit, v => s.EnableFeatPointLimit = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.5", "Limit Homebrew feat points per character", false), Common.Text("Text.ModMenuSettings.Register.Text.5", "限制每个角色的房规专长点数", true)))
                .AddSliderInt(Slider("budget-points-max", d.MaxFeatPoints, () => s.MaxFeatPoints, v => s.MaxFeatPoints = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.6", "Maximum points", false), Common.Text("Text.ModMenuSettings.Register.Text.6", "点数上限", true)))
                .AddToggle(Toggle("solo-pets", d.TrulySoloCountsPets, () => s.TrulySoloCountsPets, v => s.TrulySoloCountsPets = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.7", "Truly Solo: pets in the party reduce the count", false), Common.Text("Text.ModMenuSettings.Register.Text.7", "真·独行：队伍中的宠物抵扣数量", true)))
                .AddSubHeader(L("Exclusion", Common.Text("Text.ModMenuSettings.Register.Text.8", "Exclusion groups (applies immediately)", false), Common.Text("Text.ModMenuSettings.Register.Text.8", "互斥组（即时生效）", true)))
                .AddToggle(Toggle("mutex", d.EnableMutex, () => s.EnableMutex, v => s.EnableMutex = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.9", "Enable exclusion groups (master switch)", false), Common.Text("Text.ModMenuSettings.Register.Text.9", "启用互斥组（总开关）", true)));

            foreach (var group in FeatGroupRules.Settings)
            {
                var current = group;
                builder
                    .AddToggle(Toggle($"group-{current.SettingId.ToLowerInvariant()}", current.DefaultEnabled,
                        () => FeatBudget.GroupLimit(current).Enabled,
                        v => FeatBudget.SetGroupLimit(current, v, FeatBudget.GroupLimit(current).Max),
                        current.NameEn, current.NameZh))
                    .AddSliderInt(Slider($"group-{current.SettingId.ToLowerInvariant()}-max", current.DefaultMax,
                        () => FeatBudget.GroupLimit(current).Max,
                        v => FeatBudget.SetGroupLimit(current, FeatBudget.GroupLimit(current).Enabled, v),
                        Common.Format("Text.ModMenuSettings.Register.Text.10", "{0}: limit", false, current.NameEn), Common.Format("Text.ModMenuSettings.Register.Text.10", "{0}：上限", true, current.NameZh)));
            }

            builder
                .AddSubHeader(L("Modes", Common.Text("Text.ModMenuSettings.Register.Text.11", "Damage and casting modes (applies immediately)", false), Common.Text("Text.ModMenuSettings.Register.Text.11", "伤害与施法模式（即时生效）", true)))
                .AddDropdownList(Choice("weapon-damage", (int)d.WeaponDamage, () => (int)s.WeaponDamage, v => s.WeaponDamage = (WeaponDamageMode)v,
                    Common.Text("Text.ModMenuSettings.Register.Text.12", "Weapon Damage mode", false), Common.Text("Text.ModMenuSettings.Register.Text.12", "武器伤害模式", true), (Common.Text("Text.ModMenuSettings.Register.Text.29", "Replace", false), Common.Text("Text.ModMenuSettings.Register.Text.29", "替换", true)), (Common.Text("Text.ModMenuSettings.Register.Text.30", "Add", false), Common.Text("Text.ModMenuSettings.Register.Text.30", "附加", true))))
                .AddDropdownList(Choice("casting-scope", (int)d.CastingScope, () => (int)s.CastingScope, v => s.CastingScope = (CastingAttributeScope)v,
                    Common.Text("Text.ModMenuSettings.Register.Text.13", "Casting attribute scope", false), Common.Text("Text.ModMenuSettings.Register.Text.13", "施法属性范围", true), (Common.Text("Text.ModMenuSettings.Register.Text.31", "Selected spellbook", false), Common.Text("Text.ModMenuSettings.Register.Text.31", "选定的法术书", true)), (Common.Text("Text.ModMenuSettings.Register.Text.32", "All spellbooks", false), Common.Text("Text.ModMenuSettings.Register.Text.32", "全部法术书", true))))
                .AddDropdownList(Choice("casting-mode", (int)d.CastingMode, () => (int)s.CastingMode, v => s.CastingMode = (CastingAttributeMode)v,
                    Common.Text("Text.ModMenuSettings.Register.Text.14", "Casting attribute mode", false), Common.Text("Text.ModMenuSettings.Register.Text.14", "施法属性模式", true), (Common.Text("Text.ModMenuSettings.Register.Text.33", "Always", false), Common.Text("Text.ModMenuSettings.Register.Text.33", "总是", true)), (Common.Text("Text.ModMenuSettings.Register.Text.34", "Only when higher", false), Common.Text("Text.ModMenuSettings.Register.Text.34", "仅在更高时", true))))
                .AddSubHeader(L("Restart", Common.Text("Text.ModMenuSettings.Register.Text.15", "Requires restarting the game", false), Common.Text("Text.ModMenuSettings.Register.Text.15", "需要重启游戏", true)))
                .AddDropdownList(Choice("power-level", (int)d.powerLevel, () => (int)s.powerLevel, v => s.powerLevel = (PowerLevel)v,
                    Common.Text("Text.ModMenuSettings.Register.Text.16", "Power level", false), Common.Text("Text.ModMenuSettings.Register.Text.16", "强度等级", true), (Common.Text("Text.ModMenuSettings.Register.Text.35", "Balanced", false), Common.Text("Text.ModMenuSettings.Register.Text.35", "平衡", true)), (Common.Text("Text.ModMenuSettings.Register.Text.36", "Legacy all full", false), Common.Text("Text.ModMenuSettings.Register.Text.36", "旧版全额", true))))
                .AddToggle(Toggle("self-stack", d.IncludeSelfInAttributeStack, () => s.IncludeSelfInAttributeStack, v => s.IncludeSelfInAttributeStack = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.17", "Include self in attribute stack", false), Common.Text("Text.ModMenuSettings.Register.Text.17", "属性自我叠加", true)))
                .AddToggle(Toggle("attributes", d.EnableAttributes, () => s.EnableAttributes, v => s.EnableAttributes = v, Common.Text("Text.ModMenuSettings.Register.Text.18", "Enable attribute bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.18", "启用属性加成", true)))
                .AddToggle(Toggle("defenses", d.EnableDefenses, () => s.EnableDefenses, v => s.EnableDefenses = v, Common.Text("Text.ModMenuSettings.Register.Text.19", "Enable defense bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.19", "启用防御加成", true)))
                .AddToggle(Toggle("maneuvers", d.EnableManeuvers, () => s.EnableManeuvers, v => s.EnableManeuvers = v, Common.Text("Text.ModMenuSettings.Register.Text.20", "Enable maneuver bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.20", "启用战技加成", true)))
                .AddToggle(Toggle("checks", d.EnableChecks, () => s.EnableChecks, v => s.EnableChecks = v, Common.Text("Text.ModMenuSettings.Register.Text.21", "Enable check bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.21", "启用检定加成", true)))
                .AddToggle(Toggle("skills", d.EnableSkills, () => s.EnableSkills, v => s.EnableSkills = v, Common.Text("Text.ModMenuSettings.Register.Text.22", "Enable skill bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.22", "启用技能加成", true)))
                .AddToggle(Toggle("caster-dc", d.EnableCasterDC, () => s.EnableCasterDC, v => s.EnableCasterDC = v, Common.Text("Text.ModMenuSettings.Register.Text.23", "Enable spell DC bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.23", "启用法术DC加成", true)))
                .AddToggle(Toggle("caster-level", d.EnableCasterLevel, () => s.EnableCasterLevel, v => s.EnableCasterLevel = v, Common.Text("Text.ModMenuSettings.Register.Text.24", "Enable caster level bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.24", "启用施法者等级加成", true)))
                .AddToggle(Toggle("spell-penetration", d.EnableSpellPenetration, () => s.EnableSpellPenetration, v => s.EnableSpellPenetration = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.25", "Enable spell penetration bonuses", false), Common.Text("Text.ModMenuSettings.Register.Text.25", "启用法术穿透加成", true)))
                .AddToggle(Toggle("meme", d.EnableMemeFeats, () => s.EnableMemeFeats, v => s.EnableMemeFeats = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.26", "Enable meme feats", false), Common.Text("Text.ModMenuSettings.Register.Text.26", "启用梗专长", true)))
                .AddToggle(Toggle("bab", d.EnableBAB, () => s.EnableBAB, v => s.EnableBAB = v, Common.Text("Text.ModMenuSettings.Register.Text.27", "Enable BAB bonuses (power option)", false), Common.Text("Text.ModMenuSettings.Register.Text.27", "启用BAB加成（强力选项）", true)))
                .AddToggle(Toggle("power-mode", d.EnablePowerMode, () => s.EnablePowerMode, v => s.EnablePowerMode = v,
                    Common.Text("Text.ModMenuSettings.Register.Text.28", "Enable Power Mode (power option)", false), Common.Text("Text.ModMenuSettings.Register.Text.28", "启用威力模式（强力选项）", true)));

            MenuApi.AddSettings(builder);
            // The settings file stays authoritative; show its values in the menu.
            foreach (var push in PushCurrentValues) push();
        }
    }
}
#else
namespace ACHomebrew
{
    /// <summary>Built without ModMenu installed: the UMM settings panel is the only settings UI.</summary>
    internal static class ModMenuSettings
    {
        public static void TryRegister() { }
    }
}
#endif
