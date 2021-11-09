using System;
using System.Text;

namespace Richiban.Cmdr.Generators
{
    internal class CodeBuilder
    {
        private readonly StringBuilder _sb = new();
        private int _indentationLevel;

        private string Indentation => new(c: ' ', 4 * _indentationLevel);

        public void AppendLine()
        {
            AppendLine("", ignoreBlank: false);
        }

        public void AppendLine(string line, bool ignoreBlank = true)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (ignoreBlank)
                {
                    return;
                }
                else
                {
                    _sb.AppendLine();

                    return;
                }
            }

            _sb.AppendLine(Indentation + line);
        }

        public void AppendLines(string line1, string line2)
        {
            AppendLine(line1);
            AppendLine(line2);
        }

        public void AppendLines(params string[] lines)
        {
            foreach (var line in lines)
            {
                AppendLine(line);
            }
        }

        public void AppendLine(string line1, string line2, string line3)
        {
            AppendLine(line1);
            AppendLine(line2);
            AppendLine(line3);
        }

        public void Append(string text)
        {
            _sb.Append(text);
        }

        private void IncreaseIndentation() => _indentationLevel++;

        private void DecreaseIndentation() =>
            _indentationLevel = Math.Max(_indentationLevel - 1, val2: 0);

        public override string ToString() => _sb.ToString();

        public IDisposable Indent() => new Indenter(this);

        public CommaSeparatedExpressionSyntax OpenExpressionList()
        {
            return new CommaSeparatedExpressionSyntax(this);
        }

        private class Indenter : IDisposable
        {
            private readonly CodeBuilder _codeBuilder;

            public Indenter(CodeBuilder codeBuilder)
            {
                _codeBuilder = codeBuilder;
                _codeBuilder.IncreaseIndentation();
            }

            public void Dispose()
            {
                _codeBuilder.DecreaseIndentation();
            }
        }

        public class CommaSeparatedExpressionSyntax : IDisposable
        {
            private readonly bool _any = false;
            private readonly CodeBuilder _codeBuilder;

            public CommaSeparatedExpressionSyntax(CodeBuilder codeBuilder)
            {
                _codeBuilder = codeBuilder;
                _codeBuilder.IncreaseIndentation();
            }

            public void Dispose()
            {
                _codeBuilder.DecreaseIndentation();
            }

            public void Append(string expression)
            {
                if (_any)
                {
                    _codeBuilder.Append(", ");
                }

                _codeBuilder.Append(expression);
            }

            public void AppendLine(string expression)
            {
                if (_any)
                {
                    _codeBuilder.Append(",\n");
                }

                _codeBuilder.Append(expression);
            }
        }
    }
}