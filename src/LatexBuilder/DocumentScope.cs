namespace LatexBuilder;

/// <summary>
/// A disposable that performs a side effect when disposed.
/// in our case this means closing open brackets, or
/// dedenting.
/// </summary>
public readonly ref struct DocumentScope(Action onDispose) 
{
    public void Dispose()
    {
        onDispose();
    }
}