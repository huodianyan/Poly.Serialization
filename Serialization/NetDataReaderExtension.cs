//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Newtonsoft.Json;

//namespace Poly.Serialization
//{
//    public static class NetDataReaderExtension
//    {
//        public static TType ReadValue<TType>(this ref PolyReader reader)
//        {
//            return (TType)ReadValue(ref reader, typeof(TType));
//        }
//        private static T CreateInstance<T>(this ref PolyReader reader, Type type)
//        {
//            object obj = null;
//            var context = reader.context;
//            if (context != null)
//            {
//                obj = context.GetObjectCreator(type)?.Invoke();
//            }
//            if (obj == null)
//                obj = Activator.CreateInstance(type);
//            return (T)obj;
//        }
//        public static object ReadValue(this ref PolyReader reader, Type type)
//        {
//            var isNull = reader.ReadBool();
//            if (isNull)
//                return null;

//            #region Generic Values
//            if (type.IsEnum)
//                type = type.GetEnumUnderlyingType();

//            if (type == typeof(bool))
//                return reader.ReadBool();

//            if (type == typeof(byte))
//                return reader.ReadByte();

//            if (type == typeof(char))
//                return reader.ReadChar();

//            if (type == typeof(double))
//                return reader.ReadDouble();

//            if (type == typeof(float))
//                return reader.ReadFloat();

//            if (type == typeof(int))
//                return ReadPackedInt(ref reader);

//            if (type == typeof(long))
//                return ReadPackedLong(ref reader);

//            if (type == typeof(sbyte))
//                return reader.ReadSByte();

//            if (type == typeof(short))
//                return ReadPackedShort(ref reader);

//            if (type == typeof(string))
//                return reader.ReadString();

//            if (type == typeof(uint))
//                return ReadPackedUInt(ref reader);
//            if (type == typeof(ulong))
//                return ReadPackedULong(ref reader);
//            if (type == typeof(ushort))
//                return ReadPackedUShort(ref reader);
//            if (type == typeof(Type))
//                return ReadType(ref reader);
//            else if (type.IsArray)
//            {
//                //var obj = createFunc?.Invoke(type);
//                //obj = obj ?? Activator.CreateInstance(type);
//                //var array = obj as Array;
//                var array = reader.CreateInstance<Array>(type);
//                var elementType = type.GetElementType();
//                var length = reader.ReadUShort();
//                for (int i = 0; i < length; i++)
//                    array.SetValue(ReadValue(ref reader, elementType), i);
//                return array;
//            }
//            else if (typeof(IList).IsAssignableFrom(type))
//            {
//                if (type.GenericTypeArguments.Length == 0)
//                {
//                    //logger.LogError($"{reader.GetType().Name}.Deserialize: only support IList<>!");
//                    return null;
//                }
//                //var obj = Activator.CreateInstance(type);
//                //var obj = createFunc?.Invoke(type);
//                //obj = obj ?? Activator.CreateInstance(type);
//                //var list = obj as IList;
//                var list = reader.CreateInstance<IList>(type);
//                var elementType = type.GenericTypeArguments[0];
//                var length = reader.ReadUShort();
//                for (int i = 0; i < length; i++)
//                {
//                    list.Add(ReadValue(ref reader, elementType));
//                }
//                return list;
//            }
//            else if (typeof(IDictionary).IsAssignableFrom(type))
//            {
//                if (type.GenericTypeArguments.Length == 0)
//                {
//                    //logger.LogError($"{reader.GetType().Name}.Deserialize: only support IList<>!");
//                    return null;
//                }
//                //var obj = Activator.CreateInstance(type);
//                //var obj = createFunc?.Invoke(type);
//                //obj = obj ?? Activator.CreateInstance(type);
//                //var dict = obj as IDictionary;
//                var dict = reader.CreateInstance<IDictionary>(type);
//                var keyType = type.GenericTypeArguments[0];
//                var valueType = type.GenericTypeArguments[1];
//                var length = reader.ReadUShort();
//                for (int i = 0; i < length; i++)
//                {
//                    dict.Add(ReadValue(ref reader, keyType), ReadValue(ref reader, valueType));
//                }
//                return dict;
//            }
//            #endregion

//            #region  Unity types
//#if UNITY_2017_1_OR_NEWER
//            if (type == typeof(Vector2))
//                return reader.ReadVector2();
//            if (type == typeof(Vector3))
//                return reader.ReadVector3();
//            if (type == typeof(Vector4))
//                return reader.ReadVector4();
//            if (type == typeof(Quaternion))
//                return reader.ReadQuaternion();
//            if (type == typeof(Vector2Int))
//                return reader.ReadVector2Int();
//            if (type == typeof(Vector3Int))
//                return reader.ReadVector3Int();
//            if (type == typeof(Color))
//                return reader.ReadColor();
//            if (type == typeof(Color32))
//                return reader.ReadColor32();
//            if (type == typeof(Pose))
//                return reader.ReadPose();
//#endif
//            #endregion

//            else if (typeof(INetSerializable).IsAssignableFrom(type))
//            {
//                //object instance = Activator.CreateInstance(type);
//                //var obj = createFunc?.Invoke(type);
//                //obj = obj ?? Activator.CreateInstance(type);
//                var obj = reader.CreateInstance<INetSerializable>(type);
//                obj.Deserialize(ref reader);
//                return obj;
//            }
//            else
//            {
//                var handler = reader.context?.GetSerialzationHandler(type);
//                // Debug.LogWarning($"NetDataReader.ReadValue: JsonConvert [{type}] -> [{handler}]");
//                if (handler != null)
//                {
//                    var obj = handler.Read(ref reader, type);
//                    return obj;
//                }
//                else
//                {
//                    try
//                    {
//                        var json = reader.ReadString();
//                        // Debug.LogWarning($"NetDataReader.ReadValue: JsonConvert [{type}] -> [{json}]");
//                        return JsonConvert.DeserializeObject(json, type);
//                    }
//                    catch (Exception ex)
//                    {
//                        Debug.LogWarning($"NetDataReader.ReadValue: JsonConvert [{type}] -> [{ex}]");
//                    }
//                }
//            }

//            //if(NetSettings.NetDataSerializer != null)
//            //{
//            //    return NetSettings.NetDataSerializer.Deserialize(type, ref reader);
//            //}

//            throw new ArgumentException("NetDataReader.ReadValue: cannot read type " + type.Name);
//        }

//#if UNITY_2017_1_OR_NEWER
//        public static Vector2 ReadVector2(this ref NetDataReader reader)
//        {
//            return new Vector2(reader.ReadFloat(), reader.ReadFloat());
//        }
//        public static Vector3 ReadVector3(this ref NetDataReader reader)
//        {
//            return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
//        }
//        public static Vector4 ReadVector4(this ref NetDataReader reader)
//        {
//            return new Vector4(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
//        }
//        public static Quaternion ReadQuaternion(this ref NetDataReader reader)
//        {
//            return new Quaternion(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
//        }
//        public static Vector2Int ReadVector2Int(this ref NetDataReader reader)
//        {
//            return new Vector2Int(reader.ReadInt(), reader.ReadInt());
//        }
//        public static Vector3Int ReadVector3Int(this ref NetDataReader reader)
//        {
//            return new Vector3Int(reader.ReadInt(), reader.ReadInt(), reader.ReadInt());
//        }
//        public static Color ReadColor(this ref NetDataReader reader)
//        {
//            return new Color(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
//        }
//        public static Color32 ReadColor32(this ref NetDataReader reader)
//        {
//            return new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
//        }
//        public static Pose ReadPose(this ref NetDataReader reader)
//        {
//            return new Pose(reader.ReadVector3(), reader.ReadQuaternion());
//        }
//#endif
//        public static Type ReadType(this ref PolyReader reader)
//        {
//            var typeStr = reader.ReadString();
//            Debug.Log($"ReadType: {typeStr}");
//            if (string.IsNullOrEmpty(typeStr))
//                return null;
//            return Type.GetType(typeStr);
//        }

//        public static TValue[] ReadArray<TValue>(this ref PolyReader reader)
//        {
//            int count = reader.ReadUShort();
//            TValue[] result = new TValue[count];
//            for (int i = 0; i < count; ++i)
//            {
//                result[i] = ReadValue<TValue>(ref reader);
//            }
//            return result;
//        }

//        public static byte[] ReadByteArray(this ref PolyReader reader)
//        {
//            int count = reader.ReadUShort();
//            var array = new byte[count];
//            //logger.LogTrace($"NetDataWriter.ReadByteArray: {array.Length},0,{count}, {reader.DataSize}, {reader.AvailableBytes}");
//            reader.ReadBytes(array, 0, count);
//            return array;
//        }

//        public static object ReadArray(this ref PolyReader reader, Type type)
//        {
//            int count = reader.ReadUShort();
//            Array array = Array.CreateInstance(type, count);
//            for (int i = 0; i < count; ++i)
//            {
//                array.SetValue(ReadValue(ref reader, type), i);
//            }
//            return array;
//        }

//        public static void ReadList(this ref PolyReader reader, IList result, Type type)
//        {
//            int count = reader.ReadUShort();
//            for (int i = 0; i < count; ++i)
//            {
//                result.Add(ReadValue(ref reader, type));
//            }
//        }
//        public static void ReadList<TValue>(this ref PolyReader reader, IList<TValue> result)
//        {
//            int count = reader.ReadUShort();
//            for (int i = 0; i < count; ++i)
//            {
//                result.Add(ReadValue<TValue>(ref reader));
//            }
//        }

//        public static List<TValue> ReadList<TValue>(this ref PolyReader reader)
//        {
//            int count = reader.ReadUShort();
//            List<TValue> result = new List<TValue>();
//            for (int i = 0; i < count; ++i)
//            {
//                result.Add(ReadValue<TValue>(ref reader));
//            }
//            return result;
//        }

//        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this ref PolyReader reader)
//        {
//            int count = reader.ReadUShort();
//            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
//            for (int i = 0; i < count; ++i)
//            {
//                result.Add(ReadValue<TKey>(ref reader), ReadValue<TValue>(ref reader));
//            }
//            return result;
//        }

//        #region Packed Signed Int (Ref: https://developers.google.com/protocol-buffers/docs/encoding#signed-integers)
//        public static short ReadPackedShort(this ref PolyReader reader)
//        {
//            return (short)ReadPackedInt(ref reader);
//        }

//        public static int ReadPackedInt(this ref PolyReader reader)
//        {
//            uint value = ReadPackedUInt(ref reader);
//            return (int)((value >> 1) ^ (-(int)(value & 1)));
//        }

//        public static long ReadPackedLong(this ref PolyReader reader)
//        {
//            return ((long)ReadPackedInt(ref reader)) << 32 | ((uint)ReadPackedInt(ref reader));
//        }
//        #endregion

//        #region Packed Unsigned Int (Ref: https://sqlite.org/src4/doc/trunk/www/varint.wiki)
//        public static ushort ReadPackedUShort(this ref PolyReader reader)
//        {
//            return (ushort)ReadPackedULong(ref reader);
//        }

//        public static uint ReadPackedUInt(this ref PolyReader reader)
//        {
//            return (uint)ReadPackedULong(ref reader);
//        }

//        public static ulong ReadPackedULong(this ref PolyReader reader)
//        {
//            byte a0 = reader.ReadByte();
//            if (a0 < 241)
//            {
//                return a0;
//            }

//            byte a1 = reader.ReadByte();
//            if (a0 >= 241 && a0 <= 248)
//            {
//                return 240 + 256 * (a0 - ((ulong)241)) + a1;
//            }

//            byte a2 = reader.ReadByte();
//            if (a0 == 249)
//            {
//                return 2288 + (((ulong)256) * a1) + a2;
//            }

//            byte a3 = reader.ReadByte();
//            if (a0 == 250)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16);
//            }

//            byte a4 = reader.ReadByte();
//            if (a0 == 251)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24);
//            }

//            byte a5 = reader.ReadByte();
//            if (a0 == 252)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32);
//            }

//            byte a6 = reader.ReadByte();
//            if (a0 == 253)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40);
//            }

//            byte a7 = reader.ReadByte();
//            if (a0 == 254)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48);
//            }

//            byte a8 = reader.ReadByte();
//            if (a0 == 255)
//            {
//                return a1 + (((ulong)a2) << 8) + (((ulong)a3) << 16) + (((ulong)a4) << 24) + (((ulong)a5) << 32) + (((ulong)a6) << 40) + (((ulong)a7) << 48) + (((ulong)a8) << 56);
//            }
//            throw new System.IndexOutOfRangeException("ReadPackedULong() failure: " + a0);
//        }
//        #endregion
//    }
//}