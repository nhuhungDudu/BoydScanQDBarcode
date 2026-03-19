using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public static class ClsDefine
{
    public class DefineSystem
    {
        public static bool IsOfflineMode = bool.Parse(ClsIO.ReadValue("SystemCongfig", "OfflineMode", "False", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini"));
        public static int iLifeTimeWarning = int.Parse(ClsIO.ReadValue("SystemCongfig", "LifeTimeWarning", "20", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini"));
        public static string sQDlogpath = ClsIO.ReadValue("SystemCongfig", "QDlogpath", "C:\\Model\\LogLifeTime\\QDlifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        //public static string sOringlogpath = ClsIO.ReadValue("SystemCongfig", "Oringlogpath", "C:\\Model\\LogLifeTime\\Oringlifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        public static string sQD2logpath = ClsIO.ReadValue("SystemCongfig", "QD2logpath", "C:\\Model\\LogLifeTime\\QD2lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        //public static string sOring2logpath = ClsIO.ReadValue("SystemCongfig", "Oring2logpath", "C:\\Model\\LogLifeTime\\Oring2lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        public static string sQD3logpath = ClsIO.ReadValue("SystemCongfig", "QD3logpath", "C:\\Model\\LogLifeTime\\QD3lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        //public static string sOring2logpath = ClsIO.ReadValue("SystemCongfig", "Oring2logpath", "C:\\Model\\LogLifeTime\\Oring2lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        public static string sQD4logpath = ClsIO.ReadValue("SystemCongfig", "QD4logpath", "C:\\Model\\LogLifeTime\\QD4lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
        //public static string sOring2logpath = ClsIO.ReadValue("SystemCongfig", "Oring2logpath", "C:\\Model\\LogLifeTime\\Oring2lifetime.json", "C:\\Aavid_Test\\Setup-ini\\SystemConfiguration.ini");
    }
    public class DefineASP
    {
        public static string PortName = ClsIO.ReadValue("ASPSerialPort", "SerialCom", "", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        public static int Baudrate = int.Parse(ClsIO.ReadValue("ASPSerialPort", "Baudrate", "115200", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        public static string Parity = ClsIO.ReadValue("ASPSerialPort", "Parity", "None", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini:\\Aavid_Test\\Setup-ini\\SerialPort");
        public static int DataBit = int.Parse(ClsIO.ReadValue("ASPSerialPort", "DataBit", "8", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini"));
        public static string StopBit = ClsIO.ReadValue("ASPSerialPort", "StopBit", "One", "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        public static void SaveASPSerialPortInformation()
        {
            ClsIO.WriteValue("ASPSerialPort", "SerialCom", PortName, "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Baudrate", Baudrate.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "DataBit", DataBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "Parity", Parity.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
            ClsIO.WriteValue("ASPSerialPort", "StopBit", StopBit.ToString(), "C:\\Aavid_Test\\Setup-ini\\SerialPort.ini");
        }
    }
    public class DefineFixture
    {
        public static bool bDualFixture = bool.Parse(ClsIO.ReadValue("Fixture", "DualFixture", "False", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static bool bAutoScanner = bool.Parse(ClsIO.ReadValue("Fixture", "AutoScanner", "True", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static bool bAutoFixture = bool.Parse(ClsIO.ReadValue("Fixture", "AutoFixture", "True", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static bool bN2PressureCheck = bool.Parse(ClsIO.ReadValue("Fixture", "N2PressureCheck", "False", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static double dN2FillTimeOut = int.Parse(ClsIO.ReadValue("Fixture", "N2FillTimeOut", "20", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static double dN2StabilityTime = int.Parse(ClsIO.ReadValue("Fixture", "N2StabilityTime", "15", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static double dN2CheckMin = int.Parse(ClsIO.ReadValue("Fixture", "N2CheckMin", "100", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static double dN2CheckMax = int.Parse(ClsIO.ReadValue("Fixture", "N2CheckMax", "150", "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini"));
        public static void SaveFixtureSetting()
        {
            ClsIO.WriteValue("Fixture", "DualFixture", bDualFixture.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "AutoScanner", bAutoScanner.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "AutoFixture", bAutoFixture.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "N2PressureCheck", bN2PressureCheck.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "N2FillTimeOut", dN2FillTimeOut.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "N2StabilityTime", dN2StabilityTime.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "N2CheckMin", dN2CheckMin.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
            ClsIO.WriteValue("Fixture", "N2CheckMax", dN2CheckMax.ToString(), "C:\\Aavid_Test\\Setup-ini\\FixtureConfiguration.ini");
        }
    }
    public class DefineAnalogSensor
    {
        public static double N2_PressureMax = double.Parse(ClsIO.ReadValue("N2_Pressure", "N2_Pressure_Max", "145.04", $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini"));
        public static double N2_PressureMin = double.Parse(ClsIO.ReadValue("N2_Pressure", "N2_Pressure_Min", "-14.504", $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini"));
        public static double N2_PressureVoltgeMax = double.Parse(ClsIO.ReadValue("N2_Pressure", "N2_Pressure_Voltge_Max", "5", $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini"));
        public static double N2_PressureVoltgeMin = double.Parse(ClsIO.ReadValue("N2_Pressure", "N2_Pressure_Voltge_Min", "0.6", $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini"));
        public static void SaveFixtureSetting()
        {
            ClsIO.WriteValue("N2_Pressure", "N2_Pressure_Max", N2_PressureMax.ToString(), $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini");
            ClsIO.WriteValue("N2_Pressure", "N2_Pressure_Min", N2_PressureMin.ToString(), $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini");
            ClsIO.WriteValue("N2_Pressure", "N2_Pressure_Voltge_Max", N2_PressureVoltgeMax.ToString(), $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini");
            ClsIO.WriteValue("N2_Pressure", "N2_Pressure_Voltge_Min", N2_PressureVoltgeMin.ToString(), $"C:\\Aavid_Test\\Setup-ini\\AnalogSensorConfiguration.ini");
        }
    }
    public class DefineTesting
    {
        public static int Time2stable = 5;
    }
}

