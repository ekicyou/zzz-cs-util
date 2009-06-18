using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CSUtil;

namespace CSUtil.Linq.Test
{
  using NUnit.Framework;

  [TestFixture]
  public class LinqSpeedTest
  {
    private static int[] ShaffleData(int length)
    {
      int[] array = new int[length];
      for (int i = 0; i < array.Length; i++) array[i] = i;
      return ArrayUtil<int>.Shuffle(ref array);
    }

    private delegate IDictionary<int, int> DicFunc(int[] src);

    private static TimeSpan CheckSelectSpped(int length,DicFunc func)
    {
      Stopwatch sw = new Stopwatch();
      int loopCount = 100;
      Random rand = new Random();
      for (int i = 0; i < loopCount; i++) {
        IDictionary<int, int> dic = func(ShaffleData(length));
        int key = rand.Next(length);
        var selList = from p in dic where p.Key == key select p.Value;
        sw.Start();
        int value =selList.First();
        sw.Stop();
        GC.KeepAlive(value);
      }
      long tick = sw.ElapsedTicks / loopCount;
      double dtick = tick;
      TimeSpan time= TimeSpan.FromSeconds(dtick / Stopwatch.Frequency);
      Trace.WriteLine(string.Format("n= {0,5:####0}: tick={1,7:######0}", length, tick));
      return time;
    }


    [Test]
    public void SortedDicTest()
    {
      DicFunc func = delegate(int[] src) {
        IDictionary<int,int>dic= new SortedDictionary<int, int>();
        foreach (int k in src) dic[k] = k;
        return dic;
      };

      Trace.WriteLine("** [SortedDictionary] **");
      CheckSelectSpped(10, func);
      CheckSelectSpped(100, func);
      CheckSelectSpped(1000, func);
      CheckSelectSpped(10000, func);
    }

    [Test]
    public void DicTest()
    {
      DicFunc func = delegate(int[] src) {
        IDictionary<int, int> dic = new Dictionary<int, int>();
        foreach (int k in src) dic[k] = k;
        return dic;
      };

      Trace.WriteLine("** [Dictionary] **");
      CheckSelectSpped(10, func);
      CheckSelectSpped(100, func);
      CheckSelectSpped(1000, func);
      CheckSelectSpped(10000, func);
    }
  }
}
