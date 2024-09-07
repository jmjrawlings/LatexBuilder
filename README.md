# LatexBuilder

Like a StringBuilder but for LaTeX documents.

## Quickstart

```csharp
using LatexBuilder;

var doc = LatexBuilder
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