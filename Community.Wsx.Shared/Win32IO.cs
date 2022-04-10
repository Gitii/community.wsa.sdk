namespace Community.Wsx.Shared;

internal class Win32IO : IIo
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }
}
