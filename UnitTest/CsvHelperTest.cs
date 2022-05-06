using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCSharpLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace UnitTest
{
    [TestClass]
    public sealed class CsvHelperTest
    {
        static StreamReader StringToStreamReader(string s)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(s)), Encoding.UTF8);
        }

        [TestMethod]
        public void ReadLineTest1()
        {
            var reader = StringToStreamReader("a,b,c");
            var t = CsvHelper.ReadLine(reader);
            Assert.AreEqual(3, t.Count);
            Assert.AreEqual("a", t[0]);
            Assert.AreEqual("b", t[1]);
            Assert.AreEqual("c", t[2]);
            Assert.IsNull(CsvHelper.ReadLine(reader));
        }

        [TestMethod]
        public void ReadLinesTest1()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest2()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest3()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("\"\"\"a\",\"\"\"\"\"b\",\"c\"\"\",\"d\"\"\"\"\",\",e\",\"f,\",\"\"\",\"\"\",\"\"\"\"")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(8, t0.Count);
            Assert.AreEqual("\"a", t0[0]);
            Assert.AreEqual("\"\"b", t0[1]);
            Assert.AreEqual("c\"", t0[2]);
            Assert.AreEqual("d\"\"", t0[3]);
            Assert.AreEqual(",e", t0[4]);
            Assert.AreEqual("f,", t0[5]);
            Assert.AreEqual("\",\"", t0[6]);
            Assert.AreEqual("\"", t0[7]);
        }

        [TestMethod]
        public void ReadLinesTest4()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\n")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest5()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,\r\n")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest6()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\n")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest7()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,\n")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("", t0[2]);
        }

        [TestMethod]
        public void ReadLinesTest8()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\na,b,c")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(3, t1.Count);
            Assert.AreEqual("a", t1[0]);
            Assert.AreEqual("b", t1[1]);
            Assert.AreEqual("c", t1[2]);
        }

        [TestMethod]
        public void ReadLinesTest9()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\na")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual("a", t1[0]);
        }

        [TestMethod]
        public void ReadLinesTest10()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\n,")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(2, t1.Count);
            Assert.AreEqual("", t1[0]);
            Assert.AreEqual("", t1[1]);
        }

        [TestMethod]
        public void ReadLinesTest11()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\na,b,c\r\n")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(3, t1.Count);
            Assert.AreEqual("a", t1[0]);
            Assert.AreEqual("b", t1[1]);
            Assert.AreEqual("c", t1[2]);
        }

        [TestMethod]
        public void ReadLinesTest12()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\na\r\n")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(1, t1.Count);
            Assert.AreEqual("a", t1[0]);
        }

        [TestMethod]
        public void ReadLinesTest13()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("a,b,c\r\n,\r\n")).ToArray();
            Assert.AreEqual(2, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a", t0[0]);
            Assert.AreEqual("b", t0[1]);
            Assert.AreEqual("c", t0[2]);
            var t1 = t[1];
            Assert.AreEqual(2, t1.Count);
            Assert.AreEqual("", t1[0]);
            Assert.AreEqual("", t1[1]);
        }

        [TestMethod]
        public void ReadLinesTest14()
        {
            var t = CsvHelper.ReadLines(StringToStreamReader("\"a\r\nb\",\"\nc\",\"d\"\"\r\n\"")).ToArray();
            Assert.AreEqual(1, t.Length);
            var t0 = t[0];
            Assert.AreEqual(3, t0.Count);
            Assert.AreEqual("a\r\nb", t0[0]);
            Assert.AreEqual("\nc", t0[1]);
            Assert.AreEqual("d\"\r\n", t0[2]);
        }

        [TestMethod]
        public void EscapeValueTest1()
        {
            DateTime? value = null;
            Assert.AreEqual("\"\"", CsvHelper.EscapeValue(value));
        }

        [TestMethod]
        public void EscapeValueTest2()
        {
            DateTime? value = new DateTime(2020, 4, 16, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual("\"2020-04-16T00:00:00.0000000Z\"", CsvHelper.EscapeValue(value));
        }

        [TestMethod]
        public void ToStringAsCsvTest1()
        {
            var t = new string[] { "a", "\"b", "c\"", ",d", "e," };
            Assert.AreEqual("\"a\",\"\"\"b\",\"c\"\"\",\",d\",\"e,\"", CsvHelper.ToString(t));
        }

        [TestMethod]
        public void ToStringAsCsvTest2()
        {
            var t = new string[] { null, "a", null, "b", null };
            Assert.AreEqual("\"\",\"a\",\"\",\"b\",\"\"", CsvHelper.ToString(t));
        }

        public enum Enum1
        {
            A = 0,
            B = 1,
            C = 2
        }

        public class Class1
        {
            public int I { get; set; }
            public bool B { get; set; }
            public string S { get; set; }
            public decimal D { get; set; }
            public Guid G { get; set; }
            public DateTime DT { get; set; }
            public Enum1 E { get; set; }
        }

        public class Class2
        {
            public int? I { get; set; }
            public bool? B { get; set; }
            public string S { get; set; }
            public decimal? D { get; set; }
            public Guid? G { get; set; }
            public DateTime? DT { get; set; }
            public Enum1? E { get; set; }
        }

        [TestMethod]
        public void BindTest1()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new List<string> { "10", "True", "a", "1.1", "6e917ccc-ae71-479f-91b5-4c01be65e915", "2020-04-09T11:57:56.0075565Z", "C" };
            var actual = CsvHelper.Bind<Class1>(d, s);
            Assert.AreEqual(10, actual.I);
            Assert.AreEqual(true, actual.B);
            Assert.AreEqual("a", actual.S);
            Assert.AreEqual(1.1m, actual.D);
            Assert.AreEqual(Guid.Parse("6e917ccc-ae71-479f-91b5-4c01be65e915"), actual.G);
            Assert.AreEqual(DateTime.ParseExact("2020-04-09T11:57:56.0075565Z", "o", null), actual.DT);
            Assert.AreEqual(Enum1.C, actual.E);
        }

        [TestMethod]
        public void BindTest2()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new List<string> { "10", "True", "a", "1.1", "6e917ccc-ae71-479f-91b5-4c01be65e915", "2020-04-09T11:57:56.0075565Z", "C" };
            var actual = CsvHelper.Bind<Class2>(d, s);
            Assert.AreEqual(10, actual.I);
            Assert.AreEqual(true, actual.B);
            Assert.AreEqual("a", actual.S);
            Assert.AreEqual(1.1m, actual.D);
            Assert.AreEqual(Guid.Parse("6e917ccc-ae71-479f-91b5-4c01be65e915"), actual.G);
            Assert.AreEqual(DateTime.ParseExact("2020-04-09T11:57:56.0075565Z", "o", null), actual.DT);
            Assert.AreEqual(Enum1.C, actual.E);
        }

        [TestMethod]
        public void BindTest3()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new List<string> { "", "", "", "", "", "", "" };
            var actual = CsvHelper.Bind<Class2>(d, s);
            Assert.AreEqual(null, actual.I);
            Assert.AreEqual(null, actual.B);
            Assert.AreEqual("", actual.S);
            Assert.AreEqual(null, actual.D);
            Assert.AreEqual(null, actual.G);
            Assert.AreEqual(null, actual.DT);
            Assert.AreEqual(null, actual.E);
        }

        [TestMethod]
        public void DebindTest1()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new Class1()
            {
                I = 10,
                B = true,
                S = "a",
                D = 1.1m,
                G = Guid.Parse("6e917ccc-ae71-479f-91b5-4c01be65e915"),
                DT = DateTime.ParseExact("2020-04-09T11:57:56.0075565Z", "o", null, DateTimeStyles.RoundtripKind),
                E = Enum1.C
            };

            var actual = CsvHelper.Debind(d, s);
            Assert.AreEqual("10", actual[0]);
            Assert.AreEqual("True", actual[1]);
            Assert.AreEqual("a", actual[2]);
            Assert.AreEqual("1.1", actual[3]);
            Assert.AreEqual("6e917ccc-ae71-479f-91b5-4c01be65e915", actual[4]);
            Assert.AreEqual("2020-04-09T11:57:56.0075565Z", actual[5]);
            Assert.AreEqual("C", actual[6]);
        }

        [TestMethod]
        public void DebindTest2()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new Class2()
            {
                I = 10,
                B = true,
                S = "a",
                D = 1.1m,
                G = Guid.Parse("6e917ccc-ae71-479f-91b5-4c01be65e915"),
                DT = DateTime.ParseExact("2020-04-09T11:57:56.0075565Z", "o", null, DateTimeStyles.RoundtripKind),
                E = Enum1.C
            };

            var actual = CsvHelper.Debind(d, s);
            Assert.AreEqual("10", actual[0]);
            Assert.AreEqual("True", actual[1]);
            Assert.AreEqual("a", actual[2]);
            Assert.AreEqual("1.1", actual[3]);
            Assert.AreEqual("6e917ccc-ae71-479f-91b5-4c01be65e915", actual[4]);
            Assert.AreEqual("2020-04-09T11:57:56.0075565Z", actual[5]);
            Assert.AreEqual("C", actual[6]);
        }

        [TestMethod]
        public void DebindTest3()
        {
            var d = new Dictionary<string, int>() {
                { "I", 0 },
                { "B", 1 },
                { "S", 2 },
                { "D", 3 },
                { "G", 4 },
                { "DT", 5 },
                { "E", 6 },
            };
            var s = new Class2()
            {
                I = null,
                B = null,
                S = null,
                D = null,
                G = null,
                DT = null,
                E = null
            };

            var actual = CsvHelper.Debind(d, s);
            Assert.AreEqual("", actual[0]);
            Assert.AreEqual("", actual[1]);
            Assert.AreEqual("", actual[2]);
            Assert.AreEqual("", actual[3]);
            Assert.AreEqual("", actual[4]);
            Assert.AreEqual("", actual[5]);
            Assert.AreEqual("", actual[6]);
        }

        [TestMethod]
        public void SaveTest1()
        {
            var s = new Class1()
            {
                I = 10,
                B = true,
                S = "a\r\nb",
                D = 1.1m,
                G = Guid.Parse("6e917ccc-ae71-479f-91b5-4c01be65e915"),
                DT = DateTime.ParseExact("2020-04-09T11:57:56.0075565Z", "o", null, DateTimeStyles.RoundtripKind),
                E = Enum1.C
            };

            using (var ms = new MemoryStream())
            {
                var expected = "\uFEFF\"I\",\"B\",\"S\",\"D\",\"G\",\"DT\",\"E\"\r\n\"10\",\"True\",\"a\r\nb\",\"1.1\",\"6e917ccc-ae71-479f-91b5-4c01be65e915\",\"2020-04-09T11:57:56.0075565Z\",\"C\"\r\n";
                CsvHelper.Save(ms, new Class1[] { s });
                var actual = Encoding.UTF8.GetString(ms.ToArray());
                Assert.AreEqual(expected, actual);
            }
        }

        public class Class3
        {
            [ColumnName("Id")]
            public int A { get; set; }
            [ColumnName("Message")]
            public string B { get; set; }
        }

        [TestMethod]
        public void SaveTest2()
        {
            var s = new Class3()
            {
                A = 1,
                B = "hello",
            };

            using (var ms = new MemoryStream())
            {
                var expected = "\uFEFF\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n";
                CsvHelper.Save(ms, new Class3[] { s });
                var actual = Encoding.UTF8.GetString(ms.ToArray());
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void SaveTest3()
        {
            var s = new Class3()
            {
                A = 1,
                B = "hello",
            };

            var tempFileName = Path.GetTempFileName();
            try
            {
                CsvHelper.Save(tempFileName, new Class3[] { s });
                var expected = "\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n";
                var actual = File.ReadAllText(tempFileName);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void SaveTest4()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                using (var writer = CsvHelper.GetWriter<Class3>(tempFileName))
                {
                    writer.Write(new Class3()
                    {
                        A = 1,
                        B = "hello",
                    });
                }
                using (var writer = CsvHelper.GetWriter<Class3>(tempFileName, true))
                {
                    writer.Write(new Class3()
                    {
                        A = 2,
                        B = "world",
                    });
                }
                var expected = "\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n\"2\",\"world\"\r\n";
                var actual = File.ReadAllText(tempFileName);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void SaveTest5()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                CsvHelper.Save(tempFileName, new Class3[] {
                    new Class3()
                    {
                        A = 1,
                        B = "hello",
                    },
                    new Class3()
                    {
                        A = 2,
                        B = "world",
                    }
                });
                var expected = "\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n\"2\",\"world\"\r\n";
                var actual = File.ReadAllText(tempFileName);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void SaveTest6()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                CsvHelper.Save(tempFileName, new Class3[] { });
                var expected = "\"Id\",\"Message\"\r\n";
                var actual = File.ReadAllText(tempFileName);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        public class Class4
        {
            public int A { get; set; }
            [IgnoreDataMember]
            public string B { get; set; }
        }

        [TestMethod]
        public void SaveTest7()
        {
            var s = new Class4()
            {
                A = 1,
                B = "hello",
            };

            var tempFileName = Path.GetTempFileName();
            try
            {
                CsvHelper.Save(tempFileName, new Class4[] { s });
                var expected = "\"A\"\r\n\"1\"\r\n";
                var actual = File.ReadAllText(tempFileName);
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void LoadTest1()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n")))
            {
                var expected = new Class3()
                {
                    A = 1,
                    B = "hello",
                };
                var actual = CsvHelper.Load<Class3>(ms).First();
                Assert.AreEqual(expected.A, actual.A);
                Assert.AreEqual(expected.B, actual.B);
            }
        }

        [TestMethod]
        public void LoadTest2()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("\"Message\",\"Id\"\r\n\"hello\",\"1\"\r\n")))
            {
                var expected = new Class3()
                {
                    A = 1,
                    B = "hello",
                };
                var actual = CsvHelper.Load<Class3>(ms).First();
                Assert.AreEqual(expected.A, actual.A);
                Assert.AreEqual(expected.B, actual.B);
            }
        }

        [TestMethod]
        public void LoadTest3()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("\"Id\",\"Message\",\"Reserved\"\r\n\"1\",\"hello\",\"\"\r\n")))
            {
                var expected = new Class3()
                {
                    A = 1,
                    B = "hello",
                };
                var actual = CsvHelper.Load<Class3>(ms).First();
                Assert.AreEqual(expected.A, actual.A);
                Assert.AreEqual(expected.B, actual.B);
            }
        }

        [TestMethod]
        public void LoadTest4()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFileName, "\"Id\",\"Message\"\r\n\"1\",\"hello\"\r\n");
                var instance3 = CsvHelper.Load<Class3>(tempFileName).Single();
                Assert.AreEqual(1, instance3.A);
                Assert.AreEqual("hello", instance3.B);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void LoadTest5()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFileName, "\"Id\",\"Message\"\r\n");
                var instance3s = CsvHelper.Load<Class3>(tempFileName).ToList();
                Assert.AreEqual(0, instance3s.Count);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void LoadTest6()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFileName, "\"Id\",\"Message\"");
                var instance3s = CsvHelper.Load<Class3>(tempFileName).ToList();
                Assert.AreEqual(0, instance3s.Count);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }

        [TestMethod]
        public void LoadTest7()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFileName, "\"A\",\"B\"\r\n\"1\",\"hello\"\r\n");
                var instance4 = CsvHelper.Load<Class4>(tempFileName).Single();
                Assert.AreEqual(1, instance4.A);
                Assert.AreEqual(null, instance4.B);
            }
            finally
            {
                if (File.Exists(tempFileName)) File.Delete(tempFileName);
            }
        }
    }
}
