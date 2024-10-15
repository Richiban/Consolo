using Microsoft.CodeAnalysis;

namespace Consolo;

record ConsoloAttributeUsage(
    Option<string> Name, 
    Option<string> Alias,
    Option<Location> AttributeLocation,
    Option<Location> NameLocation,
    Option<Location> AliasLocation);