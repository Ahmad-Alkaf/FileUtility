using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFileUtility {
  [TestClass]
  public class TestADirectory : TestHelper {
    [TestMethod]
    public async Task FromFullPath() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        ADirectory aDirectory = ADirectory.FromFullPath(path);
        Assert.AreEqual(path, await aDirectory.Path());
      }
    }

    [TestMethod]
    public async Task Constructor1() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();

        ADirectory parent = ADirectory.FromFullPath(path);
        var aDirectory = parent.CDirectory("c1");
        Assert.AreEqual(path + "\\c1", await aDirectory.Path());
        Assert.AreEqual(path + "\\c1", await new ADirectory(parent, "c1").Path());
        Assert.AreEqual(path + "\\c1", await new ADirectory(parent, new string[] { "c1" }).Path());
        Assert.AreEqual(path + "\\c1", await new ADirectory(parent, new string[] { "c1", "c2" }).Path());
        Assert.AreNotEqual(path + "\\c1", await new ADirectory(parent, new string[] { "c2", "c3" }).Path());
        Assert.AreEqual(aDirectory, new ADirectory(parent, "c1"));
        Assert.AreEqual(aDirectory, new ADirectory(parent, new string[] { "c1" }));
        Assert.AreEqual(aDirectory, new ADirectory(parent, new string[] { "c1", "c2" }));
        Assert.AreEqual(aDirectory, new ADirectory(parent, new string[] { "c2", "c1" }));
        Assert.AreNotEqual(aDirectory, new ADirectory(parent, new string[] { "c2", "c3" }));

      }
    }
    [TestMethod]
    public async Task Constructor2() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        ADirectory aDirectory = ADirectory.FromFullPath(path).CDirectory(new string[] { "c2", "c2.1" });
        Assert.AreEqual(path + "\\c2", await aDirectory.Path());
        Assert.AreEqual(2, aDirectory.Alias.Count());
      }
    }

    [TestMethod]
    public async Task GetFilesPaths() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();
        ADirectory aDirectory = ADirectory.FromFullPath(path);
        Assert.AreEqual(0, (await aDirectory.GetFilesPaths()).Count);
      }
    }

    [TestMethod]
    public async Task GetDirectoriesPath() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();

        ADirectory aDirectory = ADirectory.FromFullPath(path);
        Assert.AreEqual(0, (await aDirectory.GetDirectoriesPath()).Count);
        await aDirectory.CDirectory("c1").Create();
        Assert.AreEqual(1, (await aDirectory.GetDirectoriesPath()).Count);
        await aDirectory.CDirectory("c2").Create();
        Assert.AreEqual(2, (await aDirectory.GetDirectoriesPath()).Count);
        var nested = aDirectory.CDirectory("nested").CDirectory("nested2");
        await nested.Create();
        Assert.IsTrue(Directory.Exists(await nested.Path()));

        var alias = aDirectory.CDirectory(new string[] { "nestedAlias", "nestedAlias2" });
        var deepNested = alias.CDirectory("deepNested").CDirectory("deepNested").CDirectory("deeepNested");
        Assert.IsFalse(Directory.Exists((await deepNested.Path())));
        await deepNested.Create();
        Assert.IsTrue(Directory.Exists((await deepNested.Path())));

        await alias.Delete();
        Assert.IsFalse(await alias.Exists());

        var aliasDeepNested = aDirectory.CDirectory("nestedAlias2").CDirectory("deepAliasNested").CDirectory("deepAliasNested").CDirectory("deepAliasNested");
        await aliasDeepNested.Create();
        Assert.IsTrue(await alias.Exists());
        Assert.IsTrue(await aliasDeepNested.Exists());
      }
    }
    [TestMethod]
    public async Task Delete() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();

        ADirectory aDirectory = ADirectory.FromFullPath(path).CDirectory("deleteMe");
        Assert.IsFalse(await aDirectory.Exists());
        await aDirectory.Create();
        Assert.IsTrue(await aDirectory.Exists());
        await aDirectory.Delete(false);
        Assert.IsFalse(await aDirectory.Exists());
        await aDirectory.Create();
        Assert.IsTrue(await aDirectory.Exists());
        string deletedPath = await aDirectory.Path();
        await aDirectory.Delete(true);
        Assert.IsFalse(await aDirectory.Exists());

      }
    }
    [TestMethod]
    public async Task ExistedPathAlias() {
      using(var act = new Act()) {
        string path = act.CreateEmptyDirectory();

        var aDir = ADirectory.FromFullPath(path).CDirectory("existedAlias");
        Assert.AreEqual(0, (await aDir.ExistedPathAlias()).Count);
        await aDir.Create();
        Assert.IsTrue(await aDir.Exists());
        aDir = ADirectory.FromFullPath(path).CDirectory(new string[] { "existedAlias", "existedaliasalias" });
        await aDir.Create();
        Assert.IsTrue(await aDir.Exists());
        await aDir.Delete();
        Assert.IsFalse(await aDir.Exists());
        await ADirectory.FromFullPath(path).CDirectory("existedaliasalias").Create();
        Assert.IsTrue(await aDir.Exists());
        var existedAlias = (await aDir.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 1);
        Assert.AreEqual(existedAlias[0], aDir);
        await aDir.Create();
        existedAlias = (await aDir.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 1);

        await ADirectory.FromFullPath(path).CDirectory("existedAlias").Create();
        existedAlias = (await aDir.ExistedPathAlias());
        Assert.IsTrue(existedAlias.Count == 2);
        Assert.AreEqual(existedAlias[0], aDir);
        Assert.AreEqual(existedAlias[1], aDir);
        await aDir.Create();
        existedAlias = await aDir.ExistedPathAlias();
        Assert.IsTrue(existedAlias.Count == 2);
      }
    }
  }
}
