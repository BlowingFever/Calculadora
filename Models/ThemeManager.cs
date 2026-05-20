using System;
using System.Linq;
using System.Windows;
using Wpf.Ui.Appearance;

namespace Calculadora.Models
{
    /// <summary>
    /// Application-wide theme controller.
    /// Applies the selected WPF UI theme and merges the corresponding colour sub-dictionary
    /// from <c>Themes/Colors.xaml</c> into the application resource tree.
    /// </summary>
    public static class ThemeManager
    {
        private const string DarkKey = "Dark";
        private const string LightKey = "Light";

        /// <summary>Gets a value indicating whether the dark theme is currently active.</summary>
        public static bool IsDark { get; private set; } = true;

        /// <summary>Raised after each successful theme switch.</summary>
        public static event EventHandler? ThemeChanged;

        /// <summary>
        /// Initializes the theme system and applies the startup theme.
        /// </summary>
        /// <param name="startDark">
        /// <c>true</c> (default) to start in dark mode; <c>false</c> for light mode.
        /// </param>
        public static void Initialize(bool startDark = true) => Apply(startDark);

        /// <summary>Toggles between dark and light themes.</summary>
        public static void Toggle() => Apply(!IsDark);

        /// <summary>
        /// Applies the specified theme immediately.
        /// </summary>
        /// <param name="dark"><c>true</c> to apply dark theme; <c>false</c> for light.</param>
        public static void Apply(bool dark)
        {
            IsDark = dark;
            ApplicationThemeManager.Apply(dark ? ApplicationTheme.Dark : ApplicationTheme.Light);
            MergeColorSubDictionary(dark ? DarkKey : LightKey);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Locates the <c>Colors.xaml</c> dictionary (loading it if absent), then copies all
        /// entries from the requested colour sub-dictionary into the application root resources,
        /// removing any entries belonging to the opposite theme first.
        /// </summary>
        /// <param name="key">Resource key of the sub-dictionary to activate (<c>"Dark"</c> or <c>"Light"</c>).</param>
        private static void MergeColorSubDictionary(string key)
        {
            ResourceDictionary? colorDict = FindColorsDictionary();
            if (colorDict is null)
            {
                colorDict = new ResourceDictionary
                {
                    Source = new Uri("Themes/Colors.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(colorDict);
            }

            if (colorDict[key] is not ResourceDictionary newDict) return;

            var appRes = Application.Current.Resources;
            string oppositeKey = key == DarkKey ? LightKey : DarkKey;

            if (colorDict[oppositeKey] is ResourceDictionary oldDict)
            {
                foreach (var k in oldDict.Keys.Cast<object>().ToList())
                    if (appRes.Contains(k))
                        appRes.Remove(k);
            }

            foreach (var k in newDict.Keys.Cast<object>().ToList())
                appRes[k] = newDict[k];
        }

        /// <summary>
        /// Searches the merged dictionaries for the <c>Colors.xaml</c> resource dictionary.
        /// </summary>
        /// <returns>The dictionary if found; otherwise <c>null</c>.</returns>
        private static ResourceDictionary? FindColorsDictionary()
        {
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
                if (dict.Source?.ToString().EndsWith("Colors.xaml", StringComparison.OrdinalIgnoreCase) == true)
                    return dict;
            return null;
        }
    }
}