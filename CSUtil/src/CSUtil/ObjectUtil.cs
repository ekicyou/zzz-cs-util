using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil
{
  /// <summary>
  /// オブジェクト操作ユーティリティ。
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ObjectUtil<T>
  {
    /// <summary>
    /// ダミーコンストラクタ。
    /// </summary>
    protected ObjectUtil()
    {
    }


    /// <summary>
    /// 値型の２値を入れ替えます。
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void Swap(ref T a, ref T b)
    {
      T c = a;
      a = b;
      b = c;
    }

  }
}
