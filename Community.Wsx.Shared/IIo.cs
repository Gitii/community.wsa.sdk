using System.IO;

namespace Community.Wsx.Shared;

/// <summary>
/// Selection of most used methods from <see cref="System.IO.Path"/>, <see cref="Directory"/> and <see cref="File"/>.
/// </summary>
public interface IIo
{
    /// <inheritdoc cref="Directory.Exists"/>
    public abstract bool DirectoryExists(string path);

    /// <inheritdoc cref="File.Exists"/>
    public abstract bool FileExists(string path);

    /// <inheritdoc cref="Path.Combine(string[])"/>
    public abstract string Combine(params string[] paths);

    /// <inheritdoc cref="Path.GetFullPath(string)"/>
    public abstract string GetFullPath(string path);
}
