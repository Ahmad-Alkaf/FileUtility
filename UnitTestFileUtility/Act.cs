using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestFileUtility {
  internal class Act : IDisposable {
    public static string DesktopPath = "C:\\Users\\" + Environment.UserName + "\\Desktop";

    public Act() {
    }

    public string CreateEmptyDirectory(string name = "") {
      string path = DesktopPath + "\\UnitTestEmptyDirectory" + (string.IsNullOrEmpty(name) ? "" : "\\" + name);
      if(Directory.Exists(path))
        Directory.Delete(path, true);
      Directory.CreateDirectory(path);
      return path;
    }
    public void Dispose() {
      string[] paths = new string[] { DesktopPath + "\\UnitTestEmptyDirectory" };
      foreach(string path in paths) {
        if(Directory.Exists(path)) {
          Directory.Delete(path, true);
        }
      }
    }

  }
}
