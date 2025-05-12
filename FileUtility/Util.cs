using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUtility {
  public class Util {
    /// <summary>
    /// Retry an operation X times (0 for run once (no retry)) then if all of them failed it will throw the exception that has failed them.
    /// Useful to simplify retrying in any try catch block.
    /// You have to implement your own try/catch block but using RetryOperation will only retry if failed (No error logging exist).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="maxRetryCounter"></param>
    /// <param name="currentCounter"></param>
    /// <param name="lastException"></param>
    public static async Task<T> RetryOperation<T>(Func<Task<T>> action, int maxRetryCounter = 10, int delay = 50, int currentCounter = 0, Exception lastException = null) {
      if(maxRetryCounter < 0) {
        throw new ArgumentOutOfRangeException(nameof(maxRetryCounter), "maxRetryCounter must be greater or equal to 0");
      }
      if(delay <= 0) {
        throw new ArgumentOutOfRangeException(nameof(delay), "delay must be greater than 0");
      }
      if(currentCounter < 0) {
        throw new ArgumentOutOfRangeException(nameof(currentCounter), "currentCounter must be greater or equal to 0");
      }

      if(maxRetryCounter >= currentCounter++) {
        try {
          return await action();
        } catch(Exception e) {
          await Task.Delay(delay);
          return await RetryOperation(action, maxRetryCounter, delay, currentCounter, e);
        }
      }
      throw lastException;
    }
    /// <summary>
    /// Retry an operation X times then if all of them failed it will throw the exception that has failed them.
    /// Useful to simplify retrying in any try catch block.
    /// You have to implement your own try/catch block but using RetryOperation will only retry if failed (No error dialog).
    /// </summary>
    /// <param name="action"></param>
    /// <param name="maxRetryCounter"></param>
    /// <param name="currentCounter"></param>
    /// <param name="lastException"></param>
    public static async Task RetryOperation(Func<Task> action, int maxRetryCounter = 10, int delay = 50, int currentCounter = 0, Exception lastException = null) {

      if(maxRetryCounter < 0) {
        throw new ArgumentOutOfRangeException(nameof(maxRetryCounter), "maxRetryCounter must be greater or equal to 0");
      }
      if(delay <= 0) {
        throw new ArgumentOutOfRangeException(nameof(delay), "delay must be greater than 0");
      }
      if(currentCounter < 0) {
        throw new ArgumentOutOfRangeException(nameof(currentCounter), "currentCounter must be greater or equal to 0");
      }
      if(maxRetryCounter >= currentCounter++) {
        try {
          await action();
        } catch(Exception e) {
          await Task.Delay(delay);
          await RetryOperation(action, maxRetryCounter, delay, currentCounter, e);
        }
      } else
        throw lastException;
    }
  }
}
