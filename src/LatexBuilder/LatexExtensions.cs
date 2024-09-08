namespace LatexBuilder;

public static class LatexExtensions
{
    public static string Ref(this string s) => $"\\ref{{{s}}}";

    public static string Tex(this int i) => $"${i}$";

    public static string Tex(this double d, string format) => $"${d.ToString(format)}$";
}