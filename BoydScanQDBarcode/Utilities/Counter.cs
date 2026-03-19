using System.Diagnostics;

namespace BoydScanQDBarcode.Utilities
{
    public class Counter
    {
        #region Properties
        public PerformanceCounter FreeSpaceDiskC { get; set; }
        public PerformanceCounter FreeSpaceDiskD { get; set; }
        public System.IO.DriveInfo DriveC { get; set; }
        public System.IO.DriveInfo DriveD { get; set; }
        #endregion

        #region Constructor(s)
        public Counter()
        {
            FreeSpaceDiskC = new PerformanceCounter("LogicalDisk", "% Free Space", "C:");
            FreeSpaceDiskD = new PerformanceCounter("LogicalDisk", "% Free Space", "D:");
            DriveC = new System.IO.DriveInfo("C");
            DriveD = new System.IO.DriveInfo("D");
        }
        #endregion

        #region Methods
        public double GetFreeSpaceDiskC()
        {
            return FreeSpaceDiskC.NextValue();
        }
        public double GetFreeSpaceDiskD()
        {
            return FreeSpaceDiskD.NextValue();
        }
        public long GetTotalSpaceDiskC()
        {
            return DriveC.TotalSize;
        }
        public long GetTotalSpaceDiskD()
        {
            return DriveD.TotalSize;
        }
        public long GetAvailableSpaceDiskC()
        {
            return DriveC.AvailableFreeSpace;
        }
        public long GetAvailableSpaceDiskD()
        {
            return DriveD.AvailableFreeSpace;
        }
        #endregion
    }
}
