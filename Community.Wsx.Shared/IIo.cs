namespace Community.Wsx.Shared;

public interface IIo
{
    public abstract bool DirectoryExists(string path);
    public abstract bool FileExists(string path);
    public abstract string Combine(params string[] paths);
    public abstract string GetFullPath(string path);
}
