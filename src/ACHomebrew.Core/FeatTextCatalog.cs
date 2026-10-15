using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace ACHomebrew.Feats
{
    internal static class FeatTextCatalog
    {
        private static Dictionary<string, string> English;
        private static Dictionary<string, string> Chinese;

        public static void Initialize(string modDirectory, Action<string> log)
        {
            var assembly = Assembly.GetExecutingAssembly();
            English = ReadEmbedded(assembly, "AttributeFeats.Localization.FeatText.json", log);
            Chinese = ReadEmbedded(assembly, "AttributeFeats.Localization.zhCN.json", log);
            if (string.IsNullOrEmpty(modDirectory)) return;
            var directory = Path.Combine(modDirectory, "Localization");
            Overlay(English, Path.Combine(directory, "enGB.json"), log);
            Overlay(Chinese, Path.Combine(directory, "zhCN.json"), log);
        }

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

        private static void Overlay(Dictionary<string, string> embedded, string path, Action<string> log)
        {
            if (!File.Exists(path))
            {
                log?.Invoke("AC's Homebrew: localization file missing: " + path + "; using embedded text.");
                return;
            }
            try
            {
                var external = Read(File.ReadAllText(path));
                var missing = embedded.Keys.Count(key => !external.ContainsKey(key));
                if (missing > 0)
                    log?.Invoke($"AC's Homebrew: {path} is missing {missing} translations; using embedded text for those keys.");
                foreach (var entry in external)
                {
                    if (!embedded.TryGetValue(entry.Key, out var original))
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

        public static string Get(string key, string fallback, bool chinese)
        {
            if (English == null) Initialize(null, message => Mod.Log?.Log(message));
            English.TryGetValue(key, out var english);
            Chinese.TryGetValue(key, out var translation);
            return SelectText(english, translation, fallback, chinese);
        }

        internal static string SelectText(string english, string chinese, string fallback, bool useChinese)
            => useChinese && !string.IsNullOrWhiteSpace(chinese)
                ? chinese
                : !string.IsNullOrWhiteSpace(english) ? english : fallback;
    }
}
