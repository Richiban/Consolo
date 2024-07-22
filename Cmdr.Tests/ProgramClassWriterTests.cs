using System;
using System.Collections.Generic;
using NUnit.Framework;


using Shouldly;

namespace Cmdr.Tests
{
    //TODO: reinstate [TestFixture]
    public class ProgramClassWriterTests
    {
        //[Test]
        public void RootWithCommandHandlerTest()
        {
            var rootCommand = new CommandTree.Root()
            {
                Method = new CommandMethod(
                    "SomeNamespace.SomeClass",
                    "SomeMethod",
                    new List<CommandParameter>())
            };
            
            var codeGenerator = new ProgramClassFileGenerator("testAssembly", rootCommand);

            var actual = codeGenerator.GetCode();
            
            actual.ShouldBe(
                """
                using System;

                public static class Program
                {
                    public static int Main(string[] args)
                    {
                        var rootCommand = new RootCommand()
                        {
                        };

                        rootCommand.Handler = CommandHandler.Create(SomeNamespace.SomeClass.SomeMethod);

                        if (Repl.IsCall(args))
                        {
                            Repl.EnterNewLoop(rootCommand, ""Select a command"");

                            return 0;
                        }
                        else
                        {
                            return rootCommand.Invoke(args);
                        }
                    }
                }
                """
                );
        }

        [Test]
        public void BasicTest()
        {
            var rootCommand = new CommandModel.RootCommandModel();

            rootCommand.SubCommands.Add(
                new CommandTree.SubCommand("some-function")
                {
                    Method = new CommandMethod(
                        "SomeClass",
                        "SomeFunction",
                        new List<CommandParameter>())
                });

            var codeGenerator = new ProgramClassFileGenerator("testAssembly", rootCommand);

            var actual = codeGenerator.GetCode();

            actual.ShouldBe(
                """
                using System;
                using System.CommandLine;
                using System.CommandLine.Invocation;


                public static class Program
                {
                    public static int Main(string[] args)
                    {
                        var someFunctionCommand = new Command(""some-function"")
                        {
                        };

                        someFunctionCommand.Handler = CommandHandler.Create(SomeClass.SomeFunction);

                        var rootCommand = new RootCommand()
                        {
                            someFunctionCommand
                        };

                        if (Repl.IsCall(args))
                        {
                            Repl.EnterNewLoop(rootCommand, ""Select a command"");

                            return 0;
                        }
                        else
                        {
                            return rootCommand.Invoke(args);
                        }
                    }
                }
                """
            );
        }

        [Test]
        public void MoreComplexTest()
        {
            var rootCommand = new CommandTree.Root();

            rootCommand.SubCommands.Add(
                new CommandTree.SubCommand("items")
                {
                    Method =
                        new CommandMethod(
                            "ItemActions",
                            "ListItems",
                            new List<CommandParameter>(),
                            "test description for items"),
                    SubCommands =
                    {
                        new CommandTree.SubCommand("add")
                        {
                            Method = new CommandMethod(
                                "ItemActions",
                                "AddItem",
                                [
                                    new CommandParameter.Positional(
                                        name: "item-name",
                                        originalName: "itemName",
                                            
                                            
                                            "System.String",
                                            true,
                                            null,
                                            "test description for itemName"),
                                    new CommandParameter.Flag(
                                        "allowClobber", 
                                        "a",
                                        "test description for allowClobber")
                                ],
                                "test description for add")
                        }
                    }
                });

            var codeGenerator = new ProgramClassFileGenerator("testAssembly", rootCommand);

            var actual = codeGenerator.GetCode();

            actual.ShouldBe(
                """
                using System;
                using System.CommandLine;
                using System.CommandLine.Invocation;


                public static class Program
                {
                    public static int Main(string[] args)
                    {
                        var addItemCommand = new Command(""add"")
                        {
                            new Argument(""itemName"")
                            ,
                            new Option(new string[] {""a"", ""allowClobber""})
                        };

                        addItemCommand.Handler = CommandHandler.Create<System.String, System.Boolean>(ItemActions.AddItem);

                        var listItemsCommand = new Command(""items"")
                        {
                            addItemCommand
                        };

                        listItemsCommand.Handler = CommandHandler.Create(ItemActions.ListItems);

                        var rootCommand = new RootCommand()
                        {
                            listItemsCommand
                        };

                        if (Repl.IsCall(args))
                        {
                            Repl.EnterNewLoop(rootCommand, ""Select a command"");

                            return 0;
                        }
                        else
                        {
                            return rootCommand.Invoke(args);
                        }
                    }
                }
                """
            );
        }
    }
}