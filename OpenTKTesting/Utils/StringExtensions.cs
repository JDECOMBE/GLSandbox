namespace OpenTKTesting.Utils;

public static class StringExtensions
{
    public static string GetFileContent(this string path)
    {
        return !File.Exists(path) ? null : File.ReadAllText(path);
    }
}