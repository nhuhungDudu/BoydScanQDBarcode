using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BoydScanQDBarcode.MVVM.Views
{
    /// <summary>
    /// Interaction logic for WDMessageView.xaml
    /// </summary>
    public partial class WDMessageView : Window
    {
        private bool IsFlickering = false;
        public WDMessageView(string message, int timeout = 5000)
        {
            InitializeComponent();

            txtMessage.Text = message;

            System.Timers.Timer timer = new System.Timers.Timer(500); 
            timer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsFlickering)
                    {
                        borderMessage.Background = Brushes.Red;
                    }
                    else
                    {
                        borderMessage.Background = Brushes.Transparent;
                    }
                    IsFlickering = !IsFlickering;
                });
            };
            timer.Start();


            System.Timers.Timer closeTimer = new System.Timers.Timer(timeout); 
            closeTimer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    timer.Stop();
                    closeTimer.Stop();
                    this.Close();
                });
            };
            closeTimer.Start();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
