using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace ACHomebrew
{
    public enum PowerLevel
    {
        Balanced,
        Legacy_AllFull,
    }

    public enum CastingAttributeScope
    {
        SelectedSpellbook,
        AllSpellbooks,
    }

    public enum CastingAttributeMode
    {
        Always,
        IfHigher,
    }

    public class FeatGroupSetting
    {
        [XmlAttribute] public string Id;
        [XmlAttribute] public bool Enabled;
        [XmlAttribute] public int Max;
    }

    [XmlRoot("ACHomebrewSettings")]
    [XmlType("ACHomebrewSettings")]
    public class ModSettings : UnityModManager.ModSettings
    {
        private static readonly XmlSerializer CurrentSerializer = new XmlSerializer(typeof(ModSettings));
        private static readonly XmlSerializer LegacySerializer = new XmlSerializer(typeof(ModSettings), new XmlRootAttribute("AttributeFeatsSettings"));
        private bool createOnly;
        private string saveBlockReason;

        public bool IncludeSelfInAttributeStack = false;

        public bool EnableAttributes = true;
        public bool EnableDefenses = true;
        public bool EnableManeuvers = true;
        public bool EnableChecks = true;
        public bool EnableSkills = true;

        public bool EnableCasterDC = true;
        public bool EnableCasterLevel = true;
        public bool EnableSpellPenetration = true;

        public bool EnableBAB = false;
        public bool EnablePowerMode = false;

        public bool EnableMutex = true;

        public int ManeuverFreePerRound = 1;
        public int ManeuverBurstPerCombat = 3;
        public int ManeuverFreePerRoundCap = 2;
        public float ManeuverDurationMultiplier = 2f;
        public int ManeuverMinimumExtraRounds = 2;

        public WeaponDamageMode WeaponDamage = WeaponDamageMode.Replace;

        // Per-character AC's Homebrew budget. Both limits may be enabled together.
        public bool EnableFeatCountLimit = false;
        public int MaxFeatCount = 6;
        public bool EnableFeatPointLimit = false;
        public int MaxFeatPoints = 10;

        // Casting Attribute feats: which spellbooks they change, and whether only when higher.
        public CastingAttributeScope CastingScope = CastingAttributeScope.SelectedSpellbook;
        public CastingAttributeMode CastingMode = CastingAttributeMode.Always;

        // Truly Solo: whether pets in the party reduce the number of absent companions.
        public bool TrulySoloCountsPets = false;

        // Meme feats are registered only when enabled (requires restarting).
        public bool EnableMemeFeats = false;

        // Logs the measured distance on each close-quarters hit, for calibrating the distance scores.
        public bool LogDistanceCalibration = false;

        // Exclusion group overrides by group setting id. Missing groups use built-in defaults.
        public List<FeatGroupSetting> FeatGroups = new List<FeatGroupSetting>();

        public PowerLevel powerLevel = PowerLevel.Balanced;

        public static ModSettings LoadCompatible(UnityModManager.ModEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            var defaults = new ModSettings();
            string sourcePath = defaults.GetPath(entry);
            try
            {
                ModSettings loaded;
                if (TryRead(sourcePath, out loaded)) return loaded;

                // Only an absent new file permits importing the retired sibling installation.
                // Loading is read-only; the next normal Save persists the new XML identity.
                var parent = Directory.GetParent(Path.GetFullPath(entry.Path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (parent != null)
                {
                    sourcePath = Path.Combine(parent.FullName, "AttributeFeats", "Settings.xml");
                    if (TryRead(sourcePath, out loaded))
                    {
                        loaded.createOnly = true;
                        entry.Logger.Log("Imported settings from '" + sourcePath + "'; the original stays unchanged. The next save writes ACHomebrewSettings to '" + defaults.GetPath(entry) + "'.");
                        return loaded;
                    }
                }
                defaults.createOnly = true;
            }
            catch (Exception ex)
            {
                defaults.saveBlockReason = "Cannot read settings '" + sourcePath + "': " + ex.GetBaseException().Message
                    + " Automatic saving is disabled; repair or move that file and reload the mod.";
                entry.Logger.Error(defaults.saveBlockReason);
            }
            return defaults;
        }

        private static bool TryRead(string path, out ModSettings settings)
        {
            settings = null;
            FileStream stream;
            try { stream = File.OpenRead(path); }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
            using (stream)
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                reader.MoveToContent();
                if (reader.NamespaceURI != "" || (reader.LocalName != "ACHomebrewSettings" && reader.LocalName != "AttributeFeatsSettings"))
                    throw new InvalidDataException("Expected ACHomebrewSettings or a legacy AttributeFeatsSettings XML root; found '" + reader.Name + "'.");
                settings = (ModSettings)(reader.LocalName == "ACHomebrewSettings" ? CurrentSerializer : LegacySerializer).Deserialize(reader);
                if (settings == null) throw new InvalidDataException("The settings XML contains no settings object.");
                return true;
            }
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            if (saveBlockReason != null)
            {
                modEntry.Logger.Error(saveBlockReason);
                return;
            }
            string path = GetPath(modEntry);
            string temporaryPath = null;
            try
            {
                path = Path.GetFullPath(path);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    CurrentSerializer.Serialize(stream, this);

                // A migrated/default object must never replace a file that appeared after its load.
                // Successful serialization precedes either atomic operation, preserving old bytes on failure.
                if (createOnly || !File.Exists(path)) File.Move(temporaryPath, path);
                else File.Replace(temporaryPath, path, null);
                temporaryPath = null;
                createOnly = false;
            }
            catch (Exception ex)
            {
                modEntry.Logger.Error("Cannot save settings '" + path + "': " + ex.GetBaseException().Message + " Existing settings were not overwritten.");
            }
            finally
            {
                if (temporaryPath != null)
                {
                    try { File.Delete(temporaryPath); }
                    catch (Exception ex) { modEntry.Logger.Error("Cannot remove temporary settings '" + temporaryPath + "': " + ex.GetBaseException().Message); }
                }
            }
        }
    }
}
