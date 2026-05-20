using System.Windows;
using Calculadora.Models;

namespace Calculadora
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.Initialize(startDark: true);

            var window = new MainWindow();
            window.Show();
        }
    }
}