using System.Runtime.Serialization.Json;

namespace MyCSharpLibrary
{
    public static class JsonHelper
    {
        static readonly DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true };

        public static string ToJson<T>(T obj)
        {
            return DataContractHelper.Serialize(new DataContractJsonSerializer(typeof(T), settings), obj);
        }

        public static T FromJson<T>(string json)
        {
            return DataContractHelper.Deserialize<T>(new DataContractJsonSerializer(typeof(T), settings), json);
        }
    }
}
