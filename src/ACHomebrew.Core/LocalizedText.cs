using System;
using System.Linq;
using Kingmaker.Localization.Shared;

namespace ACHomebrew.Feats
{
    /// <summary>
    /// One player-facing text in every game locale the mod ships. Built once at registration by composing
    /// catalog entries per locale, so a later language switch only selects a different stored value.
    /// </summary>
    internal sealed class LocalizedText
    {
        /// <summary>Supported game locales; English first, used for every unsupported locale.</summary>
        public static readonly Locale[] Locales =
        {
            Locale.enGB, Locale.zhCN, Locale.deDE, Locale.frFR, Locale.esES, Locale.itIT, Locale.ptBR, Locale.ruRU,
        };

        private readonly string[] values;

        private LocalizedText(string[] values) => this.values = values;

        public string this[Locale locale] => values[Index(locale)];

        public string English => values[0];

        public static int Index(Locale locale)
        {
            var index = Array.IndexOf(Locales, locale);
            return index < 0 ? 0 : index;
        }

        /// <summary>Runs <paramref name="compose"/> once per supported locale.</summary>
        public static LocalizedText Compose(Func<Locale, string> compose)
            => new(Locales.Select(compose).ToArray());

        /// <summary>A catalog entry in every locale; <paramref name="fallback"/> is the compiled English text.</summary>
        public static LocalizedText Text(string key, string fallback)
            => Compose(locale => Common.Text(key, fallback, locale));

        /// <summary>
        /// A catalog template formatted per locale. <see cref="LocalizedText"/> arguments are resolved in the
        /// same locale; any other argument is passed through unchanged.
        /// </summary>
        public static LocalizedText Format(string key, string fallback, params object[] arguments)
            => Compose(locale => Common.Format(key, fallback, locale, Resolve(arguments, locale)));

        /// <summary>The same string in every locale, for numbers and names that are never translated.</summary>
        public static LocalizedText Invariant(string value) => Compose(_ => value);

        public LocalizedText Map(Func<string, Locale, string> transform)
            => Compose(locale => transform(this[locale], locale));

        internal static object[] Resolve(object[] arguments, Locale locale)
            => arguments.Select(argument => argument is LocalizedText text ? text[locale] : argument).ToArray();

        public override string ToString() => English;
    }
}
