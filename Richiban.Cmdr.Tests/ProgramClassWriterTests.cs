using System;
using System.Collections.Generic;
using NUnit.Framework;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Writers;
using Shouldly;

namespace Richiban.Cmdr.Tests
{
    [TestFixture]
    public class ProgramClassWriterTests
    {
        [Test]
        public void RootWithCommandHandlerTest()
        {
            var rootCommand = new CommandModel.RootCommandModel()
            {
                Method = new CommandMethod(
                    "SomeNamespace.SomeClass",
                    "SomeMethod",
                    new List<CommandParameterModel>())
            };
            
            var codeGenerator = new ProgramClassFileGenerator(rootCommand);

            var actual = codeGenerator.GetCode();
            
            actual.ShouldBe(@"using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Richiban.Cmdr;

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
");
        }

        [Test]
        public void BasicTest()
        {
            var rootCommand = new CommandModel.RootCommandModel();

            rootCommand.SubCommands.Add(
                new CommandModel.SubCommandModel
                {
                    CommandName = "some-function",
                    Method = new CommandMethod(
                        "SomeClass",
                        "SomeFunction",
                        new List<CommandParameterModel>())
                });

            var codeGenerator = new ProgramClassFileGenerator(rootCommand);

            var actual = codeGenerator.GetCode();

            actual.ShouldBe(
                @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Richiban.Cmdr;

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
");
        }

        [Test]
        public void MoreComplexTest()
        {
            var rootCommand = new CommandModel.RootCommandModel();

            rootCommand.SubCommands.Add(
                new CommandModel.SubCommandModel
                {
                    CommandName = "items",
                    Method =
                        new CommandMethod(
                            "ItemActions",
                            "ListItems",
                            new List<CommandParameterModel>()),
                    SubCommands =
                    {
                        new CommandModel.SubCommandModel
                        {
                            CommandName = "add",
                            Method = new CommandMethod(
                                "ItemActions",
                                "AddItem",
                                [
                                    new CommandParameterModel.
                                        CommandPositionalParameterModel(
                                            "itemName",
                                            "System.String",
                                            true,
                                            null,
                                            "test description"),
                                    new CommandParameterModel.
                                        CommandFlagModel("allowClobber", "test description")
                                ])
                        }
                    }
                });

            var codeGenerator = new ProgramClassFileGenerator(rootCommand);

            var actual = codeGenerator.GetCode();

            actual.ShouldBe(
                @"using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Richiban.Cmdr;

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
");
        }
    }
}