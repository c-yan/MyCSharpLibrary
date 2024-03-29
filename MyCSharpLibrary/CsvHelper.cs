﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MyCSharpLibrary
{
    public interface ICsvWriter<T> : IDisposable
        where T : class
    {
        void Write(T o);
        void Close();
    }

    public static class CsvHelper
    {
        private sealed class CsvWriter<T> : ICsvWriter<T>
            where T : class
        {
            StreamWriter Writer;
            readonly Dictionary<string, int> NameToIndexMap;

            internal CsvWriter(Stream stream, bool appending = false, Encoding encoding = null)
            {
                NameToIndexMap = typeof(T).GetProperties()
                    .Where(e => e.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Length == 0)
                    .Select((e, i) => new KeyValuePair<string, int>(e.Name, i))
                    .ToDictionary(e => e.Key, e => e.Value);

                if (encoding == null)
                {
                    if (appending) encoding = new UTF8Encoding(false, true);
                    else encoding = Encoding.UTF8;
                }
                Writer = new StreamWriter(stream, encoding);
                if (!appending) WriteHeader();
            }

            void WriteHeader()
            {
                var headerLine = typeof(T).GetProperties()
                    .Where(e => e.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Length == 0)
                    .Select(e =>
                    {
                        var a = e.GetCustomAttributes(typeof(DataMemberAttribute), true)
                            .Cast<DataMemberAttribute>()
                            .SingleOrDefault();
                        if (a == null || a.Name == null)
                        {
                            return e.Name;
                        }

                        return a.Name;
                    }).ToList();
                Writer.WriteLine(CsvHelper.ToString(headerLine));
            }

            public void Write(T o)
            {
                Writer.WriteLine(CsvHelper.ToString(Debind(NameToIndexMap, o)));
            }

            public void Dispose()
            {
                Writer?.Close();
                Writer = null;
            }

            public void Close()
            {
                Dispose();
            }
        }

        public static List<string> ReadLine(TextReader reader)
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
            var s = sb.ToString();
            if (result.Count == 0 && s == "") return null;
            result.Add(s);
            return result;
        }

        public static IEnumerable<List<string>> ReadLines(TextReader reader)
        {
            while (true)
            {
                var result = ReadLine(reader);
                if (result == null) break;
                yield return result;
            }
        }

        public static T Bind<T>(Dictionary<string, int> nameToIndexMap, List<string> values)
            where T : class, new()
        {
            var result = new T();
            foreach (var p in typeof(T).GetProperties())
            {
                if (!nameToIndexMap.ContainsKey(p.Name)) continue;

                var value = values[nameToIndexMap[p.Name]];

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
                else if (type.IsEnum)
                {
                    p.SetValue(result, Enum.Parse(type, value));
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
            var columnToPropertyMap = typeof(T).GetProperties()
                .Where(e => e.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Length == 0)
                .Select(e =>
                {
                    var a = e.GetCustomAttributes(typeof(DataMemberAttribute), true)
                        .Cast<DataMemberAttribute>()
                        .SingleOrDefault();
                    if (a == null || a.Name == null)
                    {
                        return new KeyValuePair<string, string>(e.Name, e.Name);
                    }

                    return new KeyValuePair<string, string>(a.Name, e.Name);
                }).ToDictionary(e => e.Key, e => e.Value);

            if (encoding == null) encoding = Encoding.UTF8;
            using (var reader = new StreamReader(stream, encoding))
            {
                var nameToIndexMap = new Dictionary<string, int>();
                var header = ReadLine(reader);
                for (var i = 0; i < header.Count; i++)
                {
                    if (!columnToPropertyMap.ContainsKey(header[i])) continue;

                    nameToIndexMap[columnToPropertyMap[header[i]]] = i;
                }

                foreach (var values in ReadLines(reader))
                {
                    yield return Bind<T>(nameToIndexMap, values);
                }
            }
        }

        public static IEnumerable<T> Load<T>(string path, Encoding encoding = null)
            where T : class, new()
        {
            return Load<T>(new FileStream(path, FileMode.Open), encoding);
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

        public static string[] Debind<T>(Dictionary<string, int> nameToIndexMap, T obj)
            where T : class
        {
            var result = new string[nameToIndexMap.Count];
            foreach (var p in typeof(T).GetProperties())
            {
                if (!nameToIndexMap.ContainsKey(p.Name)) continue;

                var i = nameToIndexMap[p.Name];

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

        public static ICsvWriter<T> GetWriter<T>(Stream stream, bool appending = false, Encoding encoding = null)
            where T : class
        {
            return new CsvWriter<T>(stream, appending, encoding);
        }

        public static ICsvWriter<T> GetWriter<T>(string path, bool appending = false, Encoding encoding = null)
            where T : class
        {
            if (appending) return GetWriter<T>(new FileStream(path, FileMode.Append), appending, encoding);
            else return GetWriter<T>(new FileStream(path, FileMode.Create), appending, encoding);
        }

        public static void Save<T>(Stream stream, IEnumerable<T> content, Encoding encoding = null)
            where T : class
        {
            using (var writer = GetWriter<T>(stream, false, encoding))
            {
                foreach (var o in content)
                {
                    writer.Write(o);
                }
            }
        }

        public static void Save<T>(string path, IEnumerable<T> content, Encoding encoding = null)
            where T : class
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Save(stream, content, encoding);
            }
        }
    }
}
