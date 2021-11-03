using System;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests.Validation
{
    internal class NullArgumentTests
    {
        [Test]
        public void NullArgumentToExecuteMethodThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => CommandLine.Execute(args: null));
        }
    }
}