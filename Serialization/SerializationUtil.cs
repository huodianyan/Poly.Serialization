using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Poly.Serialization
{
    public static class SerializationUtil
    {
        //static void Initialize()
        //{
        //    JsonConvert.DefaultSettings = () =>
        //    {
        //        return SerializationUtil.DefaultJsonSerializerSettings;
        //    };
        //    Debug.Log("SerializerUtil.Initialize!");
        //}

        //private static JsonSerializerSettings defaultJsonSerializerSettings;
        //public static JsonSerializerSettings DefaultJsonSerializerSettings
        //{
        //    get
        //    {
        //        if (defaultJsonSerializerSettings == null)
        //        {
        //            defaultJsonSerializerSettings = new UnityJsonSerializerSettings();
        //        }
        //        return defaultJsonSerializerSettings;
        //    }
        //}


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPolyFormattable(this Type type) => type.GetCustomAttribute<DataFormattableAttribute>() != null;
    }

    //public class UnityJsonSerializerSettings : JsonSerializerSettings
    //{
    //    public UnityJsonSerializerSettings()
    //    {
    //        var converterList = new List<JsonConverter>();
    //        converterList.Add(new UnityJsonConverter());
    //        // converterList.Add(new GameObjectJsonConverter());
    //        // converterList.Add(new EntityJsonConverter());

    //        // TypeNameHandling = TypeNameHandling.Auto;
    //        // Formatting = Formatting.None;
    //        Formatting = Formatting.Indented;
    //        Converters = converterList;
    //        ContractResolver = new UnityContractResolver();
    //    }
    //}
    //public class UnityContractResolver : DefaultContractResolver
    //{
    //    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    //    {
    //        var properties = base.CreateProperties(type, memberSerialization);
    //        // Only filter class that is derived from MonoBehaviour
    //        if (type.IsSubclassOf(typeof(MonoBehaviour)))
    //        {
    //            // Keep name property OR properties derived from MonoBehaviour
    //            // properties = properties.Where(x => x.PropertyName.Equals("name") || x.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))).ToList();
    //            // properties = properties.Where(x => x.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))).ToList();
    //            properties = properties.Where(x => x.PropertyName.Equals("enabled") || x.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))).ToList();
    //        }
    //        return properties;
    //    }
    //}
}