using System;
using System.Linq;
using System.Windows;
using Wpf.Ui.Appearance;

namespace Calculadora.Models
{
    /// <summary>
    /// Application-wide theme controller.
    /// Applies the selected theme (dark/light) to both the WPF UI library
    /// and the custom colour dictionary <c>Themes/Colors.xaml</c>.
    /// </summary>
    /// <remarks>
    /// This class is static and acts as an implicit singleton that can be called
    /// from anywhere in the application without dependency injection.
    ///
    /// Theme-change flow:
    /// <list type="number">
    ///   <item><description><see cref="Apply"/> or <see cref="Toggle"/> is called.</description></item>
    ///   <item><description>WPF UI is notified via <c>ApplicationThemeManager</c>.</description></item>
    ///   <item><description>The matching colour sub-dictionary is merged into application resources.</description></item>
    ///   <item><description><see cref="ThemeChanged"/> is raised so ViewModels can react.</description></item>
    /// </list>
    /// </remarks>
    public static class ThemeManager
    {
        private const string DarkKey = "Dark";
        private const string LightKey = "Light";

        /// <summary>
        /// Gets a value indicating whether the dark theme is currently active.
        /// </summary>
        public static bool IsDark { get; private set; } = true;

        /// <summary>
        /// Raised after each successful theme switch.
        /// ViewModels can subscribe to update icons or other theme-dependent properties.
        /// </summary>
        public static event EventHandler? ThemeChanged;

        /// <summary>
        /// Initialises the theme system and applies the startup theme.
        /// Should be called once in <c>App.OnStartup</c>, before any window is shown.
        /// </summary>
        /// <param name="startDark">
        /// <c>true</c> (default) to start in dark mode; <c>false</c> for light mode.
        /// </param>
        public static void Initialize(bool startDark = true) => Apply(startDark);

        /// <summary>
        /// Toggles between dark and light themes.
        /// If the current theme is dark it switches to light, and vice-versa.
        /// </summary>
        public static void Toggle() => Apply(!IsDark);

        /// <summary>
        /// Applies the specified theme immediately.
        /// </summary>
        /// <param name="dark"><c>true</c> for the dark theme; <c>false</c> for light.</param>
        public static void Apply(bool dark)
        {
            IsDark = dark;
            ApplicationThemeManager.Apply(dark ? ApplicationTheme.Dark : ApplicationTheme.Light);
            MergeColorSubDictionary(dark ? DarkKey : LightKey);
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Locates the <c>Colors.xaml</c> dictionary (loading it if absent), removes entries
        /// belonging to the opposite theme, then merges the requested colour sub-dictionary
        /// into the application root resources.
        /// </summary>
        /// <param name="key">
        /// Resource key of the sub-dictionary to activate: <c>"Dark"</c> or <c>"Light"</c>.
        /// </param>
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

            // Remove entries from the previous theme first to avoid key conflicts.
            if (colorDict[oppositeKey] is ResourceDictionary oldDict)
            {
                foreach (var k in oldDict.Keys.Cast<object>().ToList())
                    if (appRes.Contains(k))
                        appRes.Remove(k);
            }

            // Merge entries from the new theme.
            foreach (var k in newDict.Keys.Cast<object>().ToList())
                appRes[k] = newDict[k];
        }

        /// <summary>
        /// Searches the application's merged dictionaries for <c>Colors.xaml</c>.
        /// </summary>
        /// <returns>
        /// The <see cref="ResourceDictionary"/> if found; otherwise <c>null</c>.
        /// </returns>
        private static ResourceDictionary? FindColorsDictionary()
        {
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
                if (dict.Source?.ToString().EndsWith("Colors.xaml", StringComparison.OrdinalIgnoreCase) == true)
                    return dict;
            return null;
        }
    }
}