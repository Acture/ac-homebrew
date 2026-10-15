using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;

namespace ACHomebrew.Feats
{
    internal static class FeatTextCatalog
    {
        // Locales indexed like LocalizedText.Locales; English (index 0) is the fallback for every other locale.
        private static Dictionary<string, string>[] catalogs;

        public static void Initialize(string modDirectory, Action<string> log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var loaded = new Dictionary<string, string>[LocalizedText.Locales.Length];
            for (var i = 0; i < loaded.Length; i++)
                loaded[i] = ReadEmbedded(assembly, ResourceName(LocalizedText.Locales[i]), log);
            if (!string.IsNullOrEmpty(modDirectory))
            {
                var directory = Path.Combine(modDirectory, "Localization");
                for (var i = 0; i < loaded.Length; i++)
                    Overlay(loaded[i], loaded[0], Path.Combine(directory, LocalizedText.Locales[i] + ".json"), log);
            }
            catalogs = loaded;
        }

        // English keeps its historical resource name so older tools that read it keep working.
        internal static string ResourceName(Locale locale)
            => locale == Locale.enGB ? "AttributeFeats.Localization.FeatText.json" : $"AttributeFeats.Localization.{locale}.json";

        private static Dictionary<string, string> ReadEmbedded(Assembly assembly, string name, Action<string> log)
        {
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
            {
                log?.Invoke("AC's Homebrew: embedded localization missing: " + name + "; using compiled text fallbacks.");
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }
            using var reader = new StreamReader(stream);
            return Read(reader.ReadToEnd());
        }

        private static Dictionary<string, string> Read(string json)
        {
            using var input = new StringReader(json);
            using var reader = new JsonTextReader(input) { DateParseHandling = DateParseHandling.None };
            if (!reader.Read() || reader.TokenType != JsonToken.StartObject)
                throw new InvalidDataException("Language file must be a JSON object of text keys.");
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType != JsonToken.PropertyName)
                    throw new InvalidDataException("Expected a localization key.");
                var key = (string)reader.Value;
                if (string.IsNullOrWhiteSpace(key) || result.ContainsKey(key)
                    || !reader.Read() || reader.TokenType != JsonToken.String)
                    throw new InvalidDataException("Keys must be unique and nonempty, and values must be strings: " + key);
                result.Add(key, (string)reader.Value);
            }
            if (reader.TokenType != JsonToken.EndObject || reader.Read())
                throw new InvalidDataException("Incomplete language object or trailing content.");
            return result;
        }

        private static void Overlay(Dictionary<string, string> embedded, Dictionary<string, string> english, string path, Action<string> log)
        {
            if (!File.Exists(path))
            {
                log?.Invoke("AC's Homebrew: localization file missing: " + path + "; using embedded text.");
                return;
            }
            try
            {
                var external = Read(File.ReadAllText(path));
                var missing = english.Keys.Count(key => !external.ContainsKey(key));
                if (missing > 0)
                    log?.Invoke($"AC's Homebrew: {path} is missing {missing} translations; using embedded text for those keys.");
                foreach (var entry in external)
                {
                    if (!english.TryGetValue(entry.Key, out var original))
                    {
                        log?.Invoke($"AC's Homebrew: unknown localization key {entry.Key} in {path}; ignored.");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(entry.Value) || !TextFormat.Compatible(original, entry.Value))
                    {
                        log?.Invoke($"AC's Homebrew: invalid translation {entry.Key} in {path}; using embedded text (check placeholders, markup and links).");
                        continue;
                    }
                    embedded[entry.Key] = entry.Value;
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException
                || error is JsonException || error is InvalidDataException)
            {
                log?.Invoke("AC's Homebrew: cannot load localization " + path + ": " + error.Message + "; using embedded text.");
            }
        }

        /// <summary>Text in <paramref name="locale"/>, then English, then <paramref name="fallback"/>.</summary>
        public static string Get(string key, string fallback, Locale locale)
            => Find(key, locale) ?? Find(key, Locale.enGB) ?? fallback;

        /// <summary>The nonblank entry for <paramref name="key"/> in exactly <paramref name="locale"/>, or null.</summary>
        public static string Find(string key, Locale locale)
        {
            if (catalogs == null) Initialize(null, message => Mod.Log?.Log(message));
            return catalogs[LocalizedText.Index(locale)].TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text)
                ? text : null;
        }
    }
}
