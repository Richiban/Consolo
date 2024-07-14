using System;
using System.Collections.Generic;

namespace Richiban.Cmdr;

internal class NewProgramClassFileGenerator(IReadOnlyCollection<MethodModel> results) : CodeFileGenerator
{
    public override string FileName => "NewProgram.g.cs";

    public override string GetCode()
    {
        var codeBuilder = new CodeBuilder();

        codeBuilder.AppendLines(
            "using System;",
            "using System.CommandLine;",
            "using System.CommandLine.Invocation;");

        codeBuilder.AppendLine();

        foreach(var result in results)
        {
            codeBuilder.Append("// ");
            codeBuilder.Append(String.Join(" ", result.GroupCommandPath));
            codeBuilder.Append(" ");
            codeBuilder.Append();
            codeBuilder.AppendLine($"// {String.Join(" ", result.GroupCommandPath)} => {result.MethodName}");
        }

        codeBuilder.AppendLine();

        return codeBuilder.ToString();
    }
}