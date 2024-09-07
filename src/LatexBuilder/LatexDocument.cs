namespace LatexBuilder;

using System.Diagnostics;
using System.Text;

/// <summary>
/// Like a StringBuilder but for writing LaTeX reports
/// </summary>
public sealed class LatexDocument
{
    // Underlying report
    private readonly StringBuilder _sb;

    // Current section level
    public int Level { get; private set; }

    // All open scopes
    private Stack<Scope>? _scopes;

    public int Index { get; private set; }

    /// <summary>
    /// Create a new ReportBuilder with the given title
    /// </summary>
    public LatexDocument(int level)
    {
        Level = level;
        Index = 0;
        _sb = new StringBuilder();
        _scopes = new Stack<Scope>();
    }

    public void Space()
    {
        _sb.Append(' ');
    }

    public void Insert(int index, ReadOnlySpan<char> value)
    {
        _sb.Insert(index, value);
    }

    public int Bookmark() => Index = _sb.Length;

    public void BeginSection(string title)
    {
        var (cmd, pad) = Level++ switch
        {
            0 => ("chapter", false),
            1 => ("section", false),
            2 => ("subsection", false),
            3 => ("subsubsection", false),
            4 => ("paragraph", true),
            _ => ("subparagraph", true)
        };

        if (!pad)
        {
            WriteLn($@"\{cmd}{{{title}}}");
        }
        else
        {
            WriteLn($@"\{cmd}{{{title}}} \hfill");
            NewLine();
        }
    }

    public void EndSection()
    {
        Level--;
        NewLine();
    }

    public IDisposable Section(string title)
    {
        BeginSection(title);
        return new Scope(EndSection);
    }

    public void Command2(string cmd, string arg1, string arg2) =>
        Command(cmd, arg: arg1, arg2: arg2);

    public void Command(string cmd, string? arg = null, string? opt = null, string? arg2 = null)
    {
        Write('\\');
        Write(cmd);
        if (arg is not null)
        {
            Write('{');
            Write(arg);
            Write('}');
            if (opt is not null)
            {
                Write('[');
                Write(opt);
                Write(']');
            }

            if (arg2 is not null)
            {
                Write('{');
                Write(arg2);
                Write('}');
            }
        }
        NewLine();
    }

    public IDisposable Landscape() => Environment("landscape");

    public IDisposable Table(
        string format,
        string layout = "H",
        string? caption = null,
        string? label = null,
        bool centered = true
    )
    {
        Begin("table", $"[{layout}]");
        if (centered)
            Command("centering");
        Begin("tabular", $"{{{format}}}");
        return PushScope(() =>
        {
            End("tabular");
            if (caption is not null)
                Command("caption*", caption);
            if (label is not null)
                Label(label);
            End("table");
        });
    }
    
    public void RowColour(string colour) => Command("rowcolor", colour);

    public void Label(string label) => Command("label", label);

    public void HLine() => Command("hline");

    public void TopRule() => Command("toprule");

    public void MidRule() => Command("midrule");

    public void BottomRule() => Command("bottomrule");

    public void BeginBrackets() => Command("left(");

    public void EndBrackets() => Command("right)");

    /// Write a math block
    public void WriteMath(string latex)
    {
        Begin("align*");
        WriteLn(latex);
        End("align*");
    }

    public void Write(string? s)
    {
        _sb.Append(s);
    }

    public void WriteLn(string? s)
    {
        _sb.AppendLine(s);
    }

    public void WriteRow(params string[] values)
    {
        _sb.AppendJoin(" & ", values);
        _sb.AppendLine(@"\\");
    }

    public void WriteLn(char c)
    {
        _sb.Append(c);
        _sb.AppendLine();
    }

    public void WriteLn2(string? s)
    {
        _sb.AppendLine(s);
        _sb.AppendLine();
    }

    public void WriteJoin(string join, IEnumerable<string> strings)
    {
        using var enumerator = strings.GetEnumerator();
        if (!enumerator.MoveNext())
            return;

        while (true)
        {
            _sb.Append(enumerator.Current);
            if (!enumerator.MoveNext())
                break;
            _sb.Append(join);
        }
    }

    public void Begin(string cmd, string? args = null)
    {
        Write('\\');
        Write("begin");
        Write('{');
        Write(cmd);
        Write('}');
        if (args is not null)
        {
            Write(args);
        }
        NewLine();
    }

    public void End(string cmd)
    {
        if (_sb[^1] != '\n')
            _sb.Append('\n');
        Command("end", cmd);
    }

    public IDisposable Environment(string cmd, string? args = null)
    {
        Begin(cmd, args);
        return PushScope(() => End(cmd));
    }

    public void Write(char c)
    {
        _sb.Append(c);
    }

    public void Write(char c, int count)
    {
        _sb.Append(c, count);
    }

    public void NewLine()
    {
        _sb.AppendLine();
    }

    /// <summary>
    /// Create a new scope with the given disposable action
    /// </summary>
    internal IDisposable PushScope(Action action)
    {
        var scope = new Scope(action);
        _scopes ??= new Stack<Scope>();
        _scopes.Push(scope);
        return scope;
    }
    
    public string Text()
    {
        // Close out all remaining scopes
        while (_scopes is { Count: > 0 })
        {
            var scope = _scopes.Pop();
            if (!scope.Disposed)
                scope.Dispose();
        }
        var text = _sb.ToString();
        return text;
    }

    public string WriteToFile(string path)
    {
        var text = Text();
        File.WriteAllText(path, text);
        return text;
    }

    public string WriteToFile(FileInfo file) => WriteToFile(file.FullName);


    /// <summary>
    /// A disposable that performs a side effect when disposed.
    /// in our case this means closing open brackets, or
    /// dedenting.
    /// </summary>
    private sealed class Scope(Action onDispose) : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            onDispose();
            Disposed = true;
        }
    }

    public override string ToString() => $"LatexDocument";
}
