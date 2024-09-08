namespace LatexBuilder;

/// <summary>
/// Builds a LaTeX document by correctly setting
/// the documentclass as well as any other metadata
/// </summary>
public class DocumentBuilder
{
    public string? Title { get; set; }

    public string? Author { get; set; }

    public string? Date { get; set; }
    
    public DocumentClass Class { get; private set; } = DocumentClass.Article;
    
    public DocumentLevel Level { get; private set; }
    
    public List<(string, string?)> Packages { get; private set; }

    public List<(string, int, string)> Commands { get; private set; }

    public List<(string, string)> Lengths { get; private set; }

    public bool TitlePage { get; set; }
    
    public DocumentBuilder()
    {
        Packages = new List<(string, string?)>();
        Commands = new List<(string, int, string)>();
        Lengths = new List<(string, string)>();
    }

    public DocumentBuilder WithClass(DocumentClass cls)
    {
        Class = cls;
        Level = cls switch
        {
            DocumentClass.Book => DocumentLevel.Chapter,
            _ => DocumentLevel.Section
        };
        return this;
    }

    public DocumentBuilder WithTitle(string? title)
    {
        Title = title;
        return this;
    }
    
    public DocumentBuilder WithDate(string date)
    {
        Date = date;
        return this;
    }
    
    public DocumentBuilder WithTodaysDate() => WithDate("\\today");
    
    public DocumentBuilder WithDate(DateTime date)
    {
        string dt = date.ToLongDateString();
        Date = dt;
        return this;
    }
    
    public DocumentBuilder WithTitlePage(bool? ok = true)
    {
        TitlePage = ok ?? false;
        return this;
    }

    public DocumentBuilder WithAuthor(string? author)
    {
        Author = author;
        return this;
    }


    public DocumentBuilder WithLength(string name, string val)
    {
        Lengths.Add((name, val));
        return this;
    }

    public static DocumentBuilder Default() =>
        new DocumentBuilder();

    public LatexDocument Build()
    {
        var doc = new LatexDocument(Level);
        var cls = Class switch
        {
            DocumentClass.Article => "article",
            DocumentClass.Book => "book",
            DocumentClass.Report => "report",
            _ => throw new ArgumentException(Class.ToString())
        };
        doc.Command("documentclass", cls);

        foreach (var (pkg, extra) in Packages)
            doc.Command("usepackage", pkg, extra);
        
        foreach (var (name, len) in Lengths)
            doc.Command2("setlength", $"\\{name}", len);

        doc.NewLine();
        if (Title is not null)
            doc.Command("title", Title);
        if (Author is not null)
            doc.Command("author", Author);
        if (Date is not null)
            doc.Command("date", Date);
        doc.Env("document");

        foreach (var (name, args, body) in Commands)
            if (args is 0)
                doc.WriteLn($@"\newcommand{{\{name}}}{{{body}}}");
            else
                doc.WriteLn($@"\newcommand{{\{name}}}[{args}]{{{body}}}");

        if (TitlePage)
            doc.Command("maketitle");

        doc.NewLine();
        return doc;
    }

    public static DocumentBuilder Report() => Default().WithClass(global::LatexBuilder.DocumentClass.Report);

    public static DocumentBuilder Book() => Default().WithClass(global::LatexBuilder.DocumentClass.Book);

    public static DocumentBuilder Article() => Default().WithClass(global::LatexBuilder.DocumentClass.Article);

    /// Use the given package eg: `siunitx`
    public DocumentBuilder WithPackage(string package, string? extra = null)
    {
        Packages.Add((package, extra));
        return this;
    }

    /// Use the given packages eg: `siunitx, mathsymb`
    public DocumentBuilder WithPackages(params string[] packages)
    {
        foreach (var pkg in packages)
            WithPackage(pkg);
        return this;
    }

    /// Add a new alias (command with no args)
    public DocumentBuilder WithAlias(string name, string body) => WithCommand(name, 0, body);

    /// Add a new command to the report
    public DocumentBuilder WithCommand(string name, int args, string body)
    {
        Commands.Add((name, args, body));
        return this;
    }
    
    public override string ToString() => $"Latex {Class} - {Title}";
}