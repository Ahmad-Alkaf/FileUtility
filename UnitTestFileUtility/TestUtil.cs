using FileUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestFileUtility {
  [TestClass]
  public class TestUtil {
    private volatile int counter = 0;
    [TestMethod]
    public async Task retryOperation() {
      await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => Util.RetryOperation(() => Task.CompletedTask, -1));
      await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => Util.RetryOperation(() => Task.FromResult(true), -1));

      counter = 0;
      Assert.IsTrue(await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 0) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
        return true;
      }), 0)
    );
      Assert.AreEqual(0, counter);

      counter = 0;
      await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 0) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
      }), 0);
      Assert.AreEqual(0, counter);

      counter = 0;
      Assert.IsTrue(await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 1) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
        return true;
      }), 1)
    );
      Assert.AreEqual(1, counter);

      counter = 0;
      await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 1) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
      }), 1);
      Assert.AreEqual(1, counter);

      counter = 0;
      Assert.IsTrue(await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 10) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
        return true;
      }), 10)
    );
      Assert.AreEqual(10, counter);

      counter = 0;
      await Util.RetryOperation(() => Task.Run(() => {
        if(counter != 10) {
          counter++;
          throw new Exception("TEST RETRY_OPERATION");
        }
      }), 10);
      Assert.AreEqual(10, counter);

      counter = 0;
      Assert.ThrowsException<CustomAttributeFormatException>(() => Util.RetryOperation(() => Task.Run(() => {
        if(counter != 10) {
          counter++;
          throw new CustomAttributeFormatException("TEST RETRY_OPERATION");
        }
        return true;
      }), 5).GetAwaiter().GetResult()
    );
      Assert.AreEqual(6, counter); // Run once and retried 5 times

      counter = 0;
      Assert.ThrowsException<CustomAttributeFormatException>(() => Util.RetryOperation(() => Task.Run(() => {
        if(counter != 10) {
          counter++;
          throw new CustomAttributeFormatException("TEST RETRY_OPERATION");
        }
      }), 5).GetAwaiter().GetResult()
    );
      Assert.AreEqual(6, counter); // Run once and retried 5 times
    }
  }
}
