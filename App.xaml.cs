using System.Windows;
using Calculadora.Models;

namespace Calculadora
{
    /// <summary>
    /// Application entry point.
    /// Initialises the theme system and launches the main window.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called by the WPF framework before the first window is shown.
        /// Applies the default dark theme and creates <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Initialize(startDark: true);
            new MainWindow().Show();
        }
    }
}