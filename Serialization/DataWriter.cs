using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Poly.Serialization
{
    public struct DataWriter
    {
        private byte[] data;
        private readonly int offset;
        private int position;
        private readonly IPolySerializationContext context;

        public byte[] Data => data;
        public int Position => position;
        public int Count => position - offset;
        public ArraySegment<byte> DataSegment => new ArraySegment<byte>(data, offset, Count);

        public DataWriter(byte[] data, int offset, int position, IPolySerializationContext context = null)
        {
            this.data = data;
            this.offset = offset;
            this.position = position;
            this.context = context ?? DataSerializationContext.DefaultContext;
        }
        public override string ToString()
        {
            return $"NetDataWriter:{{{data?.Length},{offset},{position},{Count}}}";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeed(int newSize)
        {
            int len = data.Length;
            if (len >= newSize) return;
            while (len < newSize) len <<= 1;
            Array.Resize(ref data, len);
        }

        #region Primitive
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            ResizeIfNeed(position + 1);
            data[position++] = (byte)value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            ResizeIfNeed(position + 1);
            data[position++] = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBool(bool value)
        {
            ResizeIfNeed(position + 1);
            data[position++] = (byte)(value ? 1 : 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value)
        {
            ResizeIfNeed(position + 2);
            FastBitConverter.GetBytes(data, position, value);
            position += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShort(short value)
        {
            ResizeIfNeed(position + 2);
            FastBitConverter.GetBytes(data, position, value);
            position += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUShort(ushort value)
        {
            ResizeIfNeed(position + 2);
            FastBitConverter.GetBytes(data, position, value);
            position += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutUShort(int offset, ushort value)
        {
            int position = this.offset + offset;
            ResizeIfNeed(position + 2);
            FastBitConverter.GetBytes(data, position, value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value)
        {
            ResizeIfNeed(position + 4);
            FastBitConverter.GetBytes(data, position, value);
            position += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt(uint value)
        {
            ResizeIfNeed(position + 4);
            FastBitConverter.GetBytes(data, position, value);
            position += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFloat(float value)
        {
            ResizeIfNeed(position + 4);
            FastBitConverter.GetBytes(data, position, value);
            position += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(long value)
        {
            ResizeIfNeed(position + 8);
            FastBitConverter.GetBytes(data, position, value);
            position += 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteULong(ulong value)
        {
            ResizeIfNeed(position + 8);
            FastBitConverter.GetBytes(data, position, value);
            position += 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            ResizeIfNeed(position + 8);
            FastBitConverter.GetBytes(data, position, value);
            position += 8;
        }
        #endregion

        #region Packed Signed Short/Int/Long
        //(Ref: https://developers.google.com/protocol-buffers/docs/encoding#signed-integers)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePackedShort(short value) => WritePackedInt(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePackedInt(int value) => WritePackedUInt((uint)((value << 1) ^ (value >> 31)));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePackedLong(long value)
        {
            WritePackedInt((int)(value >> 32));
            WritePackedInt((int)(value & uint.MaxValue));
        }
        #endregion

        #region Packed Unsigned Short/Int/Long
        //(Ref: https://sqlite.org/src4/doc/trunk/www/varint.wiki)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePackedUShort(ushort value) => WritePackedULong(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WritePackedUInt(uint value) => WritePackedULong(value);
        public void WritePackedULong(ulong value)
        {
            if (value <= 240)
            {
                WriteByte((byte)value);
                return;
            }
            if (value <= 2287)
            {
                WriteByte((byte)((value - 240) / 256 + 241));
                WriteByte((byte)((value - 240) % 256));
                return;
            }
            if (value <= 67823)
            {
                WriteByte((byte)249);
                WriteByte((byte)((value - 2288) / 256));
                WriteByte((byte)((value - 2288) % 256));
                return;
            }
            if (value <= 16777215)
            {
                WriteByte((byte)250);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                return;
            }
            if (value <= 4294967295)
            {
                WriteByte((byte)251);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                return;
            }
            if (value <= 1099511627775)
            {
                WriteByte((byte)252);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                return;
            }
            if (value <= 281474976710655)
            {
                WriteByte((byte)253);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                WriteByte((byte)((value >> 40) & 0xFF));
                return;
            }
            if (value <= 72057594037927935)
            {
                WriteByte((byte)254);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                WriteByte((byte)((value >> 40) & 0xFF));
                WriteByte((byte)((value >> 48) & 0xFF));
                return;
            }
            // all others
            WriteByte((byte)255);
            WriteByte((byte)(value & 0xFF));
            WriteByte((byte)((value >> 8) & 0xFF));
            WriteByte((byte)((value >> 16) & 0xFF));
            WriteByte((byte)((value >> 24) & 0xFF));
            WriteByte((byte)((value >> 32) & 0xFF));
            WriteByte((byte)((value >> 40) & 0xFF));
            WriteByte((byte)((value >> 48) & 0xFF));
            WriteByte((byte)((value >> 56) & 0xFF));
        }
        #endregion

        #region string
        public void WriteString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteUShort(0);
                return;
            }
            //put bytes count
            int bytesCount = Encoding.UTF8.GetByteCount(value);
            ResizeIfNeed(position + bytesCount + 4);
            WriteUShort((ushort)bytesCount);
            //put string
            Encoding.UTF8.GetBytes(value, 0, value.Length, data, position);
            position += bytesCount;
        }

        #endregion

        #region Type
        private string GetAssemblyQualifiedName(Type type)
        {
            if (type == null) return "";
            var name = type.FullName;
            if (type.IsGenericType)
            {
                var arguments = new List<string>();
                foreach (var argument in type.GetGenericArguments())
                    arguments.Add($"[{GetAssemblyQualifiedName(argument)}]");
                name = $"{name}[{string.Join(", ", arguments)}]";
            }
            return $"{name}, {type.Module.Assembly.GetName().Name}";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteType(Type type)
        {
            WriteString(GetAssemblyQualifiedName(type));
        }
        #endregion

        #region bytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ArraySegment<byte> segment) => WriteBytes(segment.Array, segment.Offset, segment.Count);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(byte[] data, int offset, int count)
        {
            ResizeIfNeed(position + count);
            Buffer.BlockCopy(data, offset, this.data, position, count);
            position += count;
        }
        #endregion

        #region Array
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray<T>(ArraySegment<T> segment) => WriteArray<T>(segment.Array, segment.Offset, segment.Count);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray<T>(T[] array) => WriteArray<T>(array, 0, array.Length);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray<T>(T[] array, int offset, int count)
        {
            if (array == null) { WriteUShort(0); return; }
            WriteUShort((ushort)count);
            for (int i = 0; i < count; i++)
                WriteObject(array[i + offset]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteArray(Array array, int offset, int count)
        {
            if (array == null) { WriteUShort(0); return; }
            WriteUShort((ushort)count);
            for (int i = 0; i < count; i++)
                WriteObject(array.GetValue(i + offset));
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void WriteByteArray(ArraySegment<byte> segment) => WriteByteArray(segment.Array, segment.Offset, segment.Count);
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void WriteByteArray(byte[] array, int offset, int count)
        //{
        //    if (array == null) { WriteUShort(0); return; }
        //    WriteUShort((ushort)count);
        //    WriteBytes(array, offset, count);
        //}
        #endregion

        #region List
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteList(IList list)
        {
            if (list == null) { WriteUShort(0); return; }
            var count = list.Count;
            WriteUShort((ushort)count);
            for (int i = 0; i < count; i++)
                WriteObject(list[i]);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteList<TValue>(IList<TValue> list)
        {
            if (list == null) { WriteUShort(0); return; }
            var count = list.Count;
            WriteUShort((ushort)count);
            for (int i = 0; i < count; i++)
                WriteObject(list[i]);
        }
        #endregion

        #region Dictionary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDictionary(IDictionary dict)
        {
            if (dict == null) { WriteUShort(0); return; }
            WriteUShort((ushort)dict.Count);
            foreach (var key in dict.Keys)
            {
                WriteObject(key);
                WriteObject(dict[key]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDictionary<TKey, TValue>(IDictionary<TKey, TValue> dict)
        {
            if (dict == null) { WriteUShort(0); return; }
            WriteUShort((ushort)dict.Count);
            foreach (var pair in dict)
            {
                WriteObject(pair.Key);
                WriteObject(pair.Value);
            }
        }
        #endregion

        #region IPolySerialzable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T obj) where T : IDataSerializable
        {
            obj.Serialize(ref this);
        }
        #endregion

        #region PolyFormattable
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteFormattable(object obj)
        {
            var type = obj.GetType();
            var info = context.GetFormattableTypeInfo(type);
            var propertyInfos = info.propertyInfos;
            var count = info.propertyInfoCount;
            WriteByte((byte)count);
            var headerPos = position;
            var dataPos = headerPos + count * 2;
            position = dataPos;
            ResizeIfNeed(position);
            for (int i = 0; i < count; i++)
            {
                var headerPos1 = headerPos + (i << 1) - offset;
                var dataPos1 = position - headerPos;
                PutUShort(headerPos + (i << 1) - offset, (ushort)(position - headerPos));
                var propertyInfo = propertyInfos[i];
                var propertyValue = propertyInfo.GetValue(obj);
                WriteObject(propertyValue);
                Console.WriteLine($"WriteFormattable: {i}:{propertyInfo},{propertyInfo.PropertyType},{propertyValue},{headerPos1},{dataPos1}");
            }
        }
        #endregion

        #region object
        public void WriteObject(object value)
        {
            if (value == null) { WriteBool(true); return; }
            WriteBool(false);
            var type = value.GetType();
            var typeArguments = type.GenericTypeArguments;
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();
            if (type.IsPrimitive)
            {
                if (type == typeof(bool)) WriteBool((bool)value);
                else if (type == typeof(byte)) WriteByte((byte)value);
                else if (type == typeof(sbyte)) WriteSByte((sbyte)value);
                else if (type == typeof(char)) WriteChar((char)value);
                else if (type == typeof(short)) WritePackedShort((short)value);
                else if (type == typeof(ushort)) WritePackedUShort((ushort)value);
                else if (type == typeof(int)) WritePackedInt((int)value);
                else if (type == typeof(uint)) WritePackedUInt((uint)value);
                else if (type == typeof(float)) WriteFloat((float)value);
                else if (type == typeof(long)) WritePackedLong((long)value);
                else if (type == typeof(ulong)) WritePackedULong((ulong)value);
                else if (type == typeof(double)) WriteDouble((double)value);
            }
            else if (type == typeof(string)) WriteString((string)value);
            else if (type == typeof(Type)) WriteType((Type)value);
            else if (type.IsArray)
            {
                var array = value as Array;
                WriteArray(array, 0, array.Length);
            }
            //List<>
            else if (value is IList list && typeArguments.Length > 0)
                WriteList(list);
            //Dictionary<K,V>
            else if (value is IDictionary dict && typeArguments.Length > 0)
                WriteDictionary(dict);
            else if (value is IDataSerializable serializable)
                serializable.Serialize(ref this);
            else if(type.IsPolyFormattable())
                WriteFormattable(value);
            else
            {
                var handler = context?.GetSerialzationHandler(type);
                if (handler != null)
                    handler.Write(ref this, value, type);
                else
                    Console.Error.WriteLine($"PolyWriter cannot write type {type}");
            }
            //throw new ArgumentException("INetDataWriter cannot write type " + value.GetType().Name);
        }
        #endregion
    }
}