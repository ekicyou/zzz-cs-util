using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace CSUtil.Threading
{
  /// <summary>
  /// 時計に同期して定期的な処理を行なうためのタイマーです。
  /// </summary>
  public class ClockSyncTimer
  {
    /// <summary>
    /// 時計に対する同期間隔。
    /// </summary>
    public TimeSpan Interval
    {
      get { return interval; }
      set { interval = value; }
    }
    private TimeSpan interval;

    /// <summary>
    /// 間隔が経過すると発生します。
    /// </summary>
    public event EventHandler Elapsed;

    /// <summary>
    /// イベントを発生させるためのスレッド。
    /// </summary>
    private Thread thread;

    /// <summary>
    /// 時計同期タイマーを作成します。
    /// バックグラウンドスレッドとして動作します。
    /// </summary>
    public ClockSyncTimer()
    {
      thread = new Thread(ThreadMain);
      thread.SetApartmentState(ApartmentState.STA);
      thread.IsBackground = true;
    }

    /// <summary>
    /// タイマーをスタートします。
    /// </summary>
    public void Start()
    {
      thread.Start();
    }

    private void ThreadMain()
    {
      long spanTick = interval.Ticks;
      while (true) {
        long waitTicks = spanTick - (DateTime.Now.Ticks % spanTick);
        Thread.Sleep(new TimeSpan(waitTicks));
        if (Elapsed == null) continue;
        EventArgs args = new EventArgs();
        Elapsed(this, args);
      }
    }

  }
}
