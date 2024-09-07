namespace LatexBuilder;


/// <summary>
/// Builds a LaTeX document by correctly setting
/// the documentclass as well as any other metadata
/// </summary>
public class LatexBuilder
{
    public string? Title { get; set; }

    public string? Author { get; set; }

    public LatexClass Class { get; private set; } = LatexClass.Article;

    public string DocumentClass { get; private set; } = "article";
    
    public string? Institute { get; set; }
    
    public int Level { get; private set; }

    public List<(string, string?)> Packages { get; private set; }

    public List<(string, int, string)> Commands { get; private set; }

    public List<(string, string)> Lengths { get; private set; }

    public bool TitlePage { get; set; }

    public LatexBuilder()
    {
        Packages = new List<(string, string?)>();
        Commands = new List<(string, int, string)>();
        Lengths = new List<(string, string)>();
    }

    public LatexBuilder WithClass(LatexClass cls)
    {
        Class = cls;
        Level = cls switch
        {
            LatexClass.Book => 0,
            _ => 1
        };
        DocumentClass = cls switch
        {
            LatexClass.Article => "article",
            LatexClass.Book => "book",
            LatexClass.Report => "report",
            _ => throw new ArgumentException(cls.ToString())
        };
        return this;
    }

    public LatexBuilder WithTitle(string? title)
    {
        Title = title;
        return this;
    }

    public LatexBuilder WithTitlePage(bool? ok = null)
    {
        TitlePage = ok ?? false;
        return this;
    }

    public LatexBuilder WithAuthor(string? author)
    {
        Author = author;
        return this;
    }

    public LatexBuilder WithInstitute(string? institute)
    {
        Institute = institute;
        return this;
    }

    public LatexBuilder WithLength(string name, string val)
    {
        Lengths.Add((name, val));
        return this;
    }

    public static LatexBuilder Default() =>
        new LatexBuilder()
            .WithPackage("mathtools")
            .WithPackage("amssymb")
            .WithPackage("float") // for explicit layout
            .WithPackage("graphicx") // layout geometry
            .WithPackage("hyperref") // Hyperlinks between elements in PDF
            .WithPackage("siunitx") // for unit of measure support
            .WithPackage("xfrac") // for nice (a/b) fraction styles
            .WithPackage("caption") // for no numbering captions
            .WithPackage("booktabs") // for nice looking tables
            .WithPackage("pifont") // for wingdings tick / cross marks
            .WithPackage("pdflscape") // for nice landscape support in PDF
            .WithPackage("xcolor", "table") // for font/cell colours in tables
            .WithCommand("parens", 1, @"\left( #1 \right)")
            .WithCommand("parensq", 1, @"\left[ #1 \right]")
            .WithCommand("sq", 1, @"\parens{#1}^2")
            .WithCommand("cb", 1, @"\parens{#1}^3")
            .WithCommand("fmin", 1, @"min \left( #1 \right)")
            .WithCommand("starr", 1, "{#1}^*")
            .WithCommand("half", 1, @"\frac{#1}{2}")
            .WithAlias("cmark", @"\ding{51}")
            .WithAlias("xmark", @"\ding{55}")
            .WithLength("parindent", "0pt");

    public LatexDocument Build()
    {
        var doc = new LatexDocument(Level);
        doc.Command("documentclass", DocumentClass);
        foreach (var (pkg, extra) in Packages)
            doc.Command("usepackage", pkg, extra);
        foreach (var (name, len) in Lengths)
            doc.Command2("setlength", $"\\{name}", len);

        doc.NewLine();
        if (Title is not null)
            doc.Command("title", Title);
        if (Author is not null)
            doc.Command("author", Author);
        doc.Command("date", "\\today");
        if (TitlePage)
            doc.Command("maketitle");
        doc.Environment("document");
        foreach (var (name, args, body) in Commands)
            if (args is 0)
                doc.WriteLn($@"\newcommand{{\{name}}}{{{body}}}");
            else
                doc.WriteLn($@"\newcommand{{\{name}}}[{args}]{{{body}}}");
        doc.NewLine();
        return doc;
    }

    public static LatexBuilder Report() => Default().WithClass(LatexClass.Report);

    public static LatexBuilder Book() => Default().WithClass(LatexClass.Book);

    public static LatexBuilder Article() => Default().WithClass(LatexClass.Article);

    /// Use the given package eg: `siunitx`
    public LatexBuilder WithPackage(string package, string? extra = null)
    {
        Packages.Add((package, extra));
        return this;
    }

    /// Use the given packages eg: `siunitx, mathsymb`
    public LatexBuilder WithPackages(params string[] packages)
    {
        foreach (var pkg in packages)
            WithPackage(pkg);
        return this;
    }

    /// Add a new alias (command with no args)
    public LatexBuilder WithAlias(string name, string body) => WithCommand(name, 0, body);

    /// Add a new command to the report
    public LatexBuilder WithCommand(string name, int args, string body)
    {
        Commands.Add((name, args, body));
        return this;
    }

    // public void SetLength(string name, string value)
    // {
    //     WriteLn($@"\setlength{{\{name}}}{{{value}}}");
    // }

    public override string ToString() => $"Latex {Class} - {Title}";
}
