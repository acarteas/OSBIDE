using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace OSBIDE.Web.Helpers
{
    public static class JsonSerializerExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj, Formatting.None, GetJsonSerializerSettings());
        }

        public static T FromJson<T>(this string obj)
        {
            return string.IsNullOrEmpty(obj) ? default(T) : JsonConvert.DeserializeObject<T>(obj, GetJsonSerializerSettings());
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }
    }
}