using CommunityToolkit.Mvvm.ComponentModel;
using BoydScanQDBarcode.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BoydScanQDBarcode.ViewModels
{
    public partial class HeaderVM : ViewModelBase
    {

        [ObservableProperty] private string _appVersion;
        [ObservableProperty] private string _timeNow;
        [ObservableProperty] private string _dateNow;

        public HeaderVM()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            AppVersion = $"Version: {version}";

            System.Timers.Timer timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            DateNow = dtNow.ToString("yyyy-MM-dd");
            TimeNow = dtNow.ToString("HH:mm:ss");

            //Debug.WriteLine(TimeNow);
        }
    }
}
