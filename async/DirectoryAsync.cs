using System.IO;
using System.Threading.Tasks;

namespace FileUtility {
  /// <summary>
  /// To avoid running sync I/O methods, we should never import File/Directory classes.
  /// </summary>
  internal class DirectoryAsync {
    public static Task<bool> Exists(string path) {
      return Util.RetryOperation(() => Task.Run(() => Directory.Exists(path)));
    }
    public static Task Delete(string path, bool recursive = true) {
      return Util.RetryOperation(() => Task.Run(() => {
        Directory.Delete(path, recursive);
      }));
    }
    public static Task<string[]> GetDirectories(string path) {
      return Util.RetryOperation(() => Task.Run(() => Directory.GetDirectories(path)));
    }
    public static Task<string[]> GetFiles(string path) {
      return Util.RetryOperation(() => Task.Run(() => {
        if(Directory.Exists(path))
          return Directory.GetFiles(path);
        return new string[] { };
      }));
    }
    public static async Task CreateDirectory(string path) {
      await Util.RetryOperation(() => Task.Run(() =>
         Directory.CreateDirectory(path))
      );
    }
  }
}
