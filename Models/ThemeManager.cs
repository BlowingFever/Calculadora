using System;
using System.Linq;
using System.Windows;
using Wpf.Ui.Appearance;

namespace Calculadora.Models   // ← namespace correcto según carpeta
{
    public static class ThemeManager
    {
        private const string DarkKey = "Dark";
        private const string LightKey = "Light";

        public static bool IsDark { get; private set; } = true;

        public static event EventHandler? ThemeChanged;

        public static void Initialize(bool startDark = true) => Apply(startDark);

        public static void Toggle() => Apply(!IsDark);

        public static void Apply(bool dark)
        {
            IsDark = dark;

            ApplicationThemeManager.Apply(
                dark ? ApplicationTheme.Dark : ApplicationTheme.Light);

            MergeColorSubDictionary(dark ? DarkKey : LightKey);

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void MergeColorSubDictionary(string key)
        {
            ResourceDictionary? colorDict = FindColorsDictionary();
            if (colorDict is null)
            {
                var uri = new Uri("Themes/Colors.xaml", UriKind.Relative);
                colorDict = new ResourceDictionary { Source = uri };
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

        private static ResourceDictionary? FindColorsDictionary()
        {
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
                if (dict.Source?.ToString().EndsWith("Colors.xaml",
                        StringComparison.OrdinalIgnoreCase) == true)
                    return dict;
            return null;
        }
    }
}