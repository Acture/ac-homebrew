using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ACHomebrew.Feats;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem;
using UnityEngine;
using UnityModManagerNet;

namespace ACHomebrew
{
    public static class Main
    {
        internal static Harmony HarmonyInstance;
        internal static UnityModManager.ModEntry.ModLogger Log;
        internal static UnityModManager.ModEntry Entry;
        internal static ModSettings Settings;

        public static bool Enabled = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            // A loaded legacy copy keeps its Harmony patches until the process exits.
            var legacy = UnityModManager.FindMod("AttributeFeats");
            if (legacy != null && (legacy.Enabled || legacy.Started))
            {
                modEntry.Logger.Error("Disable the old AttributeFeats installation in Unity Mod Manager, then restart before enabling AC's Homebrew.");
                return false;
            }

            Entry = modEntry;
            Log = modEntry.Logger;
            Settings = ModSettings.LoadCompatible(modEntry);
            Mod.Entry = Entry;
            Mod.Log = Log;
            Mod.Settings = Settings;
            FeatTextCatalog.Initialize(modEntry.Path, message => Log.Log(message));

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;

            HarmonyInstance = new Harmony(modEntry.Info.Id);
            // Feature projects ship merged into this assembly; Distinct keeps separate builds working too.
            foreach (var assembly in new[]
            {
                Assembly.GetExecutingAssembly(), typeof(Mod).Assembly, typeof(MainAbilityToEverything_Feats).Assembly,
                typeof(SpecializedFeats).Assembly, typeof(StanceFeats).Assembly, typeof(ReactiveArmorFeats).Assembly,
                typeof(SpellTagFeats).Assembly, typeof(GreaterSummoningFeats).Assembly, typeof(StealthFeats).Assembly,
            }.Distinct())
            {
                HarmonyInstance.PatchAll(assembly);
            }

            Log.Log("AC's Homebrew loaded.");
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            Log.Log(value ? "AC's Homebrew enabled." : "AC's Homebrew disabled (restart required to fully unload).");
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var s = Settings;
            var changed = false;

            GUILayout.Label("<color=cyan><b>[ Global Scaling Configuration ]</b></color>");
            GUILayout.Label("Weapon Damage Mode, casting settings, maneuver settings, Feat Budget and exclusion groups apply immediately. Other settings require restarting.");

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Power Level: <color=yellow>{s.powerLevel}</color>", GUILayout.Width(260));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cycle Power Level", GUILayout.Width(140)))
            {
                s.powerLevel = s.powerLevel == PowerLevel.Balanced ? PowerLevel.Legacy_AllFull : PowerLevel.Balanced;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"<color=grey><size=11>{GetPowerLevelDescription(s.powerLevel)}</size></color>");
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("<color=green><b>[ Build Enablers ]</b></color>");
            GUILayout.BeginVertical("box");
            GUILayout.Label("<color=green><size=11>These defaults keep Main feats focused on build-enabling stats rather than raw power.</size></color>");
            changed |= ToggleSetting(ref s.EnableAttributes, "Enable Attributes — adds to the six ability scores (default: ON)");
            changed |= ToggleSetting(ref s.EnableDefenses, "Enable Defenses — AC, CMD, saves, and initiative (default: ON)");
            changed |= ToggleSetting(ref s.EnableManeuvers, "Enable Maneuvers — Combat Maneuver Bonus only (default: ON)");
            changed |= ToggleSetting(ref s.EnableChecks, "Enable Checks — Bluff, Diplomacy, and Intimidate checks (default: ON)");
            changed |= ToggleSetting(ref s.EnableSkills, "Enable Skills — all skill bonuses (default: ON)");
            changed |= ToggleSetting(ref s.EnableCasterDC, "Enable Caster DC — spell save DC scaling (default: ON)");
            changed |= ToggleSetting(ref s.EnableCasterLevel, "Enable Caster Level — caster level scaling (default: ON)");
            changed |= ToggleSetting(ref s.EnableSpellPenetration, "Enable Spell Penetration — bonus vs. spell resistance (default: ON)");
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("<color=orange><b>[ Stacking Options ]</b></color>");
            GUILayout.BeginVertical("box");
            changed |= ToggleSetting(ref s.IncludeSelfInAttributeStack, "Include Self in Attribute Stack — a Main feat may add its chosen attribute to itself (default: OFF)");
            GUILayout.Label("<color=grey><size=11>Leave this off for the redesign baseline. Turning it on restores recursive self-stacking behavior.</size></color>");
            var mutexChanged = ToggleSetting(ref s.EnableMutex, "Enable Mutual Exclusivity — master switch for the exclusion groups below (default: ON)");
            GUILayout.Label("<color=grey><size=11>Applies immediately. Each group limits how many of its feats one character may own; switch groups and limits individually below. When OFF, no group is enforced.</size></color>");
            if (s.EnableMutex)
            {
                foreach (var group in FeatGroupRules.Settings)
                {
                    var limit = FeatBudget.GroupLimit(group);
                    var enabled = limit.Enabled;
                    var max = limit.Max;
                    GUILayout.BeginHorizontal();
                    enabled = GUILayout.Toggle(enabled, $"{group.NameEn} (default: {(group.DefaultEnabled ? "ON" : "OFF")}, {group.DefaultMax})", GUILayout.Width(420));
                    GUILayout.Label($"max <color=yellow>{max}</color>", GUILayout.Width(60));
                    if (GUILayout.Button("-", GUILayout.Width(30))) max--;
                    if (GUILayout.Button("+", GUILayout.Width(30))) max++;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    max = FeatBudgetRules.ClampLimit(max);
                    if (enabled == limit.Enabled && max == limit.Max) continue;
                    FeatBudget.SetGroupLimit(group, enabled, max);
                    mutexChanged = true;
                }
            }
            if (mutexChanged)
            {
                FeatBudget.SyncVisibility();
                changed = true;
            }
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("<color=green><b>[ Weapon Damage Feats ] — Applies Immediately</b></color>");
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Weapon Damage Mode: <color=yellow>{s.WeaponDamage}</color>", GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Switch Mode", GUILayout.Width(140)))
            {
                s.WeaponDamage = s.WeaponDamage == WeaponDamageMode.Replace ? WeaponDamageMode.Add : WeaponDamageMode.Replace;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(s.WeaponDamage == WeaponDamageMode.Replace
                ? "Replace: use the chosen attribute for weapon damage when better, preserving normal damage multipliers."
                : "Add: keep normal weapon damage and add the chosen positive attribute modifier once.");
            GUILayout.Label("<color=grey><size=11>Applies only to the new Weapon Damage feats, for their chosen weapon category. Requires a weapon that already applies an attribute to damage. Independent of Power Mode. Mode changes apply on the next damage calculation.</size></color>");
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("<color=green><b>[ Casting Attribute Feats ] — Applies Immediately</b></color>");
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Casting Scope: <color=yellow>{s.CastingScope}</color>", GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Switch Scope", GUILayout.Width(140)))
            {
                s.CastingScope = s.CastingScope == CastingAttributeScope.SelectedSpellbook ? CastingAttributeScope.AllSpellbooks : CastingAttributeScope.SelectedSpellbook;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Casting Mode: <color=yellow>{s.CastingMode}</color>", GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Switch Mode", GUILayout.Width(140)))
            {
                s.CastingMode = s.CastingMode == CastingAttributeMode.Always ? CastingAttributeMode.IfHigher : CastingAttributeMode.Always;
                changed = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("<color=grey><size=11>SelectedSpellbook (default): each Strength/Dexterity/Constitution Spellcasting feat changes the spellbook chosen when it was taken. AllSpellbooks: it changes every spellbook. Always (default): the feat's attribute replaces the spellbook's. IfHigher: the higher of the two is used. The spellbook UI header may still show the original attribute.</size></color>");
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label("<color=green><b>[ Feat Budget ] — Applies Immediately</b></color>");
            GUILayout.BeginVertical("box");
            GUILayout.Label("<color=grey><size=11>Limits the Homebrew feats each character can take, across all families. Only chosen feats count; opening the AC's Homebrew menu or a family menu is free, and vanilla or other mods' feats are never counted. Both limits can be enabled together. Existing feats are never removed: a character above a lowered limit keeps them, but cannot choose more until the limit is raised or the character is respecced. Exclusion groups still apply separately.</size></color>");
            var budgetChanged = ToggleSetting(ref s.EnableFeatCountLimit, "Limit the number of Homebrew feats per character (default: OFF)");
            budgetChanged |= LimitStepper("Maximum feats", ref s.MaxFeatCount);
            budgetChanged |= ToggleSetting(ref s.EnableFeatPointLimit, "Limit Homebrew feat points per character (default: OFF)");
            budgetChanged |= LimitStepper("Maximum points", ref s.MaxFeatPoints);
            budgetChanged |= ToggleSetting(ref s.TrulySoloCountsPets, "Truly Solo: pets in the party reduce the number of absent companions (default: OFF)");
            changed |= ToggleSetting(ref s.EnableMemeFeats, "Enable meme feats — joke feats in their own menu, free of budget points; requires restarting (default: OFF)");
            GUILayout.Label($"<color=grey><size=11>Point costs — {DescribeCosts()}. Each rank and each Weapon Damage weapon category counts as a separate feat.</size></color>");
            ShowPartyBudget();
            GUILayout.EndVertical();
            if (budgetChanged)
            {
                FeatBudget.SyncVisibility();
                changed = true;
            }

            GUILayout.Space(10);
            GUILayout.Label(Common.CurrentText("Text.ManeuverSettings.Title", "Maneuver Tactics"));
            GUILayout.BeginVertical("box");
            changed |= LimitStepper(Common.CurrentText("Text.ManeuverSettings.FreePerRound", "Free maneuvers per round"), ref s.ManeuverFreePerRound);
            changed |= LimitStepper(Common.CurrentText("Text.ManeuverSettings.Burst", "Extra free uses per encounter"), ref s.ManeuverBurstPerCombat);
            changed |= LimitStepper(Common.CurrentText("Text.ManeuverSettings.Cap", "Maximum free maneuvers per round"), ref s.ManeuverFreePerRoundCap);
            var durationMultiplier = (int)s.ManeuverDurationMultiplier;
            if (LimitStepper(Common.CurrentText("Text.ManeuverSettings.Multiplier", "Maneuver duration multiplier"), ref durationMultiplier, 1, 10))
            {
                s.ManeuverDurationMultiplier = durationMultiplier;
                changed = true;
            }
            changed |= LimitStepper(Common.CurrentText("Text.ManeuverSettings.ExtraRounds", "Minimum extra duration (rounds)"), ref s.ManeuverMinimumExtraRounds);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.Label("<color=red><b>[ Power Mode ]</b></color>");
            GUILayout.BeginVertical("box");
            GUILayout.Label("<color=red><size=11>Warning: these settings materially increase combat power and are intentionally off by default.</size></color>");
            changed |= ToggleSetting(ref s.EnableBAB, "Enable BAB — adds reduced scaling to Base Attack Bonus (default: OFF)");
            changed |= ToggleSetting(ref s.EnablePowerMode, "Enable Power Mode — enables attack bonus, damage, AoOs, sneak attack, HP, speed, and fixed +1 Reach (default: OFF)");
            GUILayout.EndVertical();

            if (changed)
            {
                Settings.Save(modEntry);
            }
        }

        private static bool ToggleSetting(ref bool value, string label)
        {
            var newValue = GUILayout.Toggle(value, label);
            if (newValue == value) return false;
            value = newValue;
            return true;
        }

        private static bool LimitStepper(string label, ref int value, int minimum = 0, int maximum = FeatBudgetRules.MaxLimit)
        {
            var old = value;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: <color=yellow>{value}</color>", GUILayout.Width(200));
            if (GUILayout.Button("-", GUILayout.Width(30))) value--;
            if (GUILayout.Button("+", GUILayout.Width(30))) value++;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            value = Math.Max(minimum, Math.Min(maximum, value));
            return value != old;
        }

        private static string DescribeCosts()
            => string.Join("; ", FeatBudgetRules.Costs
                .GroupBy(entry => entry.Value)
                .OrderByDescending(group => group.Key)
                .Select(group => $"{group.Key}: " + string.Join(", ", group.Select(entry =>
                    entry.Key == "Root" ? "Long-Reach Gambit" : Regex.Replace(entry.Key, "(?<=[a-z])(?=[A-Z])", " ")))));

        private static void ShowPartyBudget()
        {
            var limits = FeatBudget.Limits;
            if (!limits.AnyEnabled) return;
            try
            {
                var party = Game.Instance?.Player?.PartyAndPets;
                if (party == null || party.Count == 0) return;
                GUILayout.Label("<b>Current party</b>");
                foreach (var unit in party)
                {
                    var verdict = FeatBudgetRules.Evaluate(limits, FeatBudget.Usage(unit.Descriptor), 0);
                    var usage = string.Join(", ", new[]
                    {
                        limits.CountEnabled ? $"feats {verdict.Usage.Count}/{limits.MaxCount}" : null,
                        limits.PointsEnabled ? $"points {verdict.Usage.Points}/{limits.MaxPoints}" : null,
                    }.Where(part => part != null));
                    GUILayout.Label($"{unit.CharacterName}: {usage}" +
                        (verdict.OverBudget ? " <color=orange>(over budget: feats kept, no new choices)</color>" : ""));
                }
            }
            catch (Exception)
            {
                // No loaded game (main menu or loading screen).
            }
        }

        private static string GetPowerLevelDescription(PowerLevel level) => level switch
        {
            PowerLevel.Balanced => "Balanced: full scaling for attributes, defenses, maneuvers, skills, caster level, and spell penetration; reduced scaling for spell DC, BAB, and Power Mode bonuses.",
            PowerLevel.Legacy_AllFull => "Legacy_AllFull: every rank-based Main feat bonus uses full modifier scaling, matching the old all-full behavior.",
            _ => level.ToString(),
        };

        [HarmonyPatch(typeof(BlueprintsCache))]
        private static class BlueprintsCache_Patch
        {
            private static bool Initialized;

            [HarmonyPriority(Priority.Last)]
            [HarmonyPatch(nameof(BlueprintsCache.Init))]
            [HarmonyPostfix]
            private static void Init_Postfix()
            {
                try
                {
                    if (Initialized) return;
                    Initialized = true;

                    if (!Enabled)
                    {
                        Log.Log("ACHomebrew: skipped (disabled).");
                        return;
                    }

                    Log.Log("ACHomebrew: patching blueprints...");
                    FeatRegistry.ConfigureAll();
                    Common.RefreshLocale();
                    ModMenuSettings.TryRegister();
                    ModTagSupport.TryRegister();
                    Log.Log("ACHomebrew: blueprint initialization attempt finished; see registration diagnostics above.");
                }
                catch (Exception e)
                {
                    Log.Log("ACHomebrew: failed to initialize.\n" + e);
                }
            }
        }
    }
}
