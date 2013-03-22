using System;

namespace Quadcopter.Infrastructure
{
    public static class BinaryHelper
    {
        public const int SIZE_FLOAT = 4;
        public const int SIZE_INT = 2;
        public const int SIZE_LONG = 4;

        public static int ReadInt(byte[] data, ref int offset)
        {
            int result;
            unsafe
            {
                fixed (byte* p = data)
                {
                    result = *((Int16*)(p + offset));
                }
            }
            offset += SIZE_INT;
            return result;
        }

        public static int ReadLong(byte[] data, ref int offset)
        {
            int result;
            unsafe
            {
                fixed (byte* p = data)
                {
                    result = *((Int32*)(p + offset));
                }
            }
            offset += SIZE_LONG;
            return result;
        }

        public static float ReadFloat(byte[] data, ref int offset)
        {
            float result;
            unsafe
            {
                fixed (byte* p = data)
                {
                    result = *((float*)(p + offset));
                }
            }
            offset += SIZE_FLOAT;
            return result;
        }

        public static void WriteFloat(double value, byte[] data, ref int offset)
        {
            WriteFloat((float)value, data, ref offset);
        }

        public static void WriteFloat(float value, byte[] data, ref int offset)
        {
            unsafe
            {
                fixed (byte* p = data)
                {
                    *((float*)(p + offset)) = value;
                }
            }
            offset += SIZE_FLOAT;
        }

        public static byte ComputeChecksumCRC8(byte[] buffer, int offset, int len)
        {
            byte result = 0;
            for (int i = offset; i < offset + len; i++)
            {
                result = (byte)(result ^ buffer[i]);
            }
            return result;
        }

        public static byte ReadByte(byte[] buffer, ref int i)
        {
            return buffer[i++];
        }

        public static UInt32 ReadUnsignedLong(byte[] data, ref int i)
        {
            return (UInt32)ReadLong(data, ref i);
        }
    }
}
