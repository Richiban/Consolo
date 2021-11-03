using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Richiban.CommandLine.Tests.Validation
{
    class NullArgumentTests
    {
        [Test]
        public void NullArgumentToExecuteMethodThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => CommandLine.Execute(args: null));
        }
    }
}
