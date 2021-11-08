using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using System.Text.Json;

namespace Richiban.Cmdr.Generator.Tests
{
    [TestFixture]
    class TransformerTests
    {
        [Test]
        public void Test1()
        {
            var models = new MethodModel[]
            {
                new MethodModel(
                    "SomeFunction",
                    new[] { "shortcut" },
                    new[] { "SomeParent" },
                    "SomeClass",
                    new ArgumentModel[] { })
            };

            var sut = new CommandModelTransformer();

            var actual = sut.Transform(models);

            Console.WriteLine(JsonSerializer.Serialize(actual));
        }
    }
}