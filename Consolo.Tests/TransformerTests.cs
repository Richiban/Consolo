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
using NUnit.Framework;
using Shouldly;

namespace Consolo.Tests;

[TestFixture]
class TransformerTests
{
    [Test]
    public void BaseCase()
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
                Location: null)
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
    public void TestLeafCommandWithProvidedNameInCommandGroup()
    {
        var models = new[]
        {
            new MethodModel(
                MethodName: "SomeFunction",
                ProvidedName: "shortcut",
                ParentCommandPath: [new("SomeParent", null)],
                FullyQualifiedClassName: "SomeNamespace.SomeClass",
                Parameters: [],
                Description: null,
                Location: null)
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
                ParentCommandPath: [new("remote", null)],
                FullyQualifiedClassName: "GitNamespace.RemoteActions",
                Parameters: [],
                Description: null,
                Location: null),
            new MethodModel(
                MethodName: "CreateRemote",
                ProvidedName: "add",
                ParentCommandPath: [new("remote", null)],
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
                Location: null)
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
}