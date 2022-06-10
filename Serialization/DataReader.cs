using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Poly.Serialization
{
    public struct DataReader
    {
        private readonly byte[] data;
        private readonly int offset;
        private readonly int count;
        private int position;
        private readonly IPolySerializationContext context;

        public ArraySegment<byte> DataSegment => new ArraySegment<byte>(data, offset, count);
        public ArraySegment<byte> AvailableSegment => new ArraySegment<byte>(data, position, AvailableCount);
        public int AvailableCount => offset + count - position;
        public int Position => position;

        public DataReader(byte[] data, int offset, int count, int position = -1, IPolySerializationContext context = null)
        {
            this.data = data;
            this.offset = offset;
            this.count = count;
            this.position = position == -1 ? offset : position;
            this.context = context ?? DataSerializationContext.DefaultContext;
        }
        public DataReader(ArraySegment<byte> segment, int position = -1, IPolySerializationContext context = null)
            : this(segment.Array, segment.Offset, segment.Count, position, context)
        {
        }
        public override string ToString()
        {
            return $"NetDataReader:{{{data?.Length},{offset},{position},{count}}}";
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipBytes(int count) => position += count;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T CreateInstance<T>(Type type)
        {
            object obj = context?.GetObjectCreator(type)?.Invoke();
            if (obj == null) obj = Activator.CreateInstance(type);
            return (T)obj;
        }

        #region Primitive
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte() => data[position++];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadSByte() => (sbyte)data[position++];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBool() => data[position++] > 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadChar()
        {
            char result = (char)FastBitConverter.ToInt16(data, position);
            position += 2;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShort()
        {
            short result = FastBitConverter.ToInt16(data, position);
            position += 2;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUShort()
        {
            ushort result = FastBitConverter.ToUInt16(data, position);
            position += 2;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetUShort(int offset)
        {
            int position = this.offset + offset;
            return FastBitConverter.ToUInt16(data, position);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            int result = FastBitConverter.ToInt32(data, position);
            position += 4;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt()
        {
            uint result = FastBitConverter.ToUInt32(data, position);
            position += 4;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloat()
        {
            float result = FastBitConverter.ToSingle(data, position);
            position += 4;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLong()
        {
            long result = FastBitConverter.ToInt64(data, position);
            position += 8;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadULong()
        {
            ulong result = FastBitConverter.ToUInt64(data, position);
            position += 8;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadDouble()
        {
            double result = FastBitConverter.ToDouble(data, position);
            position += 8;
            return result;
        }
        #endregion

        #region Packed Signed Short/Int/Long 
        //(Ref: https://developers.google.com/protocol-buffers/docs/encoding#signed-integers)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadPackedShort() => (short)ReadPackedInt();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadPackedInt()
        {
            uint value = ReadPackedUInt();
            return (int)((value >> 1) ^ (-(int)(value & 1)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadPackedLong() => ((long)ReadPackedInt()) << 32 | ((uint)ReadPackedInt());
        #endregion

        #region Packed Unsigned Short/Int/Long
        //(Ref: https://sqlite.org/src4/doc/trunk/www/varint.wiki)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadPackedUShort() => (ushort)ReadPackedULong();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadPackedUInt() => (uint)ReadPackedULong();
        public ulong ReadPackedULong()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
                return a0;
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
                return 240 + 256 * (a0 - ((ulong)241)) + a1;
            byte a2 = ReadByte();
            if (a0 == 249)
                return 2288 + (((ulong)256) * a1) + a2;
            byte a3 = ReadByte();
            if (a0 == 250)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16);
            byte a4 = ReadByte();
            if (a0 == 251)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24);
            byte a5 = ReadByte();
            if (a0 == 252)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32);
            byte a6 = ReadByte();
            if (a0 == 253)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40);
            byte a7 = ReadByte();
            if (a0 == 254)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48);
            byte a8 = ReadByte();
            if (a0 == 255)
                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48) + (((ulong)a8) << 56);
            throw new System.IndexOutOfRangeException("ReadPackedULong() failure: " + a0);
        }
        #endregion

        #region string
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString()
        {
            int bytesCount = ReadUShort();
            if (bytesCount <= 0) return string.Empty;
            string result = Encoding.UTF8.GetString(data, position, bytesCount);
            position += bytesCount;
            return result;
        }
        #endregion

        #region Type
        public Type ReadType()
        {
            var typeStr = ReadString();
            if (string.IsNullOrEmpty(typeStr)) return null;
            return Type.GetType(typeStr);
        }
        #endregion

        #region bytes
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(byte[] dest, int offset, int count)
        {
            Buffer.BlockCopy(data, position, dest, offset, count);
            position += count;
        }
        #endregion

        #region Array
        public Array ReadArray(Type type)
        {
            int count = ReadUShort();
            Array array = Array.CreateInstance(type, count);
            for (int i = 0; i < count; ++i)
                array.SetValue(ReadObject(type), i);
            return array;
        }
        public TValue[] ReadArray<TValue>()
        {
            int count = ReadUShort();
            TValue[] result = new TValue[count];
            for (int i = 0; i < count; ++i)
                result[i] = ReadObject<TValue>();
            return result;
        }
        //public byte[] ReadByteArray()
        //{
        //    int count = ReadUShort();
        //    var array = new byte[count];
        //    ReadBytes(array, 0, count);
        //    return array;
        //}
        #endregion

        #region List
        public void ReadList(IList result, Type elementType)
        {
            int count = ReadUShort();
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject(elementType));
        }
        public IList ReadList(Type elementType)
        {
            var result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            int count = ReadUShort();
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject(elementType));
            return result;
        }
        public void ReadList<TValue>(IList<TValue> result)
        {
            int count = ReadUShort();
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject<TValue>());
        }
        public List<TValue> ReadList<TValue>()
        {
            int count = ReadUShort();
            List<TValue> result = new List<TValue>();
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject<TValue>());
            return result;
        }
        #endregion

        #region Dictionary
        public IDictionary ReadDictionary(Type keyType, Type valueType)
        {
            int count = ReadUShort();
            var result = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject(keyType), ReadObject(valueType));
            return result;
        }
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            int count = ReadUShort();
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            for (int i = 0; i < count; ++i)
                result.Add(ReadObject<TKey>(), ReadObject<TValue>());
            return result;
        }
        #endregion

        #region IPolySerializable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : IDataSerializable, new()
        {
            var obj = new T();
            obj.Deserialize(ref this);
            return obj;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T obj) where T : IDataSerializable => obj.Deserialize(ref this);
        #endregion

        #region PolyFormattable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadFormattable(Type type)
        {
            var obj = CreateInstance<object>(type);
            var info = context.GetFormattableTypeInfo(type);
            //var propertyInfos = info.propertyInfos;
            //var count = info.propertyInfoCount;
            var count = ReadByte();
            var headerPos = position;
            //var dataPos = headerPos + count * 2;
            //position = dataPos;
            for (int i = 0; i < count; i++)
            {
                var propertyInfo = info.GetPropertyInfo(i);
                if (propertyInfo == null) continue;
                position = GetUShort(headerPos + (i << 1) - offset) + headerPos;
                var headerPos1 = headerPos + (i << 1) - offset;
                var dataPos1 = position - headerPos;
                var propertyValue = ReadObject(propertyInfo.PropertyType);
                propertyInfo.SetValue(obj, propertyValue);

                Console.WriteLine($"ReadFormattable: {i}:{propertyInfo},{propertyInfo.PropertyType},{propertyValue},{headerPos1},{dataPos1}");
            }
            return obj;
        }
        #endregion

        #region object
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadObject<T>() => (T)ReadObject(typeof(T));
        public object ReadObject(Type type)
        {
            var isNull = ReadBool();
            if (isNull) return null;
            var typeArguments = type.GenericTypeArguments;
            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();
            if (type.IsPrimitive)
            {
                if (type == typeof(bool)) return ReadBool();
                if (type == typeof(byte)) return ReadByte();
                if (type == typeof(sbyte)) return ReadSByte();
                if (type == typeof(char)) return ReadChar();
                if (type == typeof(short)) return ReadPackedShort();
                if (type == typeof(ushort)) return ReadPackedUShort();
                if (type == typeof(int)) return ReadPackedInt();
                if (type == typeof(uint)) return ReadPackedUInt();
                if (type == typeof(float)) return ReadFloat();
                if (type == typeof(long)) return ReadPackedLong();
                if (type == typeof(ulong)) return ReadPackedULong();
                if (type == typeof(double)) return ReadDouble();

            }
            if (type == typeof(string)) return ReadString();
            if (type == typeof(Type)) return ReadType();
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return ReadArray(elementType);
            }
            if (typeof(IList).IsAssignableFrom(type) && typeArguments.Length > 0)
            {
                return ReadList(type.GenericTypeArguments[0]);
            }
            if (typeof(IDictionary).IsAssignableFrom(type) && typeArguments.Length > 0)
            {
                var keyType = type.GenericTypeArguments[0];
                var valueType = type.GenericTypeArguments[1];
                return ReadDictionary(keyType, valueType);
            }
            if (typeof(IDataSerializable).IsAssignableFrom(type))
            {
                var obj = CreateInstance<IDataSerializable>(type);
                obj?.Deserialize(ref this);
                return obj;
            }
            if (type.IsPolyFormattable()) return ReadFormattable(type);

            var handler = context?.GetSerialzationHandler(type);
            // Debug.LogWarning($"NetDataReader.ReadValue: JsonConvert [{type}] -> [{handler}]");
            if (handler != null)
                return handler.Read(ref this, type);
            Console.Error.WriteLine($"PolyReader cannot read type {type}");
            //throw new ArgumentException("NetDataReader.ReadValue: cannot read type " + type.Name);
            return null;
        }
        #endregion
    }
}