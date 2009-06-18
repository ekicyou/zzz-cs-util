using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil
{
  /// <summary>
  /// 値型に関するユーティリティ。
  /// </summary>
  public class ValueUtil<T>:ObjectUtil<T> where T : struct
  {
    /// <summary>
    /// ダミーコンストラクタ。
    /// </summary>
    protected ValueUtil()
      : base()
    {
    }
  }



  /// <summary>
  /// 値型に関するユーティリティ。
  /// 今後は ValueUtil(of T)を利用してください
  /// </summary>
  [Obsolete]
  public static class ValueUtil
  {
    /// <summary>
    /// ２値を入れ替えます。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void Swap<T>(ref T a,ref T b)
    {
      ObjectUtil<T>.Swap(ref a, ref b);
    }
  }


}
