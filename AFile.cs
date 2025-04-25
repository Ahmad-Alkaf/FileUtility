using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUtility {
  /// <summary>
  /// Represent a File class for file instance with multiple helpful functionality.
  /// </summary>
  public class AFile : AFileDirectory {
    public string NameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(Name);
    public string Extension => System.IO.Path.GetExtension(Name);
    public AFile(ADirectory parent, string[] aliasWithoutExtension, string extension) :
        base(parent, aliasWithoutExtension.Select(v => v + "." + extension).ToArray()) {
      Debug.Assert(!extension.Contains("."), "Extension should not contains \".\". extension=" + extension);
    }
    public AFile(ADirectory parent, string name) : base(parent, new string[] { name }) { }
    public AFile(ADirectory parent, string[] aliasWithExtension) : base(parent, aliasWithExtension) { }
    public AFile(string fullPath) : this(ADirectory.FromFullPath(System.IO.Path.GetDirectoryName(fullPath)), System.IO.Path.GetFileName(fullPath)) { }


    /// <summary>
    /// Copy current file (if exists) to target.
    /// targetPath should include the file name. If source file doesn't exist, NO exception will be thrown.
    /// </summary>
    /// <returns>True if this file exists and copied successfully, false otherwise</returns>
    public async Task<bool> CopyTo(string targetPath, bool overWrite = true) {
      return await CopyTo(AFile.FromFullPath(targetPath), overWrite);
    }

    /// <summary>
    /// Target should includes the file name.
    /// </summary>
    /// <returns>True if copied successfully, false if target file is exists while overWrite is false, and false if current file not exists</returns>
    public async Task<bool> CopyTo(AFile target, bool overWrite = true) {
      if(await PathIfExist() is string fromPath) {
        if(!overWrite && await target.Exists())
          return false; // Target already exists, and overWrite is false.
        if(!await target.Parent.Exists())
          await target.Parent.Create();
        await FileAsync.Copy(fromPath, await target.Path(), overWrite);
        return true;
      }
      return false;
    }
    /// <summary>
    /// 0 length if file not exists. It ignores empty lines.
    /// </summary>
    public async Task<string[]> ReadAllLines() {
      if(await Exists())
        return await FileAsync.ReadAllLines(await Path());
      return new string[] { };
    }

    /// <summary>
    /// Null if file not exists.
    /// </summary>
    public async Task<string> ReadAllText() {
      if(await Exists())
        return await FileAsync.ReadAllText(await Path());
      return null;
    }

    /// <summary>
    /// Null if file not exists.
    /// </summary>
    public string ReadAllTextSync() {
      if(ExistsSync())
        return File.ReadAllText(PathSync());
      return null;
    }
    /// <summary>
    /// Writing `null` or empty string, will create an empty file.
    /// </summary>
    public async Task WriteAllText(string text) {
      if(!await Parent.Exists())
        await Parent.Create();
      await FileAsync.WriteAllText(await Path(), text);
    }
    /// <summary>
    /// Writing `null` or empty array, will create an empty file.
    /// </summary>
    public async Task WriteAllLines(string[] lines) {
      if(!await Parent.Exists())
        await Parent.Create();
      await FileAsync.WriteAllLines(await Path(), lines);
    }
    public void WriteAllTextSync(string text) {
      if(!Parent.ExistsSync())
        Parent.CreateSync();
      File.WriteAllText(PathSync(), text);
    }

    public override async Task Delete(bool recycleBin = false) {
      if(await PathIfExist() is string path) {
        if(recycleBin) {
          await Util.RetryOperation(() => Task.Run(() => {
            const int ssfBITBUCKET = 0xa;
            dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            var bin = shell.Namespace(ssfBITBUCKET);
            bin.MoveHere(path);
          })
          );
        } else
          await FileAsync.Delete(path);
      }
    }
    /// <summary>
    /// Move this file to the provided path. Full file name is included in the provided path.
    /// </summary>
    /// <returns>True if moving done successfully. False otherwise.</returns>
    public async Task<bool> MoveTo(AFile toFile) {
      if(!await Exists()) return false;
      if(await toFile.Exists())
        await toFile.Delete(true);//if file exists in destination then remove it to recycle bin.
      await FileAsync.Move(await Path(), await toFile.Path());
      return true;
    }
    /// <summary>
    /// Run the file if it exists, otherwise show an error dialog.
    /// </summary>
    /// <param name="waitForExit"></param>
    /// <param name="args"></param>
    /// <returns>The Process object of the program. If file not found it returns null</returns>
    public async Task<Process> LaunchProgram(bool waitForExit, string args = null) {
      if(await PathIfExist() is string path) {

        var p = new Process();
        p.StartInfo.FileName = path;// e.g "notepad.exe";
        p.StartInfo.UseShellExecute = true;
        p.StartInfo.Arguments = args;
        p.Start();
        if(waitForExit)
          p.WaitForExit();
        return p;
      }
      return null;
    }
    public Task<List<AFile>> ExistedPathAlias() => base.ExistedPathAlias<AFile>();

    public static AFile FromFullPath(string fullPath) => new AFile(fullPath);
  }
}
