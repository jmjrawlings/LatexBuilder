namespace LatexBuilder.Tests;

using Xunit.Abstractions;

public class UnitTests(ITestOutputHelper output)
{
    [Fact]
    public void TestWriteReadme()
    {
        LatexDocument doc = DocumentBuilder
            .Article()
            .WithTitle("LatexBuilder")
            .WithAuthor("Justin Rawlings")
            .WithDate(DateTime.Today)
            .WithPackage("booktabs")
            .WithPackage("amsmath")
            .WithTitlePage()
            .Build();

        using (doc.Section("Introduction"))
            doc.WriteLn("This is an introduction.");

        using (doc.Section("Sections"))
        {
            doc.WriteLn("""
            Document levels are maintained internally by the \emph{LatexDocument}.
            So there is no need to remember which section/subsection/paragraph level
            you are at.

            Document scopes are implemented using readonly ref structs so they should
            quite performant to use which can be a concern when writing large documents.
             
            """);

            using (doc.Section("A subsection"))
                doc.WriteLn("This is now a subsection as we were already inside a Section.");

            using (doc.Section("Another subsection"))
                using (doc.Section("A subsubsection"))
                    using (doc.Section("A paragraph"))
                        using (doc.Section("A subparagraph"))
                            using (doc.Section("Nesting maxes out at subparagraph"))
                                doc.WriteLn("That wasnt so bad.");
        }
        int alpha = 1;
        int beta = 2;
        int gamma = alpha + beta;
        using (doc.Section("Some math"))
        {
            doc.Para("""
            The string interpolation features in newer versions of C\# is 
            very well suited to mixing code variables and latex markup.
            """);
            
            doc.WriteLn(@"We seek to calculate the value for $\gamma$:");
            using (doc.Env("align*"))
                doc.WriteLn($$"""
                  \gamma &= \alpha + \beta \tag{a tag} \\
                  \alpha &= {{alpha}} \\
                  \beta &= {{beta}} \\
                  \gamma &= {{alpha}} + {{beta}} \\
                         &= {{gamma}}
                  """);
        }
        using (doc.Section("Other things")){
        
            using (doc.Section("Lists"))
            {
                using (doc.Itemize())
                {
                    doc.Item("First item");
                    using (doc.Itemize())
                    {
                        doc.Item("Nested item 1");
                        doc.Item("Nested item 2");
                    }

                    doc.Item("Second item");
                }
            }

            using (doc.Section("Tables"))
            {
                using (doc.Table("r|r|r", caption: "A table caption", label: "table"))
                {
                    doc.TopRule();
                    doc.WriteRow("n", "variable", "value");
                    doc.MidRule();
                    doc.WriteRow("1", "$\\alpha$", alpha.Tex());
                    doc.WriteRow("2", "$\\beta$", beta.Tex());
                    doc.WriteRow("3", "$\\gamma$", gamma.Tex());
                    doc.BottomRule();
                }
            }
            
            doc.BeginSection("Manual section");
            doc.WriteLn(@"You can also manage sections explicitly using \emph{BeginSection} and \emph{EndSection}");
            doc.EndSection();    
        }
        
        var tex = doc.ToString();
        var file = doc.WriteToFile("README.tex");
        output.WriteLine(tex);
        output.WriteLine("");
        output.WriteLine("");
        output.WriteLine(file.FullName);
        var a = 2;
    }
}