﻿using Microsoft.CodeAnalysis;

namespace Consolo;

internal record ParameterModel(
    string Name,
    string SourceName,
    bool IsFlag,
    bool IsRequired,
    Option<string> DefaultValue,
    Option<string> Description,
    Option<string> Alias,
    TypeModel Type,
    Option<Location> Location,
    Option<Location> NameLocation,
    Option<Location> AliasLocation);
