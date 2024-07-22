using System;
using System.Collections.Generic;
using System.Text;

namespace Cmdr;

internal class CodeBuilder
{
    private readonly StringBuilder _sb = new();
    private int _indentationLevel;

    private string Indentation => new(c: ' ', 4 * _indentationLevel);

    public void AppendLine()
    {
        AppendLine("");
    }

    public void AppendLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            _sb.AppendLine();

            return;
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

    public void Append(string text, bool withIndentation = false)
    {
        if (withIndentation)
        {
            _sb.Append(Indentation);
        }

        _sb.Append(text);
    }

    private void IncreaseIndentation()
    {
        _indentationLevel++;
    }

    private void DecreaseIndentation()
    {
        _indentationLevel = Math.Max(_indentationLevel - 1, 0);
    }

    public override string ToString() => _sb.ToString();

    public IDisposable Indent() => new Indenter(this, null, null);

    public IDisposable Indent(string beforeLine, string afterLine) => new Indenter(this, beforeLine, afterLine);

    public IDisposable IndentBraces() => Indent("{", "}");

    public CommaSeparatedExpressionSyntax OpenExpressionList() => new(this);

    private class Indenter : IDisposable
    {
        private readonly CodeBuilder _codeBuilder;
        private readonly string? _afterLine;

        public Indenter(CodeBuilder codeBuilder, string? beforeLine, string? afterLine)
        {
            _codeBuilder = codeBuilder;
            
            if (beforeLine is not null)
            {
                _codeBuilder.AppendLine(beforeLine);
            }

            _codeBuilder.IncreaseIndentation();
            _afterLine = afterLine;
        }

        public void Dispose()
        {
            _codeBuilder.DecreaseIndentation();

            if (_afterLine is not null)
            {
                _codeBuilder.AppendLine(_afterLine);
            }
        }
    }

    public class CommaSeparatedExpressionSyntax : IDisposable
    {
        private readonly CodeBuilder _codeBuilder;
        private readonly IDisposable _indenter;
        private bool _anyWritten;
        private bool _doneOne;
        private bool _isDisposed;

        public CommaSeparatedExpressionSyntax(CodeBuilder codeBuilder)
        {
            _codeBuilder = codeBuilder;
            _indenter = codeBuilder.Indent();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            
            _indenter.Dispose();
            _isDisposed = true;
        }

        public void Append(string expression)
        {
            if (_anyWritten)
            {
                _codeBuilder.Append(", ");
                _doneOne = false;
            }

            _codeBuilder.Append(expression);

            _anyWritten = true;
        }

        public void AppendLine(string expression)
        {
            if (_anyWritten && _doneOne)
            {
                _codeBuilder.AppendLine(",");
                _doneOne = false;
            }

            _codeBuilder.AppendLine(expression);
            _anyWritten = true;
        }

        public void Next() => _doneOne = true;

        public IDisposable Indent() => _codeBuilder.Indent();
    }
}