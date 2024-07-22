﻿using Microsoft.CodeAnalysis;

namespace Cmdr;

internal record ParameterModel(
    string Name,
    string OriginalName,
    bool IsFlag,
    bool IsRequired,
    Option<string> DefaultValue,
    Option<string> Description,
    Option<string> ShortForm,
    ITypeSymbol Type,
    Location? Location);