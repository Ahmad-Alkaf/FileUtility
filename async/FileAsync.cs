using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FileUtility {
  /// <summary>
  /// To avoid running sync I/O methods, we should never import File/Directory classes.
  /// </summary>
  internal class FileAsync {
    public static Task<bool> Exists(string path) {
      return Util.RetryOperation(() => Task.Run(() => File.Exists(path)));
    }
    /// <summary>
    /// Copy a file from one location to another.
    /// </summary>
    /// <param name="fromPath"></param>
    /// <param name="toPath"></param>
    /// <param name="overWrite"></param>
    /// <returns>True if successfully copied, false if something went wrong.</returns>
    public static async Task Copy(string fromPath, string toPath, bool overWrite = true) {
      await Util.RetryOperation(() => Task.Run(() => File.Copy(fromPath, toPath, overWrite)));
    }
    public static async Task Move(string fromPath, string toPath) {
      await Util.RetryOperation(() => Task.Run(() => File.Move(fromPath, toPath)));
    }
    public static async Task Delete(string path) {
      await Util.RetryOperation(() => Task.Run(() => File.Delete(path)));
    }
    /// <summary>
    /// readAllLines if an exception is thrown then retry "retryedMax" times. If all failed an error message will be shown with ignore capablity which will retrun an empty array.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="retryed">number of retryed that occur</param>
    /// <param name="retryedMax">number of maximum retry times untill giveup</param>
    /// <returns></returns>
    public static async Task<string[]> ReadAllLines(string path) {
      return await Util.RetryOperation(async () => {
        return (await ReadAllText(path)).Replace("\r\n", "\n").Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
      });
    }
    /// <summary>
    /// Read all text from a file.
    /// It handle exception with an Error Dialog that has Retry button.
    /// </summary>
    /// <param name="path"></param>
    public static async Task<string> ReadAllText(string path) {
      return await Util.RetryOperation(() => Task.Run(() => {
        return File.ReadAllText(path);
      }));
    }
    public static async Task WriteAllText(string path, string text) {

      await Util.RetryOperation(() => Task.Run(() => {
        using(FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)) {
          fileStream.SetLength(0);
          byte[] bytes = new UTF8Encoding(true).GetBytes(text ?? "");
          fileStream.Write(bytes, 0, bytes.Length);
        }
      }));
    }
    public static async Task WriteAllLines(string path, string[] lines) {
      await Util.RetryOperation(async () => {
        await WriteAllText(path, string.Join("\r\n", lines ?? new string[] { }));
      });
    }
    public static async Task<byte[]> ReadAllBytes(string path) {
      return await Util.RetryOperation(() => Task.Run(() => File.ReadAllBytes(path)));
    }
  }
}
