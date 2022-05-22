using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Poly.Serialization
{
    //replacement for BitConverter https://docs.microsoft.com/zh-cn/dotnet/api/system.bitconverter?view=net-6.0
    public static class FastBitConverter
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterHelperDouble
        {
            [FieldOffset(0)]
            public ulong Along;
            [FieldOffset(0)]
            public double Adouble;
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct ConverterHelperFloat
        {
            [FieldOffset(0)]
            public int Aint;
            [FieldOffset(0)]
            public float Afloat;
        }

        #region Read/Write
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(byte[] bytes, int offset)
        {
#if BIGENDIAN
            return (short)((bytes[offset]) << 8 | (bytes[offset + 1]));
#else
            return (short)((bytes[offset]) | (bytes[offset + 1] << 8));
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(byte[] bytes, int offset)
        {
#if BIGENDIAN
            return (bytes[offset]) << 24 | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | (bytes[offset + 3]);
#else
            return (bytes[offset]) | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(byte[] bytes, int offset)
        {
#if BIGENDIAN
            int highBytes = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | (bytes[offset + 3]);
            int lowBytes = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | (bytes[offset + 7]);
            return ((uint)lowBytes | ((long)highBytes << 32));
#else
            int lowBytes = (bytes[offset]) | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24);
            int highBytes = (bytes[offset + 4]) | (bytes[offset + 5] << 8) | (bytes[offset + 6] << 16) | (bytes[offset + 7] << 24);
            return ((uint)lowBytes | ((long)highBytes << 32));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValue(byte[] bytes, int offset, short data)
        {
#if BIGENDIAN
            bytes[offset + 1] = (byte)(data);
            bytes[offset    ] = (byte)(data >> 8);
#else
            bytes[offset] = (byte)(data);
            bytes[offset + 1] = (byte)(data >> 8);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteValue(byte[] bytes, int offset, int data)
        {
#if BIGENDIAN
            bytes[offset + 3] = (byte)(data);
            bytes[offset + 2] = (byte)(data >> 8);
            bytes[offset + 1] = (byte)(data >> 16);
            bytes[offset    ] = (byte)(data >> 24);
#else
            bytes[offset] = (byte)(data);
            bytes[offset + 1] = (byte)(data >> 8);
            bytes[offset + 2] = (byte)(data >> 16);
            bytes[offset + 3] = (byte)(data >> 24);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteValue(byte[] bytes, int offset, ulong data)
        {
#if BIGENDIAN
            bytes[offset + 7] = (byte)(data);
            bytes[offset + 6] = (byte)(data >> 8);
            bytes[offset + 5] = (byte)(data >> 16);
            bytes[offset + 4] = (byte)(data >> 24);
            bytes[offset + 3] = (byte)(data >> 32);
            bytes[offset + 2] = (byte)(data >> 40);
            bytes[offset + 1] = (byte)(data >> 48);
            bytes[offset    ] = (byte)(data >> 56);
#else
            bytes[offset] = (byte)(data);
            bytes[offset + 1] = (byte)(data >> 8);
            bytes[offset + 2] = (byte)(data >> 16);
            bytes[offset + 3] = (byte)(data >> 24);
            bytes[offset + 4] = (byte)(data >> 32);
            bytes[offset + 5] = (byte)(data >> 40);
            bytes[offset + 6] = (byte)(data >> 48);
            bytes[offset + 7] = (byte)(data >> 56);
#endif
        }
        #endregion

        #region GetBytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, short value)
        {
            WriteValue(bytes, startIndex, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, ushort value)
        {
            WriteValue(bytes, startIndex, (short)value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, int value)
        {
            WriteValue(bytes, startIndex, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, uint value)
        {
            WriteValue(bytes, startIndex, (int)value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, float value)
        {
            ConverterHelperFloat ch = new ConverterHelperFloat { Afloat = value };
            WriteValue(bytes, startIndex, ch.Aint);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, long value)
        {
            WriteValue(bytes, startIndex, (ulong)value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, ulong value)
        {
            WriteValue(bytes, startIndex, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetBytes(byte[] bytes, int startIndex, double value)
        {
            ConverterHelperDouble ch = new ConverterHelperDouble { Adouble = value };
            WriteValue(bytes, startIndex, ch.Along);
        }
        #endregion

        #region ToXXX
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static bool ToBoolean(byte[] value, int startIndex) => value[startIndex] != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ToInt16(byte[] bytes, int startIndex) => ReadInt16(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ToUInt16(byte[] bytes, int startIndex) => (ushort)ReadInt16(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt32(byte[] bytes, int startIndex) => ReadInt32(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ToUInt32(byte[] bytes, int startIndex) => (uint)ReadInt32(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ToSingle(byte[] bytes, int startIndex) => new ConverterHelperFloat { Aint = ReadInt32(bytes, startIndex) }.Afloat;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToInt64(byte[] bytes, int startIndex) => ReadInt64(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToUInt64(byte[] bytes, int startIndex) => (ulong)ReadInt64(bytes, startIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ToDouble(byte[] bytes, int startIndex) => new ConverterHelperDouble { Along = (ulong)ReadInt64(bytes, startIndex) }.Adouble;

        #endregion
    }
}