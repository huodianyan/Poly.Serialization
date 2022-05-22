//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Newtonsoft.Json;

//namespace Poly.Serialization
//{
//    public static class NetDataWriterExtension
//    {
//        public static void WriteValue(this ref NetDataWriter writer, object value)
//        {
//            if (value == null)
//            {
//                writer.WriteBool(true);
//                return;
//            }
//            writer.WriteBool(false);
//            var type = value.GetType();

//            #region Generic Values
//            if (type.IsEnum)
//                type = type.GetEnumUnderlyingType();
//            if (type.IsPrimitive)
//            {
//                if (type == typeof(bool)) writer.WriteBool((bool)value);
//                else if (type == typeof(byte)) writer.WriteByte((byte)value);
//                else if (type == typeof(sbyte)) writer.WriteSByte((sbyte)value);
//                else if (type == typeof(char)) writer.WriteChar((char)value);
//                else if (type == typeof(short)) WritePackedShort(ref writer, (short)value);
//                else if (type == typeof(ushort)) WritePackedUShort(ref writer, (ushort)value);
//                else if (type == typeof(int)) WritePackedInt(ref writer, (int)value);
//                else if (type == typeof(uint)) WritePackedUInt(ref writer, (uint)value);
//                else if (type == typeof(float)) writer.WriteFloat((float)value);
//                else if (type == typeof(long)) WritePackedLong(ref writer, (long)value);
//                else if (type == typeof(ulong)) WritePackedULong(ref writer, (ulong)value);
//                else if (type == typeof(double)) writer.WriteDouble((double)value);
//                return;
//            }

//            if (type == typeof(string))
//            {
//                writer.WriteString((string)value);
//            }
//            // if (type == typeof(Type))
//            if (value is Type)
//            {
//                WriteType(ref writer, (Type)value);
//                return;
//            }
//            else if (type.IsArray)
//            {
//                var array = value as Array;
//                var elementType = type.GetElementType();
//                var length = array.Length;
//                writer.WriteUShort((ushort)length);
//                for (int i = 0; i < length; i++)
//                    WriteValue(ref writer, array.GetValue(i));
//                return;
//            }
//            //else if (value is IEnumerable)
//            //{
//            //    if (type.GenericTypeArguments.Length == 0)
//            //    {
//            //        logger.LogError($"{writer.GetType().Name}.Serialize: only support IList<>!");
//            //        return;
//            //    }
//            //}
//            else if (value is IList list)
//            {
//                if (type.GenericTypeArguments.Length == 0)
//                {
//                    //logger.LogError($"{writer.GetType().Name}.Serialize: only support IList<>!");
//                    return;
//                }
//                writer.WriteList(list);
//                //var length = list.Count;
//                //writer.WriteUShort((ushort)length);
//                //for (int i = 0; i < length; i++)
//                //    WriteValue(ref writer, list[i]);
//                return;
//            }
//            else if (value is IDictionary dict)
//            {
//                if (type.GenericTypeArguments.Length == 0)
//                {
//                    //logger.LogError($"{writer.GetType().Name}.Serialize: only support IList<>!");
//                    return;
//                }
//                var length = dict.Count;
//                writer.WriteUShort((ushort)length);
//                var keyType = type.GenericTypeArguments[0];
//                var valueType = type.GenericTypeArguments[1];
//                for (int i = 0; i < length; i++)
//                    foreach (var key in dict.Keys)
//                    {
//                        WriteValue(ref writer, key);
//                        WriteValue(ref writer, dict[key]);
//                    }
//                return;
//            }
//            #endregion

//            #region  Unity types
//#if UNITY_2017_1_OR_NEWER
//            if (type == typeof(Vector2))
//            {
//                writer.WriteVector2((Vector2)value);
//                return;
//            }
//            if (type == typeof(Vector3))
//            {
//                writer.WriteVector3((Vector3)value);
//                return;
//            }
//            if (type == typeof(Vector4))
//            {
//                writer.WriteVector4((Vector4)value);
//                return;
//            }
//            if (type == typeof(Quaternion))
//            {
//                writer.WriteQuaternion((Quaternion)value);
//                return;
//            }
//            if (type == typeof(Vector2Int))
//            {
//                writer.WriteVector2Int((Vector2Int)value);
//                return;
//            }
//            if (type == typeof(Vector3Int))
//            {
//                writer.WriteVector3Int((Vector3Int)value);
//                return;
//            }
//            if (type == typeof(Color))
//            {
//                writer.WriteColor((Color)value);
//                return;
//            }
//            if (type == typeof(Color32))
//            {
//                writer.WriteColor32((Color32)value);
//                return;
//            }
//            if (type == typeof(Pose))
//            {
//                writer.WritePose((Pose)value);
//                return;
//            }
//#endif
//            #endregion

//            else if (typeof(INetSerializable).IsAssignableFrom(type))
//            {
//                (value as INetSerializable).Serialize(ref writer);
//                return;
//            }
//            else
//            {
//                var handler = writer.context?.GetSerialzationHandler(type);
//                if (handler != null)
//                {
//                    handler.Write(ref writer, value, type);
//                    return;
//                }
//                else
//                {
//                    try
//                    {
//                        var json = JsonConvert.SerializeObject(value);
//                        Debug.LogWarning($"NetDataReader.WriteValue: JsonConvert [{type}] -> [{json}]");
//                        writer.WriteString(json);
//                        return;
//                    }
//                    catch (Exception) { }
//                }
//            }

//            throw new ArgumentException("INetDataWriter cannot write type " + value.GetType().Name);
//        }

//#if UNITY_2017_1_OR_NEWER
//        public static void WriteVector2(this ref NetDataWriter writer, Vector2 value)
//        {
//            writer.WriteFloat(value.x);
//            writer.WriteFloat(value.y);
//        }
//        public static void WriteVector3(this ref NetDataWriter writer, Vector3 value)
//        {
//            writer.WriteFloat(value.x);
//            writer.WriteFloat(value.y);
//            writer.WriteFloat(value.z);
//        }
//        public static void WriteVector4(this ref NetDataWriter writer, Vector4 value)
//        {
//            writer.WriteFloat(value.x);
//            writer.WriteFloat(value.y);
//            writer.WriteFloat(value.z);
//            writer.WriteFloat(value.w);
//        }
//        public static void WriteQuaternion(this ref NetDataWriter writer, Quaternion value)
//        {
//            writer.WriteFloat(value.x);
//            writer.WriteFloat(value.y);
//            writer.WriteFloat(value.z);
//            writer.WriteFloat(value.w);
//        }
//        public static void WriteVector2Int(this ref NetDataWriter writer, Vector2Int value)
//        {
//            writer.WriteInt(value.x);
//            writer.WriteInt(value.y);
//        }
//        public static void WriteVector3Int(this ref NetDataWriter writer, Vector3Int value)
//        {
//            writer.WriteInt(value.x);
//            writer.WriteInt(value.y);
//            writer.WriteInt(value.z);
//        }
//        public static void WriteColor(this ref NetDataWriter writer, Color value)
//        {
//            writer.WriteFloat(value.r);
//            writer.WriteFloat(value.g);
//            writer.WriteFloat(value.b);
//            writer.WriteFloat(value.a);
//        }
//        public static void WriteColor32(this ref NetDataWriter writer, Color32 value)
//        {
//            writer.WriteByte(value.r);
//            writer.WriteByte(value.g);
//            writer.WriteByte(value.b);
//            writer.WriteByte(value.a);
//        }
//        public static void WritePose(this ref NetDataWriter writer, Pose value)
//        {
//            writer.WriteVector3(value.position);
//            writer.WriteQuaternion(value.rotation);
//        }
//#endif


//        #region Array
//        public static void WriteArray<T>(this ref NetDataWriter writer, ArraySegment<T> segment)
//        {
//            WriteArray<T>(ref writer, segment.Array, segment.Offset, segment.Count);
//        }
//        public static void WriteArray<T>(this ref NetDataWriter writer, T[] array)
//        {
//            WriteArray<T>(ref writer, array, 0, array.Length);
//        }
//        public static void WriteArray<T>(this ref NetDataWriter writer, T[] array, int offset, int count)
//        {
//            if (array == null)
//            {
//                writer.WriteUShort(0);
//                return;
//            }
//            writer.WriteUShort((ushort)count);
//            for (int i = 0; i < count; i++)
//            {
//                var value = array[i + offset];
//                writer.WriteValue(value);
//            }
//        }
//        public static void WriteByteArray(this ref NetDataWriter writer, ArraySegment<byte> segment)
//        {
//            WriteByteArray(ref writer, segment.Array, segment.Offset, segment.Count);
//        }
//        public static void WriteByteArray(this ref NetDataWriter writer, byte[] array, int offset, int count)
//        {
//            if (array == null)
//            {
//                writer.WriteUShort(0);
//                return;
//            }
//            writer.WriteUShort((ushort)count);
//            writer.WriteBytes(array, offset, count);
//        }
//        #endregion

//        #region List
//        public static void WriteList(this ref NetDataWriter writer, IList list)
//        {
//            if (list == null)
//            {
//                writer.WriteUShort(0);
//                return;
//            }
//            var count = list.Count;
//            writer.WriteUShort((ushort)count);
//            for (int i = 0; i < count; i++)
//                WriteValue(ref writer, list[i]);
//        }
//        public static void WriteList<TValue>(this ref NetDataWriter writer, IList<TValue> list)
//        {
//            if (list == null)
//            {
//                writer.WriteUShort(0);
//                return;
//            }
//            var count = list.Count;
//            writer.WriteUShort((ushort)count);
//            for (int i = 0; i < count; i++)
//                WriteValue(ref writer, list[i]);
//        }
//        #endregion

//        #region Dictionary

//        public static void WriteDictionary<TKey, TValue>(this ref NetDataWriter writer, Dictionary<TKey, TValue> dict)
//        {
//            if (dict == null)
//            {
//                writer.WriteUShort(0);
//                return;
//            }
//            writer.WriteUShort((ushort)dict.Count);
//            foreach (var keyValuePair in dict)
//            {
//                WriteValue(ref writer, keyValuePair.Key);
//                WriteValue(ref writer, keyValuePair.Value);
//            }
//        }
//        #endregion

//    }
//}