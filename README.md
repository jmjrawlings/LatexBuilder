# LatexBuilder

Helper classes for building [LaTeX](https://www.latex-project.org/) documents.

The API is somewhat inspired by StringBuilder as it's really just a thin wrapper of it.


## Quickstart

```csharp
using LatexBuilder;

LatexDocument doc = LatexBuilder
    .Article()
    .WithTitle("A new document")
    .WithAuthor("Justin Rawlings")
    .WithPackage("booktabs")
    .Build();

using (doc.Section("Introduction"))
    doc.WriteLn("This is an introduction");

using (doc.Section("A Section")){
    doc.WriteLn("Section levels are maintained interally");
    
    using (doc.Section("A subsection"))
        doc.WriteLn("This is in a subsection");
    
    using (doc.Section("Another subsection")){
        using (doc.Environment("align*"))
            doc.WriteLn($$"""
                \alpha &= 1 \\
                \beta &= 2 \\
                \intertext{thus}
                \alpha + \beta &= 3
            """);
    }
    
    doc.BeginSection("Manul section");
    doc.WriteLn("You can also begin and end sections explicity");
    doc.EndSection();    
}

var tex = doc.WriteToFile("report.tex");
```