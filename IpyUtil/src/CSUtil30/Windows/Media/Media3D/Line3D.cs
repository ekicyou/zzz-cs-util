using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Media.Media3D;
using System.Runtime.Serialization;

namespace CSUtil.Windows.Media.Media3D
{
  /// <summary>
  /// 始点→終点で現す線分。
  /// </summary>
  [Serializable]
  public struct Line3D
  {
    private Point3D ps;
    private Point3D pe;

    /// <summary>
    /// 開始点。
    /// </summary>
    public Point3D PS { get { return ps; } set { ps = value; } }

    /// <summary>
    /// 終了点。
    /// </summary>
    public Point3D PE { get { return pe; } set { pe = value; } }

    /// <summary>
    /// コンストラクタ。始点→終点で示します。
    /// </summary>
    /// <param name="ps"></param>
    /// <param name="pe"></param>
    public Line3D(Point3D ps, Point3D pe)
    {
      this.ps = ps;
      this.pe = pe;
    }

    /// <summary>
    /// コンストラクタ。始点＋ベクトルで示します。
    /// </summary>
    /// <param name="ps"></param>
    /// <param name="vec"></param>
    public Line3D(Point3D ps, Vector3D vec)
    {
      this.ps = ps;
      this.pe = ps + vec;
    }

    /// <summary>
    /// コンストラクタ。始点各座標→終点各座標で示します。
    /// </summary>
    /// <param name="psX"></param>
    /// <param name="psY"></param>
    /// <param name="psZ"></param>
    /// <param name="peX"></param>
    /// <param name="peY"></param>
    /// <param name="peZ"></param>
    public Line3D(double psX, double psY, double psZ, double peX, double peY, double peZ)
    {
      this.ps = new Point3D(psX, psY, psZ);
      this.pe = new Point3D(peX, peY, peZ);
    }


    #region オペレータ演算、その他比較
    public static Line3D operator +(Line3D line, Vector3D vec) { return new Line3D(line.PS + vec, line.PE + vec); }
    public static Line3D operator -(Line3D line, Vector3D sub) { return new Line3D(line.PS - sub, line.PE - sub); }
    public static Line3D operator *(Line3D line, Matrix3D mat) { return new Line3D(line.PS * mat, line.PE * mat); }

    #endregion

  }
}
