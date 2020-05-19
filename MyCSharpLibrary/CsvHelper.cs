using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MyCSharpLibrary
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ColumnNameAttribute : Attribute
    {
        public string Name { get; private set; }
        public ColumnNameAttribute(string name) { Name = name; }
    }

    public static class CsvHelper
    {
        public static List<string> ReadLine(StreamReader reader)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var quoted = false;
            while (true)
            {
                while (true)
                {
                    var c = reader.Read();
                    switch (c)
                    {
                        case '"':
                            if (quoted && reader.Peek() == '"')
                            {
                                sb.Append('"');
                                reader.Read();
                            }
                            else
                            {
                                quoted = !quoted;
                            }
                            break;
                        case ',':
                            if (quoted) goto default;
                            goto ValueEnd;
                        case '\r':
                            if (quoted) goto default;
                            if (reader.Peek() == '\n')
                            {
                                reader.Read();
                            }
                            goto LineEnd;
                        case '\n':
                            if (quoted) goto default;
                            goto LineEnd;
                        case -1:
                            if (quoted) throw new ApplicationException("Unexpected end of stream.");
                            goto LineEnd;
                        default:
                            sb.Append((char)c);
                            break;
                    }
                }
            ValueEnd:
                result.Add(sb.ToString());
                sb.Clear();
            }
        LineEnd:
            result.Add(sb.ToString());
            return result;
        }

        public static IEnumerable<List<string>> ReadLines(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return ReadLine(reader);
            }
        }

        public static T Bind<T>(Dictionary<string, int> headerMap, List<string> values)
            where T : class, new()
        {
            var result = new T();
            foreach (var p in typeof(T).GetProperties())
            {
                if (!headerMap.ContainsKey(p.Name)) continue;
                var value = values[headerMap[p.Name]];

                var type = Nullable.GetUnderlyingType(p.PropertyType);
                if (type == null)
                {
                    type = p.PropertyType;
                }
                else
                {
                    if (value == "")
                    {
                        p.SetValue(result, null);
                        continue;
                    }
                }

                if (type == typeof(int))
                {
                    p.SetValue(result, int.Parse(value));
                }
                else if (type == typeof(bool))
                {
                    p.SetValue(result, bool.Parse(value));
                }
                else if (type == typeof(string))
                {
                    p.SetValue(result, value);
                }
                else if (type == typeof(decimal))
                {
                    p.SetValue(result, decimal.Parse(value));
                }
                else if (type == typeof(Guid))
                {
                    p.SetValue(result, Guid.Parse(value));
                }
                else if (type == typeof(DateTime))
                {
                    if (!DateTime.TryParseExact(value, "o", null, DateTimeStyles.RoundtripKind, out var _))
                    {
                        value = DateTime.Parse(value).ToString("o");
                    }
                    p.SetValue(result, DateTime.ParseExact(value, "o", null, DateTimeStyles.RoundtripKind));
                }
                else
                {
                    throw new ApplicationException($"Unsupported type: {typeof(T).Name}.");
                }
            }
            return result;
        }

        public static IEnumerable<T> Load<T>(Stream stream, Encoding encoding = null)
            where T : class, new()
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var reader = new StreamReader(stream, encoding))
            {
                var header = ReadLine(reader);
                var columnToPropertyMap = typeof(T).GetProperties().Select(e =>
                {
                    var a = Attribute.GetCustomAttributes(e, typeof(ColumnNameAttribute));
                    if (a.Length == 0)
                    {
                        return new KeyValuePair<string, string>(e.Name, e.Name);
                    }
                    else
                    {
                        return new KeyValuePair<string, string>((a[0] as ColumnNameAttribute).Name, e.Name);
                    }
                }).ToDictionary(e => e.Key, e => e.Value);

                var headerMap = new Dictionary<string, int>();
                for (var i = 0; i < header.Count; i++)
                {
                    if (columnToPropertyMap.ContainsKey(header[i]))
                    {
                        headerMap[columnToPropertyMap[header[i]]] = i;
                    }
                    else
                    {
                        headerMap[header[i]] = i;
                    }
                }

                foreach (var values in ReadLines(reader))
                {
                    yield return Bind<T>(headerMap, values);
                }
            }
        }

        public static IEnumerable<T> Load<T>(string path, Encoding encoding = null)
            where T : class, new()
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Load<T>(stream, encoding);
            }
        }

        public static string EscapeValue(object o, string dateTimeFormat = "o")
        {
            if (o == null) return "\"\"";
            if (o.GetType() == typeof(DateTime))
            {
                return $"\"{((DateTime)o).ToString(dateTimeFormat)}\"";
            }
            if (o.GetType() == typeof(DateTimeOffset))
            {
                return $"\"{((DateTimeOffset)o).ToString(dateTimeFormat)}\"";
            }
            return $"\"{o.ToString().Replace("\"", "\"\"")}\"";
        }

        public static string ToString(IEnumerable<object> values)
        {
            return string.Join(",", values.Select(e => EscapeValue(e)));
        }

        public static string[] Debind<T>(Dictionary<string, int> headerMap, T obj)
            where T : class
        {
            var result = new string[headerMap.Count];
            foreach (var p in typeof(T).GetProperties())
            {
                if (!headerMap.ContainsKey(p.Name)) continue;
                var i = headerMap[p.Name];

                var type = Nullable.GetUnderlyingType(p.PropertyType);
                if (type == null)
                {
                    type = p.PropertyType;
                }
                else
                {
                    if (p.GetValue(obj) == null)
                    {
                        result[i] = "";
                        continue;
                    }
                }

                if (type == typeof(DateTime))
                {
                    result[i] = ((DateTime)p.GetValue(obj)).ToString("o");
                }
                else
                {
                    result[i] = (p.GetValue(obj) ?? "").ToString();
                }
            }
            return result;
        }

        public static void Save<T>(Stream stream, IEnumerable<T> content, Encoding encoding = null)
            where T : class
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var writer = new StreamWriter(stream, encoding))
            {
                var headerLine = typeof(T).GetProperties().Select(e =>
                {
                    var a = Attribute.GetCustomAttributes(e, typeof(ColumnNameAttribute));
                    if (a.Length == 0)
                    {
                        return e.Name;
                    }
                    else
                    {
                        return (a[0] as ColumnNameAttribute).Name;
                    }
                }).ToList();
                writer.WriteLine(ToString(headerLine));

                var propertyNames = typeof(T).GetProperties().Select(e => e.Name).ToList();
                var headerMap = new Dictionary<string, int>();
                for (var i = 0; i < propertyNames.Count; i++)
                {
                    headerMap[propertyNames[i]] = i;
                }
                foreach (var o in content)
                {
                    writer.WriteLine(ToString(Debind(headerMap, o)));
                }
            }
        }

        public static void Save<T>(string path, IEnumerable<T> content, Encoding encoding = null)
            where T : class
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Save(stream, content, encoding);
            }
        }
    }
}
