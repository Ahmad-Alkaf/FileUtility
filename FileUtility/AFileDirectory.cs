using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace FileUtility {
  /// <summary>
  /// Represent a parent class for File/Directory instance with multiple helpful functionality.
  /// </summary>
  public abstract class AFileDirectory {
    /// <summary>
    /// Name doesn't include the full path. If alias exists then returns first alias.
    /// </summary>
    public string Name {
      get {//may contain 
           //char separator = System.IO.Path.DirectorySeparatorChar;
           //string firstAlias = Alias[0].Replace(System.IO.Path.AltDirectorySeparatorChar, separator).Trim(separator);
           //if(firstAlias.Contains(separator)) {
           //    string[] relatives = firstAlias.Split(separator);
           //    return relatives[relatives.Length - 1];
           //} else return firstAlias;
        return Alias[0];
      }
    }
    /// <summary>
    /// If file/directory Exists then return the exact Alias name.
    /// </summary>
    public async Task<string> ExactName() {
      return System.IO.Path.GetFileName(await Path());
    }
    /// <summary>
    /// If file/directory Exists then return the exact Alias name.
    /// </summary>
    public string ExactNameSync() {
      return System.IO.Path.GetFileName(PathSync());
    }
    /// <summary>
    /// Name alias.
    /// </summary>
    public string[] Alias { get; }
    /// <summary>
    /// Nullable field. If null then directory is the root. Ex: "C:\Users" directory has no parent directory because it's on "C:\" drive.
    /// </summary>
    public ADirectory Parent { get; }
    public bool IsRoot { get { return Parent == null; } }

    public AFileDirectory(ADirectory parent, string[] nameAlias) {
      Debug.Assert(nameAlias != null && nameAlias.Length > 0, "Name alias of a file/folder should has at least one item. nameAlias=" + nameAlias);
      Alias = nameAlias.ToHashSet().ToArray();//Remove duplicate.
      Parent = parent;
    }

    /// <returns>string of the existed path (if any; iterate alias paths if one exist then return it), if path doesn't exist return the default path Path.Combine(parentPath, Name).</returns>
    public async Task<string> Path() {
      string p = await PathIfExists();
      return p is string path ?
          path
          : (Parent == null ?
          Alias[0] :
          System.IO.Path.Combine(await Parent.Path(), Alias[0]));
    }
    /// <summary>
    /// Retrieve the path synchronously.
    /// </summary>
    public string PathSync() {
      if(Alias.Length == 1)
        return Parent == null ? Name : System.IO.Path.Combine(Parent.PathSync(), Name);

      foreach(string name in Alias) {
        var path = Parent == null ? name : System.IO.Path.Combine(Parent.PathSync(), name);
        if(this is AFile ? File.Exists(path) : Directory.Exists(path))
          return path;
      }
      return Parent == null ?
          Name :
          System.IO.Path.Combine(Parent.PathSync(), Name);
    }
    /// <returns>String of the path if exists. Otherwise null.</returns>
    public Task<string> PathIfExists() => Task.Run(async () => {
      foreach(string path in await Paths(this)) {
        if(this is AFile ? await FileAsync.Exists(path) : await DirectoryAsync.Exists(path))
          return path;
      }
      return null;
    });
    /// <summary>
    /// Array of all possible paths for each AFileDirectory alias and its parent aliases until the root directory.
    /// Ex: if directory "C:\Users\TechTroniX" has alias "C:\Users\Public" 
    /// then the parameter provided is Restore Point AFile with aliases ["Restore Point.png", "restore.png"]. 
    /// The return will ["C:\Users\Ahmed\Desktop\TechTroniX\Restore Point.png",
    ///                  "C:\Users\Ahmed\Desktop\TechTroniX\restore.png",
    ///                  "C:\Users\Public\Desktop\TechTroniX\Restore Point.png",
    ///                  "C:\Users\Public\Desktop\TechTroniX\restore.png"] 
    /// Alias order reflect returned order.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    private Task<List<string>> Paths(AFileDirectory f) => Task.Run(async () => {
      List<string> paths = new List<string>();
      if(f.Parent != null) {
        foreach(var p in await Paths(f.Parent))
          foreach(var a in f.Alias)
            if(f is AFile ? await FileAsync.Exists(System.IO.Path.Combine(p, a)) : await DirectoryAsync.Exists(System.IO.Path.Combine(p, a)))
              paths.Add(System.IO.Path.Combine(p, a));
      } else
        paths.AddRange(f.Alias);
      return paths;
    });
    //public Task<List<string>> Existed()
    /// <summary>
    /// All alias paths that are currently existed.
    /// </summary>
    protected Task<List<T>> ExistedPathAlias<T>() where T : AFileDirectory => Task.Run(async () => {
      List<AFileDirectory> existedF = new List<AFileDirectory>();
      AFileDirectory f;
      List<string> paths = await Paths(this);

      //todo use Task.WhenAll
      foreach(var p in paths)
        if(this is AFile ? await (f = new AFile(p)).Exists() : await (f = ADirectory.FromFullPath(p)).Exists())
          existedF.Add(f);
      return existedF.OfType<T>().ToList();
    });
    public async Task<bool> Exists() {
      return this is AFile ? await FileAsync.Exists(await Path()) : await DirectoryAsync.Exists(await Path());
    }
    public bool ExistsSync() {
      return this is AFile ? File.Exists(PathSync()) : Directory.Exists(PathSync());
    }

    /// <summary>
    /// Delete a file or directory.
    /// </summary>
    public abstract Task Delete(bool recycleBin = false);

    /// <returns>True if File/Directory is a system file/directory. False otherwise</returns>
    public bool IsSystem() {
      return (Alias.Select(v => v.ToLower()).Contains("desktop.ini") && this is AFile)
          || (Alias.Select(v => v.ToLower()).Contains("thumbs.db") && this is AFile)
          || (Alias.Select(v => v.ToLower()).Contains("hwinfo64.ini") && this is AFile)
          ;
    }
    public override string ToString() {
      return Name;
    }
    public override bool Equals(object obj) {
      return GetType() == obj?.GetType() && obj is AFileDirectory o && o is AFile == this is AFile
          && (Name == o.Name || Alias.Contains(o.Name) || o.Alias.Contains(Name))
          && (
          (Parent is AFileDirectory && o.Parent is AFileDirectory
          && (Parent == o.Parent || Parent.Alias.Contains(o.Parent.Name) || o.Parent.Alias.Contains(Parent.Name))
          ) 
          || (Parent == null && o.Parent == null)
          );
    }
    public override int GetHashCode() {
      return Name.GetHashCode();
    }
  }

}
