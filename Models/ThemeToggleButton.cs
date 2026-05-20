using System.Windows;
using System.Windows.Controls;
using Calculadora.Models;

namespace Calculadora.Models
{
    /// <summary>
    /// Code-behind for the theme toggle button user control.
    /// Synchronises the toggle state with <see cref="ThemeManager"/> and animates the
    /// moon/sun icon swap defined in <c>ThemeToggleButton.xaml</c>.
    /// </summary>
    public partial class ThemeToggleButton : UserControl
    {
        /// <summary>
        /// Guards against recursive state updates when the toggle is programmatically checked.
        /// </summary>
        private bool _syncing;

        /// <summary>
        /// Initializes the control, aligns its initial checked state with the current theme,
        /// and subscribes to <see cref="ThemeManager.ThemeChanged"/>.
        /// </summary>
        public ThemeToggleButton()
        {
            InitializeComponent();

            _syncing = true;
            // IsDark == true maps to IsChecked == false (moon icon shown for dark mode).
            Btn.IsChecked = !ThemeManager.IsDark;
            _syncing = false;

            ThemeManager.ThemeChanged += OnThemeChanged;
            Unloaded += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;
        }

        /// <summary>
        /// Handles the toggle button <c>Checked</c> event.
        /// <c>IsChecked == true</c> means the user wants the light theme (sun icon).
        /// </summary>
        private void Btn_Checked(object sender, RoutedEventArgs e)
        {
            if (!_syncing)
                ThemeManager.Apply(dark: false);
        }

        /// <summary>
        /// Handles the toggle button <c>Unchecked</c> event.
        /// <c>IsChecked == false</c> means the user wants the dark theme (moon icon).
        /// </summary>
        private void Btn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_syncing)
                ThemeManager.Apply(dark: true);
        }

        /// <summary>
        /// Responds to external theme changes by updating the toggle state without
        /// triggering another <see cref="ThemeManager.Apply"/> call.
        /// </summary>
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