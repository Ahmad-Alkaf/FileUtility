using System;
using System.Runtime.InteropServices;

namespace FileUtility {

  /// <summary>
  /// Uses the Windows Known Folder API, Screenshots folder is a known folder that Windows only should manipulate. The helper will help you to get the path of the Screenshots folder.
  /// </summary>
  public class ScreenshotsFolderHelper {
    // GUID for the Screenshots Known Folder
    private static readonly Guid ScreenshotsFolderGuid =
        new Guid("b7bede81-df94-4682-a7d8-57a52620b86f");

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
        uint dwFlags,
        IntPtr hToken,
        out IntPtr pszPath
    );

    public static string GetScreenshotsFolderPath() {
      IntPtr pathPtr;
      // Call the API to get the path (even if the folder doesn't exist yet)
      int result = SHGetKnownFolderPath(ScreenshotsFolderGuid, 0, IntPtr.Zero, out pathPtr);
      if(result == 0) {
        string path = Marshal.PtrToStringUni(pathPtr);
        Marshal.FreeCoTaskMem(pathPtr);
        return path;
      }
      throw new Exception("Failed to retrieve Screenshots folder path.");
    }
  }
}
