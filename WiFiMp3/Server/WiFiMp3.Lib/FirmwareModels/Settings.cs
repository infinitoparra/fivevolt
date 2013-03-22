using System;
using System.Runtime.InteropServices;

namespace WiFiMp3.Lib.FirmwareModels
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WifiSettings
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 25)]
        public string ssid;

        public byte encryption;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
        public string securityKey;

        public byte ipAddrMode;

        public UInt32 ipAddr;

        public UInt32 subnetMask;

        public UInt32 gateway;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WifiMp3Settings
    {
        public WifiSettings wifi;
    }
}
