using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Calculadora.Models;

namespace Calculadora.Models
{
    public partial class ThemeToggleButton : UserControl
    {
        private bool _syncing;

        public ThemeToggleButton()
        {
            InitializeComponent();
            _syncing = true;
            // IsDark=true aligns with IsChecked=false (renders the moon icon for dark mode)
            Btn.IsChecked = !ThemeManager.IsDark;
            _syncing = false;
            ThemeManager.ThemeChanged += OnThemeChanged;
            Unloaded += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        private void Btn_Checked(object sender, RoutedEventArgs e)
        {
            // IsChecked=true indicates user requests Light Theme (sun icon)
            if (!_syncing)
                ThemeManager.Apply(dark: false);
        }

        private void Btn_Unchecked(object sender, RoutedEventArgs e)
        {
            // IsChecked=false indicates user requests Dark Theme (moon icon)
            if (!_syncing)
                ThemeManager.Apply(dark: true);
        }

        private void OnThemeChanged(object? sender, System.EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _syncing = true;
                Btn.IsChecked = !ThemeManager.IsDark;
                _syncing = false;
            });
        }
    }
}