using NUnit.Framework;

namespace Signal.Core.Tests
{
    public sealed class CoreInfoTests
    {
        [Test]
        public void NameIsSignalCore()
        {
            Assert.That(CoreInfo.Name, Is.EqualTo("Signal.Core"));
        }
    }
}
