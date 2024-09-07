namespace LatexBuilder;

public static class LatexExtensions
{
    public static string Ref(this string s) => $"\\ref{{{s}}}";
}