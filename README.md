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

## Usage Sample

```c#
// Create a directory object.
ADirectory parentDir = ADirectory.FromFullPath(@"C:\path\to\a\directory");
// Concatenate the `parentDir` with a new directory with two possible names.
ADirectory childDir = parentDir.CDirectory("subfolder", new string[] {"alias1", "alias2"});
// Create the `childDir` on the disk.
await childDir.Create(); // Will use `alias1` name; it's the first name on its alias names.

// Create a file inside `childDir` directory
AFile file = childDir.CFile("filename.txt");
// Write text into a file
await file.WriteAllText("hello");
// Copy file to another directory (e.g., `parentDir`).
await file.CopyTo(new AFile(parentDir, "filename.txt"));
// Check if a file exists on the disk.
bool isFileExists = await file.Exists(); // Checks name alias if first name doesn't exist
// Read text from a file
string content = await file.ReadAllText();
// Or
List<string> lines = await file.ReadAllLines();

// Delete to Recycle Bin.
await childDir.Delete(recycleBin: true); // Default is `false`

// Array of files inside the `childDir`.
List<string> files = await childDir.GetFilesPaths(); // Absolute path
// Array of directories inside the `childDir`.
List<string> directories = await childDir.GetDirectoriesPath();

