using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFileUtility
{
  public static class Extension {
    public static void IsOne(this Assert assert, int number) {
      Assert.AreEqual(1, number);
    }
    public static void IsZero(this Assert assert, int number) {
      Assert.AreEqual(0, number);
    }
  }
}
