using System;

namespace FileUtility {
    /// <summary>
    /// Represent Paths utilities. 
    /// Used to navigate an ADirecotry instance with ease. 
    /// Ex: P.SERVER.PUBLIC.I => "\\TTX-SERVER\PUBLIC" path.
    /// Using ADirectory class to easily using file system functionality.
    /// Ex: P.Desktop.MyFolder.I.Exist() => boolean indicate whether that path "C:\Users\USER_NAME\Desktop\MyFolder" exist or not.
    /// </summary>
  internal class P {


  public static class User {
      public static readonly ADirectory I = new ADirectory(ADirectory.FromFullPath(@"C:\Users"), new string[] { Environment.UserName, "Public", Environment.UserName + @"\OneDrive" });
      public static readonly ADirectory Desktop = I.CDirectory("Desktop");
      public static class Documents {
        public static readonly ADirectory I = new ADirectory(User.I, new string[] { "المستندات", "Documents" });
        public static readonly ADirectory UpdateWindowsPowershellScripts = I.CDirectory("WindowsPowerShell");
      }
      /// <summary>
      /// Temporary directory path inside the user profile AppData.
      /// </summary>
      public static class Temp {
        public static ADirectory I = new ADirectory(User.I.CDirectory("AppData").CDirectory("Local"), "Temp");
      }
      public static class StartUp {
        public static readonly ADirectory Programs = new ADirectory(User.I.CDirectory("AppData").CDirectory("Roaming").CDirectory("Microsoft").CDirectory("Windows").CDirectory("Start Menu"), "Programs");
        public static readonly ADirectory I = new ADirectory(Programs, "Startup");
      }//end StartUp class
    }//end User class





  }
}
