using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCSharpLibrary;

namespace UnitTest
{
    public class Class1
    {
        public int I { get; set; }
        public double D { get; set; }
        public string S { get; set; }
        public bool B { get; set; }
        public int[] A { get; set; }
        public object N { get; set; }
    }

    [TestClass]
    public sealed class JsonHelperTest
    {
        [TestMethod]
        public void ToJsonFromJsonTest1()
        {
            var expected = new Class1()
            {
                I = 100,
                D = 2.0,
                S = "foo",
                B = true,
                A = new int[] { 1, 2, 3 },
                N = null
            };
            var actual = JsonHelper.FromJson<Class1>(JsonHelper.ToJson(expected));
            Assert.AreEqual(expected.I, actual.I);
            Assert.AreEqual(expected.D, actual.D);
            Assert.AreEqual(expected.S, actual.S);
            Assert.AreEqual(expected.B, actual.B);
            Assert.AreEqual(expected.A.Length, actual.A.Length);
            Assert.AreEqual(expected.A[0], actual.A[0]);
            Assert.AreEqual(expected.A[1], actual.A[1]);
            Assert.AreEqual(expected.A[2], actual.A[2]);
            Assert.AreEqual(expected.N, actual.N);
        }
    }
}
