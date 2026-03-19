using BoydScanQDBarcode.MVVM.Views;
using log4net;
using log4net.Config;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;

namespace BoydScanQDBarcode
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
        }
    }

}
