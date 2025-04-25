# FileUtility Library for .NET Framework 4.8

A robust file and directory manipulation library with async/await support, automatic retries, and Recycle Bin integration.

## Features
- **Async/Await Support**: All operations are non-blocking
- **Retry Mechanism**: Retries for transient failures
- **Recycle Bin Integration**: Delete files/folders into the Recycle Bin
- **File Operations**: Create/ReadAllText/ReadAllLines/WriteAllText/WriteAllLines/Copy/Move/Delete files
- **Directory Operations**: Create/List/Delete directories
- **Path Aliasing**: Multiple path representations for a single file/directory
- **Deep Operations**: Automatic nested directory creation

## Usage

### Directory Operations
```csharp
// Create a directory object.
var parentDir = ADirectory.FromFullPath(@"C:\base");
// Concatenate the `parentDir` with a new directory with two possible names.
var childDir = parentDir.CDirectory("subfolder", new string[] {"alias1", "alias2"});
// Create the `childDir` on the disk.
await childDir.Create(); // Will use `alias1` name; it's the first name on its alias names.

// Array of files inside the `childDir`.
List<string> files = await childDir.GetFilesPaths(); // Absolute path
// Array of directories inside the `childDir`.
var directories = await childDir.GetDirectoriesPath();

// Delete to Recycle Bin.
await childDir.Delete(recycleBin: true); // Default is `false`
