using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class  StateCommon
{
    public enum SecsionTest
    {
        Pressure_Decay,
        DI_Water_Wash,
        Helium_Leak_Test,
        Hot_Air_Drying,
        Pressure_Dwell,
        Thermal_Resistance_Test,
        Fill_N2_to_Ship,
        Final_Motor_Test,
        QD_Asembly_Dummy
    }
    public static SecsionTest Secsion;

}


