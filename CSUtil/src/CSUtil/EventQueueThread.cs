using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using CSUtil.Collections.Generic;
using CSUtil;

namespace CSUtil.Threading
{
  /// <summary>
  /// イベントをキューにためて別スレッドで実行するためのクラスです。
  /// </summary>
  /// <typeparam name="T">EventArgsの派生クラス</typeparam>
  public abstract class EventQueueThread<T> : DisposableObject
  {
    /// <summary>
    /// スレッドのスタートアップコードを設定するメソッドです。
    /// オーバーライドしてください。
    /// </summary>
    protected abstract void StartUp();

    /// <summary>
    /// キューから値を受け取ったときの処理を記載するメソッドです。
    /// オーバーライドしてください。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    protected abstract void Dequeue(object sender, T ev);


    /// <summary>
    /// イベントを保管している同期キューを取得します。
    /// </summary>
    protected readonly SyncWaitQueue<KeyValuePair<object, T>> Queue = new SyncWaitQueue<KeyValuePair<object, T>>();

    /// <summary>
    /// スレッドを取得します。
    /// </summary>
    public readonly Thread Thread;

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    public EventQueueThread()
    {
      Thread = new Thread(ThreadMain);
      Thread.Name = this.GetType().Name;
      Thread.SetApartmentState(ApartmentState.STA);
      Thread.IsBackground = true;
    }

    /// <summary>
    /// マネージオブジェクトのDispose処理を行ないます。
    /// スレッドの停止要求を発行します。
    /// </summary>
    protected override void DisposeManage()
    {
      Debug.WriteLine(GetType().Name + ":Dispose開始");
      try {
        Queue.Dispose();
        base.DisposeManage();
      }
      finally {
        Debug.WriteLine(GetType().Name + ":Dispose終了");
      }
    }

    /// <summary>
    /// スレッドの停止を待ちます。
    /// </summary>
    public void Join()
    {
      Debug.WriteLine(GetType().Name + ":Join開始");
      try {
        Thread.Join();
      }
      finally {
        Debug.WriteLine(GetType().Name + ":Join終了");
      }
    }


    /// <summary>
    /// スレッドを開始します。
    /// </summary>
    public void Start()
    {
      Thread.Start();
    }

    /// <summary>
    /// キューの最後にイベントを登録します。
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ev"></param>
    public void Enqueue(object sender, T ev)
    {
      Queue.Enqueue(new KeyValuePair<object, T>(sender, ev));
    }


    /// <summary>
    /// スレッド本体です。
    /// キューから値を取り出し、イベントハンドラに渡します。
    /// </summary>
    protected virtual void ThreadMain()
    {
      Debug.WriteLine(GetType().Name + ":スレッド開始");
      try {
        StartUp();
        while (Thread.IsAlive) {
          // キューから要素をひとつ取り出して処理する
          KeyValuePair<object, T> item;
          try {
            item = Queue.Dequeue();
          }
          catch (ObjectDisposedException) {
            // キューが停止したため終了
            return;
          }
          Dequeue(item.Key, item.Value);
        }
      }
      finally {
        Debug.WriteLine(GetType().Name + ":スレッド終了");
      }
    }
  }
}
