using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Consolo;

record TypeModel(
    string Name,
    string FullyQualifiedName,
    SpecialType SpecialType,
    TypeKind TypeKind,
    bool HasParseMethod,
    bool HasCastFromString,
    bool HasConstructorWithSingleStringParameter,
    ImmutableArray<TypeValueModel> AllowedValues);
