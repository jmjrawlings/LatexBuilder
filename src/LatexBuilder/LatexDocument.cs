namespace LatexBuilder;

using System.Text;

/// <summary>
/// Like a StringBuilder but for writing LaTeX reports
/// </summary>
/// <param name="level">The current section level</param>
public sealed class LatexDocument(LatexLevel level)
{
    private readonly StringBuilder _sb = new();
    
    public LatexLevel Level { get; private set; } = level;
    
    public int Index => _sb.Length;
    
    /// <summary>
    /// Begin a new section at the next document level.
    /// The section must be ended by a call to <see cref="EndSection"/>.
    /// Alternatively you can use <see cref="Section"/> for a disposable
    /// that will do this for you.
    /// </summary>
    /// <param name="title">Title of the section</param>
    public void BeginSection(string title)
    {
        var cmd = Level++ switch
        {
            LatexLevel.Chapter => "chapter",
            LatexLevel.Section => "section",
            LatexLevel.Subsection => "subsection",
            LatexLevel.SubSubsection => "subsubsection",
            LatexLevel.Paragraph => "paragraph",
            LatexLevel.SubParagraph => "subparagraph",
            _ => "subparagraph",
        };
        Command(cmd, title);
    }

    public void NewPage() => Command("newpage");
    
    public void ClearPage() => Command("clearpage");
    
    /// <summary>
    /// End the current section by decrementing the document level
    /// </summary>
    public void EndSection()
    {
        Level--;
    }
    
    /// <summary>
    /// Return a disposable scope for a section with
    /// the given title.
    /// </summary>
    /// <example>
    /// using (doc.Section("Introduction")
    ///     doc.WriteLn("Lorem Ipsum");
    /// </example>
    public LatexScope Section(string title)
    {
        BeginSection(title);
        return new LatexScope(EndSection);
    }
    
    public LatexScope Landscape() => Env("landscape");
    
    /// <summary>
    /// Create a new table using the booktabs package
    /// </summary>
    public void BeginTable(
        string columns,
        string? layout = "h",
        bool centered = true
    )
    {
        if (layout is null)
            Begin("table");
        else
            Begin("table", $"[{layout}]");
        if (centered)
            Command("centering");
        Begin("tabular", $"{{{columns}}}");
    }

    public LatexScope Itemize()
    {
        Begin("itemize");
        return new LatexScope(() => End("itemize"));
    }
    
    public void Item(ReadOnlySpan<char> item)
    {
        _sb.Append("\\item ");
        _sb.Append(item);
    }
    
    public void EndTable(string? caption = null, string? label = null) 
    {
        End("tabular");
        if (caption is not null)
            Command("caption*", caption);
        if (label is not null)
            Label(label);
        End("table");
    }
    
    /// <summary>
    /// Create a new table using the booktabs package
    /// </summary>
    public LatexScope Table(
        string columns,
        string? layout = "h",
        string? caption = null,
        string? label = null,
        bool centered = true
    )
    {
        BeginTable(columns, layout, centered);
        return new LatexScope(() =>
        {
            EndTable(caption, label);
        });
    }
    
    public void WriteRow(params string[] values)
    {
        _sb.AppendJoin(" & ", values);
        _sb.AppendLine(@"\\");
    }
    
    public void RowColour(string colour) => Command("rowcolor", colour);

    public void Label(string label) => Command("label", label);

    public void HLine() => Command("hline");

    public void TopRule() => Command("toprule");

    public void MidRule() => Command("midrule");

    public void BottomRule() => Command("bottomrule");

    public void BeginParens() => Command("left(");

    public void EndParens() => Command("right)");
    
    public void BeginBrackets() => Command("left[");

    public void EndBrackets() => Command("right]");
    
    public LatexScope Parens()
    {
        BeginParens();
        return new LatexScope(EndParens);
    } 
    
    public LatexScope Brackets()
    {
        BeginBrackets();
        return new LatexScope(EndBrackets);
    } 
    
    public void Write(ReadOnlySpan<char> chars) => _sb.Append(chars);
    
    public void Write(string? s) => _sb.Append(s);

    public void WriteLn(string? s) => _sb.AppendLine(s);
    
    /// <summary>
    /// Write a paragraph
    /// </summary>
    public void Para(ReadOnlySpan<char> text)
    {
        _sb.Append(text);
        _sb.Append('\n');
        _sb.Append('\n');
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
    
    /// <summary>
    /// Start a new environment scope
    /// </summary>
    public LatexScope Env(string cmd, string? args = null)
    {
        Begin(cmd, args);
        return new LatexScope(() => End(cmd));
    }
    
    public LatexScope Align() => Env("align*");
    
    public void Write(char c) => _sb.Append(c);
    
    public void Write(char c, int count) => _sb.Append(c, count);
    
    public void NewLine() => _sb.AppendLine();
    
    /// Insert a string at an index 
    public void Insert(int index, ReadOnlySpan<char> value)
    {
        _sb.Insert(index, value);
    }

    public FileInfo WriteToFile(string path)
    {
        var text = ToString();
        File.WriteAllText(path, text);
        return new FileInfo(path);
    }
    
    public FileInfo WriteToFile(FileInfo file) => WriteToFile(file.FullName);
    
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
    
    public override string ToString()
    {
        string s = _sb.ToString();
        string tex = $$"""
            {{s}}
            \end{document}
            """;
        return tex;
    }
}