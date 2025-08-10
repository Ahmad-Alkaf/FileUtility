using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFileUtility {
  [TestClass]
  public class TestAFile : TestHelper {
    [TestMethod]
    public void FromFullPath() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        var aFile = new AFile(path + "\\fromFullPath.txt");
        Assert.AreEqual(path + "\\fromFullPath.txt", aFile.Path().GetAwaiter().GetResult());

        AFile file1 = AFile.FromFullPath("%USERPROFILE%\\Desktop\\test.txt");
        AFile file2 = AFile.FromFullPath("C:\\Users\\Ahmed\\Desktop\\test.txt");
        AFile file3 = AFile.FromFullPath("C:\\Users\\Public\\Desktop\\test.txt");
        AFile file4 = AFile.FromFullPath("C:\\Users\\Abdoo\\Desktop\\test.txt");
        Assert.AreEqual(file1, file2);
        Assert.AreEqual(file2, file3);
        Assert.AreEqual(file1, file4);
        Assert.IsTrue(new List<AFile> { file1, file2, file3, file4 }.Contains(file1));
        Assert.IsTrue(new List<AFile> { file1, file2, file3, file4 }.Contains(file2));
        Assert.IsTrue(new List<AFile> { file1, file2, file3, file4 }.Contains(file3));
        Assert.IsTrue(new List<AFile> { file1, file2, file3 }.Contains(file4));


      }
    }

    [TestMethod]
    public async Task Constructor() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        AFile aFile = AFile.FromFullPath(path + "\\1.txt");
        Assert.AreEqual(path + "\\1.txt", await aFile.Path());

        ADirectory parent = ADirectory.FromFullPath(path);
        Assert.AreEqual(aFile, new AFile(parent, "1.txt"));
        Assert.AreEqual(aFile, parent.CFile("1.txt"));
        Assert.AreEqual(aFile, new AFile(path + "\\1.txt"));
        Assert.AreEqual(aFile, new AFile(parent, new string[] { "2.txt", "1.txt" }));
        Assert.AreNotEqual(aFile, new AFile(parent, new string[] { "2.txt", "3.txt" }));
        Assert.AreEqual(aFile, parent.CFile("1.txt"));
        Assert.AreEqual(aFile, new AFile(path + "\\1.txt"));
        Assert.AreEqual(await aFile.Path(), await new AFile(parent, "1.txt").Path());
        Assert.AreEqual(await aFile.Path(), await parent.CFile("1.txt").Path());
        Assert.AreEqual(await aFile.Path(), await new AFile(path + "\\1.txt").Path());
        Assert.AreEqual(await aFile.Path(), aFile.PathSync());

      }
    }

    [TestMethod]
    public async Task CopyTo() {
      using(var act = new Act()) {
        var parent = ADirectory.FromFullPath(act.CreateEmptyDirectory());
        var fileX = parent.CFile("copy.txt");
        var fileY = parent.CDirectory("dest").CFile("copy.txt");
        Assert.IsFalse(await fileX.Exists());
        Assert.IsFalse(fileX.ExistsSync());
        await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => await fileX.CopyTo(fileY, overWrite: true));
        await fileX.WriteAllText("hi");
        Assert.IsTrue(await fileX.Exists());
        Assert.AreEqual("hi", await fileX.ReadAllText());
        Assert.AreEqual("hi", fileX.ReadAllTextSync());
        Assert.IsFalse(await fileY.Exists());
        await fileX.CopyTo(fileY);
        Assert.IsTrue(await fileY.Exists());
        Assert.IsTrue(await fileX.Exists());
        Assert.IsTrue(fileY.ExistsSync());
        Assert.IsTrue(fileX.ExistsSync());
        Assert.AreEqual("hi", await fileY.ReadAllText());
        await fileX.WriteAllText("overwrite");
        await Assert.ThrowsExceptionAsync<FileLoadException>(async () => await fileX.CopyTo(fileY, overWrite: false));
        Assert.AreEqual("hi", await fileY.ReadAllText());
        await fileX.CopyTo(fileY, overWrite: true);
        Assert.AreEqual("overwrite", await fileY.ReadAllText());
      }
    }

    [TestMethod]
    public async Task MoveTo() {
      using(var act = new Act()) {
        var parent = ADirectory.FromFullPath(act.CreateEmptyDirectory());
        var fileX = parent.CFile("move.txt");
        var fileY = parent.CDirectory("dest").CFile("move.txt");
        Assert.IsFalse(await fileX.MoveTo(fileY));
        Assert.IsFalse(await fileX.Exists());
        await fileX.WriteAllText("hi"); // Create the file
        Assert.IsTrue(await fileX.Exists());
        Assert.AreEqual("hi", await fileX.ReadAllText());
        Assert.IsFalse(await fileY.Exists());
        Assert.IsTrue(await fileX.MoveTo(fileY));
        Assert.IsFalse(await fileX.Exists());
        Assert.IsTrue(await fileY.Exists());
        Assert.AreEqual("hi", await fileY.ReadAllText());
        await fileX.WriteAllText("overwrite");
        Assert.IsFalse(await fileX.MoveTo(fileY, overWrite: false));
        Assert.AreEqual("hi", await fileY.ReadAllText());
        Assert.IsTrue(await fileX.MoveTo(fileY, overWrite: true));
        Assert.AreEqual("overwrite", await fileY.ReadAllText());
      }
    }
    [TestMethod]
    public async Task ReadWrite() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        ADirectory parent = ADirectory.FromFullPath(path).CDirectory("nested").CDirectory("very").CDirectory("deep").CDirectory("very-deep-nested");
        AFile file = parent.CFile("readWrite.txt");
        Assert.IsFalse(parent.ExistsSync());
        Assert.IsFalse(await parent.Exists());
        Assert.AreEqual(null, file.ReadAllTextSync());
        Assert.AreEqual(null, await file.ReadAllText());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());
        Assert.AreEqual("readWrite.txt", file.Name);
        Assert.AreEqual("readWrite.txt", file.ExactNameSync());
        Assert.AreEqual("readWrite.txt", await file.ExactName());

        await file.WriteAllText(null);
        Assert.IsTrue(parent.ExistsSync());
        Assert.IsTrue(await parent.Exists());
        Assert.IsTrue(file.ExistsSync());
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("", file.ReadAllTextSync());
        Assert.AreEqual("", await file.ReadAllText());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());

        await file.WriteAllText("");
        Assert.IsTrue(parent.ExistsSync());
        Assert.IsTrue(await parent.Exists());
        Assert.IsTrue(file.ExistsSync());
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("", file.ReadAllTextSync());
        Assert.AreEqual("", await file.ReadAllText());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());

        await file.WriteAllText("written-text");
        Assert.IsTrue(parent.ExistsSync());
        Assert.IsTrue(await parent.Exists());
        Assert.IsTrue(file.ExistsSync());
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("written-text", file.ReadAllTextSync());
        Assert.AreEqual("written-text", await file.ReadAllText());
        Assert.AreEqual(1, (await file.ReadAllLines()).Count());
        Assert.AreEqual("written-text", (await file.ReadAllLines())[0]);

        await file.WriteAllText("overwrite-text\r\n");
        Assert.IsTrue(parent.ExistsSync());
        Assert.IsTrue(await parent.Exists());
        Assert.IsTrue(file.ExistsSync());
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("overwrite-text\r\n", file.ReadAllTextSync());
        Assert.AreEqual("overwrite-text\r\n", await file.ReadAllText());
        Assert.AreEqual(1, (await file.ReadAllLines()).Count());
        Assert.AreEqual("overwrite-text", (await file.ReadAllLines())[0]);
        await file.WriteAllText("overwrite-text\r\n  ");
        Assert.AreEqual(2, (await file.ReadAllLines()).Count());
        Assert.AreEqual("overwrite-text", (await file.ReadAllLines())[0]);
        Assert.AreEqual("  ", (await file.ReadAllLines())[1]);
        await file.WriteAllText("overwrite-text\r\n  hi  ");
        Assert.AreEqual(2, (await file.ReadAllLines()).Count());
        Assert.AreEqual("overwrite-text", (await file.ReadAllLines())[0]);
        Assert.AreEqual("  hi  ", (await file.ReadAllLines())[1]);
        await file.WriteAllText("overwrite-text\r\n  hi  \r\n\r\n\r\n");
        Assert.AreEqual(2, (await file.ReadAllLines()).Count());
        Assert.AreEqual("overwrite-text", (await file.ReadAllLines())[0]);
        Assert.AreEqual("  hi  ", (await file.ReadAllLines())[1]);
        string deletedPath = await parent.Parent.Parent.Path();
        await parent.Parent.Parent.Delete(true);
        Assert.IsFalse(parent.ExistsSync());
        Assert.IsFalse(await parent.Exists());
        Assert.AreEqual(null, await file.ReadAllText());
        Assert.AreEqual(null, file.ReadAllTextSync());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());

        await file.WriteAllLines(null);
        Assert.IsTrue(parent.ExistsSync());
        Assert.IsTrue(await parent.Exists());
        Assert.IsTrue(file.ExistsSync());
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("", file.ReadAllTextSync());
        Assert.AreEqual("", await file.ReadAllText());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());

        await parent.Parent.Parent.Delete();
        Assert.IsFalse(await file.Exists());
        Assert.IsNull(await file.ReadAllText());
        Assert.IsNull(file.ReadAllTextSync());

        await file.WriteAllLines(new string[] { });
        Assert.IsTrue(await parent.Exists());
        Assert.AreEqual("", file.ReadAllTextSync());
        Assert.AreEqual("", await file.ReadAllText());
        Assert.AreEqual(0, (await file.ReadAllLines()).Count());
        Assert.AreEqual(0, (await file.ReadAllBytes()).Length);

        await file.Delete();
        Assert.IsNull(await file.ReadAllBytes());
        await file.WriteAllText("hi");
        Assert.AreEqual(2, (await file.ReadAllBytes()).Length);
      }
    }
    [TestMethod]
    public async Task Delete() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        ADirectory parent = ADirectory.FromFullPath(path).CDirectory("deleteMe");
        AFile file = new AFile(parent, "deleteMe.txt");
        Assert.IsFalse(await file.Exists());
        Assert.IsFalse(file.ExistsSync());
        await file.WriteAllText("");
        Assert.IsTrue(await file.Exists());
        Assert.IsTrue(file.ExistsSync());
        await file.Delete(false);
        Assert.IsFalse(await file.Exists());
        Assert.IsFalse(file.ExistsSync());
        await file.WriteAllText("");
        Assert.IsTrue(await file.Exists());
        Assert.IsTrue(file.ExistsSync());
        file.DeleteSync();
        Assert.IsFalse(await file.Exists());
        Assert.IsFalse(file.ExistsSync());
        await file.WriteAllText("");
        Assert.IsTrue(await file.Exists());
        await file.Delete(true);
        Assert.IsFalse(await file.Exists());
      }
    }

    [TestMethod]
    public async Task ExistedPathAlias() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();

        var parent = ADirectory.FromFullPath(path).CDirectory("existedAlias");
        AFile file = parent.CFile("existedAlias.txt");
        Assert.AreEqual(0, (await file.ExistedPathAlias()).Count);
        await file.WriteAllText("");
        Assert.IsTrue(await file.Exists());
        file = parent.CFile(new string[] { "existedAlias.txt", "existedaliasalias.txt" });
        Assert.AreEqual("existedAlias.txt", file.Alias[0]);
        Assert.AreEqual("existedaliasalias.txt", file.Alias[1]);
        await file.WriteAllText("");
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("existedAlias.txt", file.Name);
        Assert.AreEqual("existedAlias.txt", file.Alias[0]);
        Assert.AreEqual("existedaliasalias.txt", file.Alias[1]);
        Assert.AreEqual("existedAlias.txt", file.ExactNameSync());
        Assert.AreEqual("existedAlias.txt", await file.ExactName());
        await file.Delete();
        Assert.IsFalse(await file.Exists());
        await parent.CFile("existedaliasalias.txt").WriteAllText("");
        Assert.IsTrue(await file.Exists());
        Assert.AreEqual("existedAlias.txt", file.Name);
        Assert.AreEqual("existedAlias.txt", file.Alias[0]);
        Assert.AreEqual("existedaliasalias.txt", file.Alias[1]);
        Assert.AreEqual("existedaliasalias.txt", file.ExactNameSync());
        Assert.AreEqual("existedaliasalias.txt", await file.ExactName());

        var existedAlias = (await file.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 1);
        Assert.AreEqual(existedAlias[0], file);
        await file.WriteAllText("");
        existedAlias = (await file.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 1);

        await parent.CFile("existedAlias.txt").WriteAllText("");
        existedAlias = (await file.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 2);
        Assert.AreEqual(existedAlias[0], file);
        Assert.AreEqual(existedAlias[1], file);
        await file.WriteAllText("");
        existedAlias = await file.ExistedPathAlias();
        Assert.IsTrue(existedAlias.Count == 2);

      }
    }
    [TestMethod]
    public async Task UpdateConcurrent() {
      using(var act = new Act()) {
        ADirectory dir = ADirectory.FromFullPath(act.CreateEmptyDirectory("update-concurrent"));
        Func<string, string> updateFunc = (n) => {
          if(string.IsNullOrEmpty(n)) {
            return "1";
          } else if(int.TryParse(n, out int intNum))
            return (++intNum).ToString();
          else Assert.Fail("concurrent.txt file contains unexpected content: " + n);
          return "";
        };
        var file = dir.CFile("concurrent.txt");
        var tasks = new List<Task>();
        for(int i = 0;i < 1000;i++) {
          tasks.Add(Task.Run(async () => {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            await file.ConcurrentUpdate(updateFunc);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Concurrent operation took " + elapsedMs + "ms");
          }));
        }
        await Task.WhenAll(tasks);
        Assert.AreEqual(await file.ReadAllText(), "1000");
      }
    }

  }
}
