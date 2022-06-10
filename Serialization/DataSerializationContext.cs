using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Poly.Serialization
{
    public delegate void PolyWriteDelegate(ref DataWriter writer, object value, Type type);
    public delegate object PolyReadDelegate(ref DataReader reader, Type type);
    public interface IPolySerialzationHandler
    {
        void Write(ref DataWriter writer, object value, Type type);
        object Read(ref DataReader reader, Type type);
        bool IsSupportType(Type type);
    }
    public class PolySerialzationFuncHandler : IPolySerialzationHandler
    {
        private PolyWriteDelegate writeDelegate;
        private PolyReadDelegate readDelegate;

        public Type Type { get; }

        public PolySerialzationFuncHandler(Type type, PolyWriteDelegate writeDelegate, PolyReadDelegate readDelegate)
        {
            Type = type;
            this.writeDelegate = writeDelegate;
            this.readDelegate = readDelegate;
        }
        public object Read(ref DataReader reader, Type type)
        {
            return readDelegate(ref reader, type);
        }
        public void Write(ref DataWriter writer, object value, Type type)
        {
            writeDelegate(ref writer, value, type);
        }
        public bool IsSupportType(Type type)
        {
            return type == Type || Type.IsAssignableFrom(type);
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DataFormattableAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DataIndexAttribute : Attribute
    {
        internal int index;
        public DataIndexAttribute(int index)
        {
            this.index = index;
        }
    }
    public class PolyFormattableTypeInfo
    {
        internal Type type;
        internal PropertyInfo[] propertyInfos;
        internal int propertyInfoCount;

        internal PolyFormattableTypeInfo(Type type)
        {
            this.type = type;
        }
        public void AddPropertyInfo(PropertyInfo propertyInfo, int index)
        {
            if (propertyInfos == null) propertyInfos = new PropertyInfo[8];
            if (index == propertyInfos.Length) Array.Resize(ref propertyInfos, index << 1);
            propertyInfos[index] = propertyInfo;
            if (index >= propertyInfoCount) propertyInfoCount = index + 1;
        }
        public PropertyInfo GetPropertyInfo(int index)
        {
            if (index >= propertyInfoCount) return null;
            return propertyInfos[index];
        }
    }
    public interface IPolySerializationContext
    {
        void RegisterType<T>(ushort typeId = 0);
        Type GetTypeById(ushort id);

        void RegisterObjectCreator(Type type, Func<object> createFunc);
        void UnregisterObjectCreator(Type type);
        Func<object> GetObjectCreator(Type type);

        void RegisterSerialzationHandler(string id, IPolySerialzationHandler serialzationHandler);
        void UnregisterSerialzationHandler(string id);
        IPolySerialzationHandler GetSerialzationHandler(Type type);

        //cache Type mapping
        PolyFormattableTypeInfo RegisterFormattableType(Type type);
        PolyFormattableTypeInfo GetFormattableTypeInfo(Type type);
    }

    public static class PolySerializerContextExtensions
    {
        public static void RegisterSerialzationHandler(this IPolySerializationContext context, string id, Type type, PolyWriteDelegate writeDelegate, PolyReadDelegate readDelegate)
        {
            var handler = new PolySerialzationFuncHandler(type, writeDelegate, readDelegate);
            context.RegisterSerialzationHandler(id, handler);
        }
    }

    public class DataSerializationContext : IPolySerializationContext
    {
        private static DataSerializationContext defaultContext = null;
        public static DataSerializationContext DefaultContext
        {
            get
            {
                if (defaultContext == null)
                {
                    defaultContext = new DataSerializationContext();
                }
                return defaultContext;
            }
        }

        protected Dictionary<ushort, Type> idTypeDict = new Dictionary<ushort, Type>();
        protected Dictionary<Type, ushort> typeIdDict = new Dictionary<Type, ushort>();
        protected ushort maxTypeId = 10;
        protected Dictionary<Type, Func<object>> typeCreatorDict = new Dictionary<Type, Func<object>>();

        public DataSerializationContext()
        {
        }

        public Type GetTypeById(ushort id)
        {
            idTypeDict.TryGetValue(id, out var type);
            return type;
        }
        public void RegisterType<T>(ushort typeId = 0)
        {
            if (typeId == 0)
                typeId = maxTypeId++;
            else
                maxTypeId = Math.Max(maxTypeId, typeId);
            var type = typeof(T);

            if (idTypeDict.ContainsKey(typeId))
            {
                Console.Error.WriteLine($"ObjectSerializer.RegisterType: {type.Name}, {type}, {typeId} exsit!!");
                return;
            }
            // Debug.Log($"ObjectSerializer.Init: {type.Name}, {type}, {typeId}");
            idTypeDict.Add(typeId, type);
            typeIdDict.Add(type, typeId);
        }
        public void RegisterObjectCreator(Type type, Func<object> createFunc)
        {
            typeCreatorDict.Add(type, createFunc);
        }
        public void UnregisterObjectCreator(Type type)
        {
            typeCreatorDict.Remove(type);
        }
        public Func<object> GetObjectCreator(Type type)
        {
            typeCreatorDict.TryGetValue(type, out var creator);
            return creator;
        }

        //protected Dictionary<Type, ISerialzationHandler> serializationHandlerDict = new Dictionary<Type, ISerialzationHandler>();
        protected SortedList<string, IPolySerialzationHandler> serializationHandlerSList = new SortedList<string, IPolySerialzationHandler>();
        public void RegisterSerialzationHandler(string id, IPolySerialzationHandler serialzationHandler)
        {
            serializationHandlerSList[id] = serialzationHandler;
            //serializationHandlerDict[type] = serialzationHandler;
        }
        public void UnregisterSerialzationHandler(string id)
        {
            serializationHandlerSList.Remove(id);
            //serializationHandlerDict.Remove(type);
        }
        public IPolySerialzationHandler GetSerialzationHandler(Type type)
        {
            var list = serializationHandlerSList.Values;
            var count = list.Count;
            for (int i = 0; i < count; i++)
            {
                var handler = list[i];
                if (handler.IsSupportType(type))
                    return handler;
            }
            //serializationHandlerDict.TryGetValue(type, out var handler);
            return null;
        }

        private Dictionary<Type, PolyFormattableTypeInfo> serializableTypeInfoDict = new Dictionary<Type, PolyFormattableTypeInfo>();
        public PolyFormattableTypeInfo RegisterFormattableType(Type type)
        {
            var attr = type.GetCustomAttribute<DataFormattableAttribute>();
            if (attr == null) return null;
            var info = new PolyFormattableTypeInfo(type);
            var propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var propertyInfo in propertyInfos)
            {
                var indexAttr = propertyInfo.GetCustomAttribute<DataIndexAttribute>();
                if (indexAttr != null)
                {
                    info.AddPropertyInfo(propertyInfo, indexAttr.index);
                }
            }
            serializableTypeInfoDict.Add(type, info);
            return info;
        }
        public PolyFormattableTypeInfo GetFormattableTypeInfo(Type type)
        {
            if (serializableTypeInfoDict.TryGetValue(type, out var info))
                return info;
            return RegisterFormattableType(type);
        }
    }
}