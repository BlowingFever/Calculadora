using System.Windows;
using Calculadora.Models;

namespace Calculadora
{
    /// <summary>
    /// WPF application entry point.
    /// Responsible for initialising the theme system and launching the main window.
    /// </summary>
    /// <remarks>
    /// Declared as <c>partial</c> because the XAML compiler generates the second half
    /// automatically from <c>App.xaml</c>.
    /// </remarks>
    public partial class App : Application
    {
        /// <summary>
        /// Called by the WPF framework before the first window is shown.
        /// Applies the default dark theme and creates <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="e">Startup event arguments provided by the framework.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Initialize(startDark: true);
            new MainWindow().Show();
        }
    }
}