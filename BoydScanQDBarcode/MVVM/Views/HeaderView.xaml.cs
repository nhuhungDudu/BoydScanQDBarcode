using BoydScanQDBarcode.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BoydScanQDBarcode.MVVM.Views
{
    /// <summary>
    /// Interaction logic for HeaderView.xaml
    /// </summary>
    public partial class HeaderView : UserControl
    {
        private readonly Counter counter = new Counter();
        private readonly PerformanceCounter cpuCounter;
        private readonly PerformanceCounter memAvailableCounter;
        private readonly ulong memTotal = 0;
        private readonly DispatcherTimer timer;
        private const int KB = 1024;
        private const int MB = KB * 1024;
        private const int GB = MB * 1024;

        public HeaderView()
        {
            InitializeComponent();
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memAvailableCounter = new PerformanceCounter("Memory", "Available Bytes");

            using (var memMC = new ManagementClass("Win32_PhysicalMemory"))
            {
                using var memMOC = memMC.GetInstances();
                foreach (var memMO in memMOC)
                {
                    memTotal += (ulong)memMO.GetPropertyValue("Capacity");
                }
            }

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1),
                IsEnabled = true
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCpuUsage();
            UpdateMemoryUsage();
            UpdateDiskUsage();
        }

        private void UpdateCpuUsage()
        {
            float cpuUsage = cpuCounter.NextValue();
            CpuUsageText.Text = $"{cpuUsage:F2}%";

            double usedWidth = cpuUsage / 100.0;
            cpuUsedColumn.Width = new GridLength(usedWidth, GridUnitType.Star);
            cpuUnusedColumn.Width = new GridLength(1 - usedWidth, GridUnitType.Star);
        }

        private void UpdateMemoryUsage()
        {
            double memAvailable = memAvailableCounter.NextValue();
            double memUsed = memTotal - memAvailable;
            double memUsagePercentage = memUsed / memTotal;

            MemUsageText.Text = $"{memUsagePercentage:P2} ({memUsed / GB:F0}GB/{memTotal / GB:F0}GB)";

            memUsedColumn.Width = new GridLength(memUsagePercentage, GridUnitType.Star);
            memUnusedColumn.Width = new GridLength(1 - memUsagePercentage, GridUnitType.Star);
        }

        private void UpdateDiskUsage()
        {
            // Disk C
            long freeSpaceC = counter.GetAvailableSpaceDiskC();
            long totalSpaceC = counter.GetTotalSpaceDiskC();
            long usedSpaceC = totalSpaceC - freeSpaceC;
            double diskCUsagePercentage = (double)usedSpaceC / totalSpaceC;

/*            if(diskCUsagePercentage > 0.5)
            {
                diskCUsedBackground.Background = Brushes.Red;
            }*/

            diskCText.Text = $"{usedSpaceC / GB:F0}GB/{totalSpaceC / GB:F0}GB ({diskCUsagePercentage:P1})";

            diskCUsedColumn.Width = new GridLength(diskCUsagePercentage, GridUnitType.Star);
            diskCUnusedColumn.Width = new GridLength(1 - diskCUsagePercentage, GridUnitType.Star);
        }
    }
}
