using System;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

record DiagnosticModel(string Message, Location? Location, DiagnosticSeverity Severity);
