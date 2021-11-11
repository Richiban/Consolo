using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using Shouldly;

namespace Richiban.Cmdr.Tests
{
    [TestFixture]
    class TransformerTests
    {
        [Test]
        public void TestLeafComandWithProvidedNameInCommandGroup()
        {
            var models = new[]
            {
                new MethodModel(
                    methodName: "SomeFunction",
                    providedName: "shortcut",
                    groupCommandPath: new[] { "SomeParent" },
                    fullyQualifiedClassName: "SomeNamespace.SomeClass",
                    arguments: new ArgumentModel[] { })
            };

            var sut = new CommandModelTransformer();

            var actual = sut.Transform(models);

            actual.ShouldBeOfType<CommandModel.RootCommandModel>();

            var group = actual.SubCommands.ShouldHaveSingleItem()
                .ShouldBeOfType<CommandModel.CommandGroupModel>();

            group.CommandName.ShouldBe("some-parent");

            var leaf = group.SubCommands.ShouldHaveSingleItem()
                .ShouldBeOfType<CommandModel.LeafCommandModel>();

            leaf.CommandName.ShouldBe("shortcut");
            leaf.VariableName.ShouldBe("someFunctionCommand");
            leaf.FullyQualifiedName.ShouldBe("SomeNamespace.SomeClass.SomeFunction");
            leaf.Parameters.ShouldBeEmpty();
        }
    }
}