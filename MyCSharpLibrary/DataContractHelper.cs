using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace MyCSharpLibrary
{
    public static class DataContractHelper
    {
        public static string Serialize<T>(XmlObjectSerializer serializer, T obj)
        {
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, obj);
                stream.Position = 0;
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public static T Deserialize<T>(XmlObjectSerializer serializer, string s)
        {
            using (var stream = new MemoryStream())
            {
                var sBytes = Encoding.UTF8.GetBytes(s);
                stream.Write(sBytes, 0, sBytes.Length);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
