using System;
using NUnit.Framework;
using Richiban.Cmdr.Generators;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr.Generator.Tests
{
    // [TestFixture]
    // public class CommandModelWriterTests
    // {
    //     [Test]
    //     public void BasicCommandTest()
    //     {
    //         var codeGenerator = new CommandCodeFileGenerator(
    //             new CommandModel.LeafCommandModel(
    //                 "test-command",
    //                 "SomeClass",
    //                 "SomeFunction",
    //                 new CommandParameterModel[] { }));
    //
    //         var actual = codeGenerator.WriteCodeLines();
    //
    //         Console.WriteLine(actual);
    //     }
    //
    //     [Test]
    //     public void BasicCommandTestWithParameters()
    //     {
    //         var codeGenerator = new CommandCodeFileGenerator(
    //             new CommandModel.LeafCommandModel(
    //                 "test-command",
    //                 "SomeClass",
    //                 "SomeFunction",
    //                 new CommandParameterModel[]
    //                 {
    //                     new CommandParameterModel.CommandPositionalParameterModel(
    //                         "someParam",
    //                         "System.String"),
    //                     new CommandParameterModel.CommandFlagParameterModel(
    //                         "someOtherParam")
    //                 }));
    //
    //         var actual = codeGenerator.WriteCodeLines();
    //
    //         Console.WriteLine(actual);
    //     }
    //
    //     [Test]
    //     public void GroupCommandTest()
    //     {
    //         var codeGenerator = new CommandCodeFileGenerator(
    //             new CommandModel.CommandGroupModel(
    //                 "remote",
    //                 new[]
    //                 {
    //                     new CommandModel.LeafCommandModel(
    //                         "add",
    //                         "SomeOtherClass",
    //                         "SomeOtherFunction",
    //                         new CommandParameterModel[] { })
    //                 }));
    //
    //         var actual = codeGenerator.WriteCodeLines();
    //
    //         Console.WriteLine(actual);
    //     }
    //
    //     [Test]
    //     public void RootCommandTest()
    //     {
    //         var codeGenerator = new CommandCodeFileGenerator(
    //             new CommandModel.RootCommandModel(
    //                 new[]
    //                 {
    //                     new CommandModel.LeafCommandModel(
    //                         "one",
    //                         "SomeClass",
    //                         "SomeFunction",
    //                         new CommandParameterModel[] { })
    //                 }));
    //
    //         var actual = codeGenerator.WriteCodeLines();
    //
    //         Console.WriteLine(actual);
    //     }
    //
    //     [Test]
    //     public void FullTreeTest()
    //     {
    //         var codeGenerator = new CommandCodeFileGenerator(
    //             new CommandModel.RootCommandModel(
    //                 new CommandModel[]
    //                 {
    //                     new CommandModel.CommandGroupModel(
    //                         "remote",
    //                         new[]
    //                         {
    //                             new CommandModel.LeafCommandModel(
    //                                 "add",
    //                                 "RemoteActions",
    //                                 "AddRemote",
    //                                 new CommandParameterModel[]
    //                                 {
    //                                     new CommandParameterModel.
    //                                         CommandPositionalParameterModel(
    //                                             "remote-name",
    //                                             "System.String")
    //                                 }),
    //                             
    //                             new CommandModel.LeafCommandModel(
    //                                 "remove",
    //                                 "RemoteActions",
    //                                 "RemoveRemote",
    //                                 new CommandParameterModel[]
    //                                 {
    //                                     new CommandParameterModel.
    //                                         CommandPositionalParameterModel(
    //                                             "remote-name",
    //                                             "System.String")
    //                                 })
    //                         }),
    //                     new CommandModel.LeafCommandModel(
    //                         "branch",
    //                         "BranchActions",
    //                         "AddBranch",
    //                         new[]
    //                         {
    //                             new CommandParameterModel.
    //                                 CommandPositionalParameterModel(
    //                                     "branchName",
    //                                     "System.String")
    //                         })
    //                 }));
    //
    //         var actual = codeGenerator.WriteCodeLines();
    //
    //         Console.WriteLine(actual);
    //     }
    // }
}