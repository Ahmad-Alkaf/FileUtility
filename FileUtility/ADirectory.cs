using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUtility {
  /// <summary>
  /// Represent a Directory class for Directory instance with multiple helpful functionality.
  /// </summary>
  public class ADirectory : AFileDirectory {
    public ADirectory(ADirectory parent, string name) : base(parent, new string[] { name }) { }
    public ADirectory(ADirectory parent, string[] alias) : base(parent, alias) { }

    /// <returns>Array of files path string that are inside current directory.</returns>
    public Task<List<string>> GetFilesPaths() => Task.Run(async () => {
      List<string> paths = new List<string>();
      foreach(var dir in await ExistedPathAlias())
        paths.AddRange(await DirectoryAsync.GetFiles(await dir.Path()));
      return paths;
    });

    /// <returns>Array of AFile instances that are inside the current directory.</returns>
    public Task<List<AFile>> GetFiles() => Task.Run(async () => {
      return (await GetFilesPaths()).Select(v => new AFile(this, System.IO.Path.GetFileName(v))).ToList();
    });

    /// <returns>Array of directories path string that are inside current directory.</returns>
    public Task<List<string>> GetDirectoriesPath() => Task.Run(async () => {
      List<string> paths = new List<string>();
      foreach(var dir in await ExistedPathAlias())
        paths.AddRange(await DirectoryAsync.GetDirectories(await dir.Path()));
      return paths;
    });

    /// <returns>Array of ADirectory instances that are inside the current directory.</returns>
    public Task<List<ADirectory>> GetDirectories() => Task.Run(async () => {
      return (await GetDirectoriesPath()).Select(v => new ADirectory(this, System.IO.Path.GetFileName(v))).ToList();
    });

    /// <returns>Array of both ADirectory and AFile instances that are inside the current directory.</returns>
    public Task<List<AFileDirectory>> GetFilesDirectories() => Task.Run(async () => {
      List<AFileDirectory> f = new List<AFileDirectory>();
      f.AddRange(await GetDirectories());
      f.AddRange(await GetFiles());
      return f;
    });

    /// <summary>
    /// Create directory to current path if not exist
    /// </summary>
    public async Task Create() {
      if(!await Exists()) {
        if(Parent != null && !await Parent.Exists())
          await CreateParentDirectory(Parent);
        await DirectoryAsync.CreateDirectory(await Path());
      }
    }
    public void CreateSync() {
      if(!ExistsSync()) {
        if(Parent != null && !Parent.ExistsSync())
          CreateParentDirectorySync(Parent);
        Directory.CreateDirectory(PathSync());
      }
    }

    private async Task CreateParentDirectory(ADirectory parent) {
      if(parent != null && !await parent.Exists()) {
        await CreateParentDirectory(parent.Parent);
        await parent.Create();
      }
    }
    private void CreateParentDirectorySync(ADirectory parent) {
      if(parent != null && !parent.ExistsSync()) {
        CreateParentDirectorySync(parent.Parent);
        parent.CreateSync();
      }
    }


    public override async Task Delete(bool recycleBin = false) {
      if(await PathIfExists() is string path) {
        if(recycleBin) {
          await Task.Run(() => {
            const int ssfBITBUCKET = 0xa;
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            var bin = shell.Namespace(ssfBITBUCKET);
            bin.MoveHere(path);
          });
        } else
          await DirectoryAsync.Delete(path, true);
      }
    }
    /// <summary>
    /// Concatenate the file to current directory.
    /// Ex: ADirectory="...Desktop/TechTroniX".CFile("Restore Point.png") => CFile="...Desktop/TechTroniX/Restore Point.png"
    /// </summary>
    /// <param name="name">Must contain the file extension. Ex:</param>
    /// <returns>AFile that represents a child file of current directory.</returns>
    public AFile CFile(string name) {
      return new AFile(this, name);
    }

    /// <summary>
    /// Extension must NOT contains the '.' dot.
    /// </summary>
    public AFile CFile(string[] nameAlias, string extension) {
      return new AFile(this, nameAlias, extension);
    }

    public AFile CFile(string[] nameAliasWithExtension) {
      return new AFile(this, nameAliasWithExtension);
    }

    /// <summary>
    /// Concatenate the directory to current directory.
    /// </summary>
    /// <returns>ADirecotry that represents a child directory of current directory.</returns>
    public ADirectory CDirectory(string name) {
      return new ADirectory(this, name);
    }
    /// <summary>
    /// Concatenate the directory to current directory.
    /// </summary>
    /// <returns>ADirecotry that represents a child directory of current directory.</returns>
    public ADirectory CDirectory(string[] name) {
      return new ADirectory(this, name);
    }

    /// <summary>
    /// Launch Explorer to the current directory. 
    /// </summary>
    public async Task LaunchExplorer() {
      if(await PathIfExists() is string path) {
        var p = new Process();
        p.StartInfo.FileName = "explorer.exe";
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.Arguments = path;
        p.Start();
      } else
        throw new FileNotFoundException("Directory path not found to be opened", await Path());
    }

    /// <summary>
    /// Convert a nested directory string path to ADirectory. Ex: "\\TTX-SERVER\PUBLIC\TechTroniX"  => new ADirectory( new ADirectory( new ADirectory( null, "TechTroniX"), "PUBLIC"), "\\TTX-SERVER")
    /// </summary>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public static ADirectory FromFullPath(string fullPath) {
      Debug.Assert(!string.IsNullOrWhiteSpace(fullPath), "Path cannot be null or empty. fullPath=" + fullPath);

      // Remove any trailing directory separators
      fullPath = fullPath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
      // Convert any environment variables in the path to their actual values. Ex: %USERPROFILE%
      fullPath = Environment.ExpandEnvironmentVariables(fullPath);
      // Remove trailing separators from the root name
      fullPath = fullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
      // Extract the root of the path
      string root = System.IO.Path.GetPathRoot(fullPath);
      //if root:
      if(root is null || root == fullPath) {
        return new ADirectory(null, fullPath);
      }

      // Remove the root from the full path to get the relative path
      string relativePath = fullPath.Substring(root.Length);


      // Split the relative path into directories
      string[] directories = relativePath.Split(new char[] { System.IO.Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

      // Create the root directory node
      ADirectory current = new ADirectory(null, root);

      // Build the directory hierarchy
      foreach(string dir in directories) {
        if(current.Name == "Users" && (dir == "Public" || dir == Environment.UserName))
          current = P.User.I;
        else
          current = new ADirectory(current, dir);
      }

      return current;
    }
    public Task<List<ADirectory>> ExistedPathAlias() => base.ExistedPathAlias<ADirectory>();
  }
}
