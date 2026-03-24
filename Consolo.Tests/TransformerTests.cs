using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Shouldly;

namespace Consolo.Tests;

[TestFixture]
class TransformerTests
{
    private readonly Mock<ITypeSymbol> VoidMock = new(MockBehavior.Strict);
    private readonly Mock<ITypeSymbol> TaskMock = new(MockBehavior.Strict);

    public TransformerTests()
    {
        VoidMock.Setup(t => t.SpecialType).Returns(SpecialType.System_Void);
        
        VoidMock
            .Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat?>()))
            .Returns("void");
        
        TaskMock
            .Setup(t => t.ToDisplayString(It.IsAny<SymbolDisplayFormat>()))
            .Returns("System.Threading.Tasks.Task");
    }
    
    [Test]
    public void RootCommandWithFunction()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "SomeFunction",
                ProvidedName: "",
                ParentCommandPath: [],
                FullyQualifiedClassName: "SomeNamespace.SomeClass",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object)
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        diagnostics.ShouldBeEmpty();

        root.SubCommands.ShouldBeEmpty();

        root
            .Method.ShouldBeSome()
            .ShouldSatisfyAllConditions(
                method => method.Parameters.ShouldBeEmpty(),
                method => method.FullyQualifiedName.ShouldBe(
                    "SomeNamespace.SomeClass.SomeFunction"),
                method => method.Options.ShouldBeEmpty(),
                method => method.MandatoryParameterCount.ShouldBe(0));
    }

    [Test]
    public void RootCommandWithAsyncFunction()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "SomeFunction",
                ProvidedName: "",
                ParentCommandPath: [],
                FullyQualifiedClassName: "SomeNamespace.SomeClass",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: TaskMock.Object)
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        diagnostics.ShouldBeEmpty();

        root.SubCommands.ShouldBeEmpty();

        root
            .Method.ShouldBeSome()
            .ShouldSatisfyAllConditions(
                method => method.Parameters.ShouldBeEmpty(),
                method => method.FullyQualifiedName.ShouldBe(
                    "SomeNamespace.SomeClass.SomeFunction"),
                method => method.Options.ShouldBeEmpty(),
                method => method.MandatoryParameterCount.ShouldBe(0),
                method => method.IsTaskReturn.ShouldBe(true));
    }

    [Test]
    public void TestLeafCommandWithProvidedNameInCommandGroup()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "SomeFunction",
                ProvidedName: "shortcut",
                ParentCommandPath: [new(SymbolName: "SomeParent", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "SomeNamespace.SomeClass",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object)
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        var group = root.SubCommands.ShouldHaveSingleItem();

        group.CommandName.ShouldBe("some-parent");
        group.Method.HasValue.ShouldBeFalse();

        var leaf = group.SubCommands.ShouldHaveSingleItem();

        leaf.CommandName.ShouldBe("shortcut");

        var method = (leaf.Method | null!).ShouldNotBeNull();

        method.FullyQualifiedName.ShouldBe("SomeNamespace.SomeClass.SomeFunction");
        method.Parameters.ShouldBeEmpty();
    }

    [Test]
    public void CommandThatHasAHandlerAsWellAsSubcommands()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "ListRemotes",
                ProvidedName: "",
                ParentCommandPath: [new(SymbolName: "RemoteActions", AttributeName: "remote", XmlComment: null)],
                FullyQualifiedClassName: "GitNamespace.RemoteActions",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
            new MethodModel(
                MethodName: "CreateRemote",
                ProvidedName: "add",
                ParentCommandPath: [new(SymbolName: "RemoteActions", AttributeName: "remote", XmlComment: null)],
                FullyQualifiedClassName: "GitNamespace.RemoteActions",
                Parameters:
                [
                    new ParameterModel(
                        Name: "name",
                        SourceName: "remoteName",
                        Type: new TypeModel(
                            Name: "String",
                            FullyQualifiedName: "System.String",
                            SpecialType: SpecialType.None,
                            TypeKind: TypeKind.Class,
                            HasParseMethod: false,
                            HasCastFromString: false,
                            HasConstructorWithSingleStringParameter: false,
                            AllowedValues: ImmutableArray<TypeValueModel>.Empty),
                        IsFlag: false,
                        IsRequired: true,
                        DefaultValue: null,
                        Alias: null,
                        Description: null,
                        Location: null,
                        NameLocation: null,
                        AliasLocation: null),
                ],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object)
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        var group = root.SubCommands.ShouldHaveSingleItem();

        group.CommandName.ShouldBe("remote");

        var groupMethod = (group.Method | null!).ShouldNotBeNull();

        groupMethod.FullyQualifiedName.ShouldBe("GitNamespace.RemoteActions.ListRemotes");
        groupMethod.Parameters.ShouldBeEmpty();

        var subCommand = group.SubCommands.ShouldHaveSingleItem();

        subCommand.CommandName.ShouldBe("add");

        var subCommandMethod = (subCommand.Method | null!).ShouldNotBeNull();

        subCommandMethod.FullyQualifiedName.ShouldBe("GitNamespace.RemoteActions.CreateRemote");
        subCommandMethod.Parameters.ShouldHaveSingleItem();
        subCommand.SubCommands.ShouldBeEmpty();
    }

    [Test]
    public void InferredParentNameGroupsMultipleSubcommands()
    {
        // Verify that when a parent command path uses a PascalCase inferred name (e.g. "Fancy"),
        // all child commands are grouped under the same "fancy" parent rather than each getting
        // their own duplicate parent node.
        var models = new[]
        {
            new MethodModel(
                MethodName: "Table",
                ProvidedName: "table",
                ParentCommandPath: [new(SymbolName: "Fancy", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "ConsoloTest.Fancy",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
            new MethodModel(
                MethodName: "Choice",
                ProvidedName: null,
                ParentCommandPath: [new(SymbolName: "Fancy", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "ConsoloTest.Fancy",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
            new MethodModel(
                MethodName: "Throw",
                ProvidedName: null,
                ParentCommandPath: [new(SymbolName: "Fancy", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "ConsoloTest.Fancy",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        diagnostics.ShouldBeEmpty();

        // There should be exactly ONE "fancy" group at root level
        var fancyGroup = root.SubCommands.ShouldHaveSingleItem();
        fancyGroup.CommandName.ShouldBe("fancy");

        // All three subcommands should be under the single "fancy" group
        fancyGroup.SubCommands.Count.ShouldBe(3);
        fancyGroup.SubCommands.Select(s => s.CommandName)
            .ShouldBe(["table", "choice", "throw"], ignoreOrder: true);
    }

    [Test]
    public void InferredNamesAtMultipleLevelsGroupCorrectly()
    {
        // Verify multi-level nesting with inferred PascalCase names groups correctly.
        // This is similar to the NestedAttributeUsage snapshot test case.
        var models = new[]
        {
            new MethodModel(
                MethodName: "TestMethod1",
                ProvidedName: null,
                ParentCommandPath: [new(SymbolName: "OuterTest", AttributeName: null, XmlComment: null), new(SymbolName: "InnerTest1", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "TestSamples.OuterTest.InnerTest1",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
            new MethodModel(
                MethodName: "TestMethod2",
                ProvidedName: null,
                ParentCommandPath: [new(SymbolName: "OuterTest", AttributeName: null, XmlComment: null), new(SymbolName: "InnerTest1", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "TestSamples.OuterTest.InnerTest1",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
            new MethodModel(
                MethodName: "TestMethod3",
                ProvidedName: null,
                ParentCommandPath: [new(SymbolName: "OuterTest", AttributeName: null, XmlComment: null), new(SymbolName: "InnerTest2", AttributeName: null, XmlComment: null)],
                FullyQualifiedClassName: "TestSamples.OuterTest.InnerTest2",
                Parameters: [],
                Description: null,
                Location: null,
                ReturnType: VoidMock.Object),
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        diagnostics.ShouldBeEmpty();

        // There should be exactly ONE "outer-test" group at root level (not three)
        var outerGroup = root.SubCommands.ShouldHaveSingleItem();
        outerGroup.CommandName.ShouldBe("outer-test");

        // "outer-test" should contain exactly two inner groups
        outerGroup.SubCommands.Count.ShouldBe(2);
        outerGroup.SubCommands.Select(s => s.CommandName)
            .ShouldBe(["inner-test1", "inner-test2"], ignoreOrder: false);

        var innerTest1 = outerGroup.SubCommands[0];
        innerTest1.CommandName.ShouldBe("inner-test1");
        innerTest1.SubCommands.Count.ShouldBe(2);

        var innerTest2 = outerGroup.SubCommands[1];
        innerTest2.CommandName.ShouldBe("inner-test2");
        innerTest2.SubCommands.ShouldHaveSingleItem();
    }

    [Test]
    public void ParameterWithDefaultValueResultsInOptionWithSameDefault()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "M",
                Description: "",
                ProvidedName: "",
                ParentCommandPath: [],
                Parameters:
                [
                    new ParameterModel(
                        Name: "arg1",
                        SourceName: "arg1",
                        IsFlag: false,
                        IsRequired: false,
                        DefaultValue: "1",
                        Description: new Option<string>(),
                        Alias: new Option<string>(),
                        Type: new TypeModel(
                            "int",
                            "int",
                            SpecialType.System_Int32,
                            TypeKind.Struct,
                            HasParseMethod: true,
                            HasCastFromString: false,
                            HasConstructorWithSingleStringParameter: false,
                            AllowedValues: []),
                        Location: new Option<Location>(),
                        NameLocation: new Option<Location>(),
                        AliasLocation: new Option<Location>()),
                ],
                FullyQualifiedClassName: "N.C",
                Location: null,
                ReturnType: VoidMock.Object),
        };

        var (root, diagnostics) = CommandTreeBuilder.Transform(models);

        var method = root.Method.ShouldBeSome();

        method
            .Parameters.ShouldHaveSingleItem()
            .ShouldBeOfType<CommandParameter.Option>()
            .ShouldSatisfyAllConditions(
                p => p.Name.ShouldBe("arg1"),
                p => p.DefaultValue.ShouldBe("1"));
    }
}