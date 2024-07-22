using System;
using NUnit.Framework;

namespace Cmdr.Tests
{
    [TestFixture]
    public class CommandTreeWriterTests
    {
        [Test]
        public void BasicCommandTest()
        {
            var codeGenerator = new ProgramClassFileGenerator(
                "testAssembly",
                new CommandTree.Root{
                    Description = "A collection of test commands",
                    SubCommands =
                    {
                        new CommandTree.SubCommand("test")
                        {
                            Description = "A test command group",
                            Method = new CommandMethod(
                                FullyQualifiedClassName: "TestClass",
                                MethodName: "TestMethod",
                                Parameters: new CommandParameter[]
                                {
                                    new CommandParameter.Positional(
                                        "file",
                                        "System.IO.FileInfo")
                                },
                                Description: "A test command that takes a file as a parameter")
                        }
                    }
                });
    
            var actual = codeGenerator.GetCode();
    
            Console.WriteLine(actual);
        }
    
        [Test]
        public void BasicCommandTestWithParameters()
        {
            var codeGenerator = new CommandCodeFileGenerator(
                new CommandTree.LeafCommandTree(
                    "test-command",
                    "SomeClass",
                    "SomeFunction",
                    new CommandParameterModel[]
                    {
                        new CommandParameterModel.CommandPositionalParameterModel(
                            "someParam",
                            "System.String"),
                        new CommandParameterModel.CommandFlagParameterModel(
                            "someOtherParam")
                    }));
    
            var actual = codeGenerator.WriteCodeLines();
    
            Console.WriteLine(actual);
        }
    
        [Test]
        public void GroupCommandTest()
        {
            var codeGenerator = new CommandCodeFileGenerator(
                new CommandTree.CommandGroupModel(
                    "remote",
                    new[]
                    {
                        new CommandTree.LeafCommandTree(
                            "add",
                            "SomeOtherClass",
                            "SomeOtherFunction",
                            new CommandParameterModel[] { })
                    }));
    
            var actual = codeGenerator.WriteCodeLines();
    
            Console.WriteLine(actual);
        }
    
        [Test]
        public void RootCommandTest()
        {
            var codeGenerator = new CommandCodeFileGenerator(
                new CommandTree.RootCommandTree(
                    new[]
                    {
                        new CommandTree.LeafCommandTree(
                            "one",
                            "SomeClass",
                            "SomeFunction",
                            new CommandParameterModel[] { })
                    }));
    
            var actual = codeGenerator.WriteCodeLines();
    
            Console.WriteLine(actual);
        }
    
        [Test]
        public void FullTreeTest()
        {
            var codeGenerator = new CommandCodeFileGenerator(
                new CommandTree.RootCommandTree(
                    new CommandTree[]
                    {
                        new CommandTree.CommandGroupModel(
                            "remote",
                            new[]
                            {
                                new CommandTree.LeafCommandTree(
                                    "add",
                                    "RemoteActions",
                                    "AddRemote",
                                    new CommandParameterModel[]
                                    {
                                        new CommandParameterModel.
                                            CommandPositionalParameterModel(
                                                "remote-name",
                                                "System.String")
                                    }),
                                
                                new CommandTree.LeafCommandTree(
                                    "remove",
                                    "RemoteActions",
                                    "RemoveRemote",
                                    new CommandParameterModel[]
                                    {
                                        new CommandParameterModel.
                                            CommandPositionalParameterModel(
                                                "remote-name",
                                                "System.String")
                                    })
                            }),
                        new CommandTree.LeafCommandTree(
                            "branch",
                            "BranchActions",
                            "AddBranch",
                            new[]
                            {
                                new CommandParameterModel.
                                    CommandPositionalParameterModel(
                                        "branchName",
                                        "System.String")
                            })
                    }));
    
            var actual = codeGenerator.GetCode();
    
            Console.WriteLine(actual);
        }
    }
}