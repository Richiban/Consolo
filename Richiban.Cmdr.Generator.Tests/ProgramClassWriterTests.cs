using System;
using NUnit.Framework;
using Richiban.Cmdr.Generators;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr.Generator.Tests
{
    [TestFixture]
    public class ProgramClassWriterTests
    {
        [Test]
        public void BasicTest()
        {
            var methods = new[]
            {
                new MethodModel(
                    "SomeFunction",
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "SomeClass",
                    Array.Empty<ArgumentModel>())
            };

            var codeGenerator = new ProgramClassCodeGenerator(methods);

            var actual = String.Join("\n", codeGenerator.GetCodeLines());

            Console.WriteLine(actual);

            Assert.Inconclusive();
        }
    }
}