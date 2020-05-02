using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyCSharpLibrary;
using System.Text;

namespace UnitTest
{
    [TestClass]
    public sealed class CipherHelperTest
    {
        [TestMethod]
        public void EncryptDecryptTest1()
        {
            var cipherText = CipherHelper.Encrypt(Encoding.UTF8.GetBytes("test"), "passw0rd");
            var actual = Encoding.UTF8.GetString(CipherHelper.Decrypt(cipherText, "passw0rd"));
            Assert.AreEqual("test", actual);
        }
    }
}
