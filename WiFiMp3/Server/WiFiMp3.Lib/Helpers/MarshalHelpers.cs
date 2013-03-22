using System;
using System.Runtime.InteropServices;

namespace WiFiMp3.Lib.Helpers
{
    public static class MarshalHelpers
    {
        public static byte[] ToBytes(object obj)
        {
            int objSize = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(objSize);
            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                int packetSize = Marshal.SizeOf(obj);
                byte[] buffer = new byte[packetSize];
                Marshal.Copy(ptr, buffer, 0, objSize);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
