﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
    public async Task CopyTo(string targetPath, bool overWrite = true) {
      await CopyTo(AFile.FromFullPath(targetPath), overWrite);
    }

    /// <summary>
    /// Copy current file (if exists) to target.
    /// </summary>
    /// <returns>True if copied successfully, false if target file is exists while overWrite is false, and false if current file not exists</returns>
    public async Task CopyTo(AFile target, bool overWrite = true) {
      if(await PathIfExists() is string fromPath) {
        if(!overWrite && await target.Exists())
          throw new FileLoadException("Target file already exists. overWrite is false. target=" + target.PathSync());
        if(!await target.Parent.Exists())
          await target.Parent.Create();
        await FileAsync.Copy(fromPath, await target.Path(), overWrite);
      } else
        throw new FileNotFoundException("Source file not found. source=" + PathSync());
    }
    /// <summary>
    /// 0 length if file not exists. It ignores empty lines.
    /// </summary>
    public async Task<string[]> ReadAllLines() {
      if(await PathIfExists() is string path)
        return await FileAsync.ReadAllLines(path);
      return new string[] { };
    }

    /// <summary>
    /// Null if file not exists.
    /// </summary>
    public async Task<string> ReadAllText() {
      if(await PathIfExists() is string path)
        return await FileAsync.ReadAllText(path);
      return null;
    }

    /// <summary>
    /// Null if file not exists.
    /// </summary>
    public async Task<byte[]> ReadAllBytes() {
      if(await PathIfExists() is string path)
        return await FileAsync.ReadAllBytes(path);
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
      if(await PathIfExists() is string path) {
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
    /// Move this file to the provided file. 
    /// </summary>
    /// <returns>True if moving done successfully. False if this file (source) doesn't exist, and false if toFile (destination) already exists while overWrite is false.</returns>
    public async Task<bool> MoveTo(AFile toFile, bool overWrite = true) {
      if(await PathIfExists() is string fromPath) {
        string toFilePathIfExists = await toFile.PathIfExists();
        if(!overWrite && toFilePathIfExists is string)
          return false; // Target already exists, and overWrite is false.

        if(toFilePathIfExists == null && !await toFile.Parent.Exists())
          await toFile.Parent.Create();

        if(overWrite && toFilePathIfExists is string) {
          await FileAsync.Delete(toFilePathIfExists);
          await FileAsync.Move(fromPath, toFilePathIfExists);
        } else
          await FileAsync.Move(fromPath, await toFile.Path()); // toFilePathIfExists is null
        return true;
      }
      return false;
    }
    /// <summary>
    /// Run the file if it exists, otherwise show an error dialog.
    /// </summary>
    /// <param name="waitForExit"></param>
    /// <param name="args"></param>
    /// <returns>The Process object of the program. If file not found it returns null</returns>
    public async Task<Process> LaunchProgram(bool waitForExit, string args = null) {
      if(await PathIfExists() is string path) {
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
    ///<summary>
    /// Creates a uniquely named, zero-byte temporary file on Temp directory provided by the OS.
    ///</summary>
    public static AFile GetTempFile() => FromFullPath(System.IO.Path.GetTempFileName());

    /// <summary>
    /// Update the file without any race condition errors that lead to lose of data integrity. 
    /// Used for files that many processes may try to write/modify at the same time.
    /// Not recommended if you want to read only access.
    /// Uses File.Replace(); which is atomic on NTFS. So, no half-written files if suddenly crashed while writing.
    /// </summary>
    /// <param name="update">A function that its parameter is the latest file content in string. And it returns the new file content.
    /// Function must takes less than 5 seconds to complete, this is why it's synchronize.
    /// Param is Empty string if file not exists.</param>
    /// <returns>New data to be written into the file</returns>
    public Task ConcurrentUpdate(Func<string, string> update) {
      return Util.RetryOperation(async () => {
        FileStream fs = null;
        try {
          fs = new FileStream(
                  await Path(),
                  FileMode.OpenOrCreate,
                  FileAccess.ReadWrite,
                  FileShare.Read);

          // Read through the same stream
          StreamReader sr = null;
          try {

            sr = new StreamReader(fs, encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
            string content = sr.BaseStream.Length > 0
                          ? sr.ReadToEnd()
                          : "";
            sr.Dispose();

            string newContent = update(content);
            if(newContent == null)
              return;

            // Re-wind and overwrite
            fs.SetLength(0);
            using(var sw = new StreamWriter(fs)) {
              sw.Write(newContent);
            }
          } finally {
            sr?.Dispose();
          }
        } finally {
          fs?.Dispose();
        }
      }, 150, 50);

    }
  }
}
