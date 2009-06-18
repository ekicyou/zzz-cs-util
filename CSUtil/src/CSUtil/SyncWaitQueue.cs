using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CSUtil;

namespace CSUtil.Collections.Generic
{

  /// <summary>
  /// キューが存在しない場合にスレッドを停止して待機するキューです。
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SyncWaitQueue<T> : DisposableObject
  {
    private Queue<T> queue = new Queue<T>();
    private const int ReadTimeout = 3000;
    private object lockObject = new object();

    /// <summary>
    /// SyncWaitQueue に格納されている要素の数を取得します。
    /// </summary>
    public int Count
    {
      get
      {
        lock (lockObject) {
          return queue.Count;
        }
      }
    }

    /// <summary>
    /// Dispose処理を実装します。以下のモードに切り替わります。
    /// ・Enqueueを無視
    /// ・DequeueでキューがなくなったときにObjectDisposedException例外
    /// ・Dequeueのロックを解除する
    /// </summary>
    protected override void DisposeManage()
    {
      lock (lockObject) {
        disposed = true;
        Monitor.PulseAll(lockObject);
      }
    }
    private volatile bool disposed = false;

    /// <summary>
    /// SyncWaitQueue の先頭にあるオブジェクトを削除し、返します。
    /// データが無い場合、データが追加されるまでロックします。
    /// オブジェクトが破棄され、かつキューが０になった場合、
    /// ObjectDisposedException例外をスローします。
    /// </summary>
    /// <returns></returns>
    public T Dequeue()
    {
      WaitEnqueue();
      lock (lockObject) {
        try {
          return queue.Dequeue();
        }
        catch (InvalidOperationException) {
          if (disposed) throw new ObjectDisposedException("SyncWaitQueueは停止状態です。");
          throw;
        }
      }
    }

    /// <summary>
    /// SyncWaitQueue に値が設定されるまで待機します。
    /// キューにストックが無く、オブジェクトが破棄された場合、
    /// ObjectDisposedException例外をスローします。
    /// </summary>
    private void WaitEnqueue()
    {
      while (true) {
        bool isTimeout;
        lock (lockObject) {
          if (queue.Count > 0) return;
          if (disposed) throw new ObjectDisposedException("SyncWaitQueueは停止状態です。");
          isTimeout = Monitor.Wait(lockObject, ReadTimeout);
        }
        if (isTimeout) Thread.Sleep(100);
      }
    }

    /// <summary>
    /// SyncWaitQueue の末尾にオブジェクトを追加します。。
    /// 取り出し処理がロックされている場合、解除します。
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
      if (disposed) return;
      lock (lockObject) {
        queue.Enqueue(item);
        Monitor.PulseAll(lockObject);
      }
    }

  }

}
