using System.Security;
using NUnit.Framework;

namespace System.FrameworkExtensions.Tests
{
    [TestFixture]
    public sealed class StringExtensionsTest
    {
        [Test]
        public void SecureNullString()
        {
            Assert.IsNull(((string) null).ToSecureString());
        }

        [Test]
        public void InsecureNullString()
        {
            Assert.IsNull(((SecureString) null).ToInsecureString());
        }

        [Test]
        public void EmptyStringCheck()
        {
            var secure = string.Empty.ToSecureString();
            Assert.NotNull(secure);

            var insecure = secure.ToInsecureString();
            Assert.AreEqual(string.Empty, insecure);
        }

        [Test]
        public void GenericStringCheck()
        {
            var str = "one two three";
            var secure = str.ToSecureString();
            Assert.NotNull(secure);

            var insecure = secure.ToInsecureString();
            Assert.AreEqual(str, insecure);
        }
    }
}