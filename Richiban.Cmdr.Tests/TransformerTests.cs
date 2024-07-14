// using System;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using System.Xml.Schema;
// using NUnit.Framework;


// using Shouldly;

// namespace Richiban.Cmdr.Tests
// {
//     [TestFixture]
//     class TransformerTests
//     {
//         [Test]
//         public void TestLeafCommandWithProvidedNameInCommandGroup()
//         {
//             var models = new[]
//             {
//                 new MethodModel(
//                     methodName: "SomeFunction",
//                     providedName: "shortcut",
//                     groupCommandPath: [ new ("SomeParent", null)],
//                     fullyQualifiedClassName: "SomeNamespace.SomeClass",
//                     arguments: [],
//                     description: null)
//             };

//             var sut = new CommandModelTransformer(new CmdrDiagnosticsManager());

//             var actual = sut.Transform(models);

//             actual.ShouldBeOfType<CommandModel.RootCommandModel>();

//             var group = actual.SubCommands.ShouldHaveSingleItem()
//                 .ShouldBeOfType<CommandModel.SubCommandModel>();

//             group.CommandName.ShouldBe("some-parent");
//             @group.GetVariableName().ShouldBe("someParentCommand");
//             group.Method.ShouldBeNull();

//             var leaf = group.SubCommands.ShouldHaveSingleItem()
//                 .ShouldBeOfType<CommandModel.SubCommandModel>();

//             leaf.CommandName.ShouldBe("shortcut");
//             leaf.GetVariableName().ShouldBe("someFunctionCommand");

//             var method = leaf.Method.ShouldNotBeNull();
            
//             method.FullyQualifiedName.ShouldBe("SomeNamespace.SomeClass.SomeFunction");
//             method.Parameters.ShouldBeEmpty();
//         }

//         [Test]
//         public void CommandThatHasAHandlerAsWellAsSubcommands()
//         {
//             var models = new[]
//             {
//                 new MethodModel(
//                     methodName: "ListRemotes",
//                     providedName: "",
//                     groupCommandPath: [new ("remote", null)],
//                     fullyQualifiedClassName: "GitNamespace.RemoteActions",
//                     arguments: [],
//                     description: null),
//                 new MethodModel(
//                     methodName: "CreateRemote",
//                     providedName: "add",
//                     groupCommandPath: [new ("remote", null)],
//                     fullyQualifiedClassName: "GitNamespace.RemoteActions",
//                     arguments:
//                     [
//                         new ParameterModel(
//                             "remoteName",
//                             "System.String",
//                             IsFlag: false,
//                             IsRequired: true,
//                             DefaultValue: null,
//                             ShortForm: null,
//                             Description: null),
//                     ],
//                     description: null)
//             };

//             var sut = new CommandModelTransformer();

//             var root = sut.Transform(models);

//             var group = root.SubCommands.ShouldHaveSingleItem()
//                 .ShouldBeOfType<CommandModel.SubCommandModel>();

//             group.CommandName.ShouldBe("remote");
            
//             var groupMethod = group.Method.ShouldNotBeNull();
            
//             groupMethod.FullyQualifiedName.ShouldBe("GitNamespace.RemoteActions.ListRemotes");
//             groupMethod.Parameters.ShouldBeEmpty();
//             @group.GetVariableName().ShouldBe("listRemotesCommand");
            
//             var subCommand = group.SubCommands.ShouldHaveSingleItem();

//             subCommand.CommandName.ShouldBe("add");
            
//             var subCommandMethod = subCommand.Method.ShouldNotBeNull();
            
//             subCommandMethod.FullyQualifiedName.ShouldBe("GitNamespace.RemoteActions.CreateRemote");
//             subCommandMethod.Parameters.ShouldHaveSingleItem();
//             subCommand.GetVariableName().ShouldBe("createRemoteCommand");
//             subCommand.SubCommands.ShouldBeEmpty();
//         }
//     }
// }