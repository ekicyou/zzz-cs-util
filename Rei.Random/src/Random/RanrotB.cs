/*
 * Copyright (C) Rei HOBARA 2007
 * 
 * Name:
 *     Well.cs
 * Class:
 *     Rei.Random.RanrotB
 * Purpose:
 *     A random number generator using RanrotB.
 * Remark:
 *     This code is C# implementation of RanrotB.
 *     Well was introduced by Agner Fog.
 *     See http://www.agner.org/random/theory/chaosran.pdf for detail of RanrotB.
 * History:
 *     2007/10/6 initial release.
 * 
 */

using System;

namespace Rei.Random {

    /// <summary>
    /// Ranrotの擬似乱数ジェネレータークラス。
    /// </summary>
    public class RanrotB : RandomBase {

        #region Field

        /// <summary>
        /// 内部状態ベクトルの個数。
        /// </summary>
        protected const int KK = 17;
        /// <summary>
        /// RanrotBのパラメーターの一つ。
        /// </summary>
        protected const int JJ = 10;
        /// <summary>
        /// RanrotBのパラメーターの一つ。
        /// </summary>
        protected const int R1 = 13;
        /// <summary>
        /// RanrotBのパラメーターの一つ。
        /// </summary>
        protected const int R2 = 9;
        /// <summary>
        /// 内部状態ベクトル。
        /// </summary>
        protected UInt32[] randbuffer;
        /// <summary>
        /// リングバッファのインデックス。
        /// </summary>
        protected int p1, p2;

        #endregion

        /// <summary>
        /// 現在時刻を種とした、Well擬似乱数ジェネレーターを初期化します。
        /// </summary>
        public RanrotB() : this(Environment.TickCount) { }

        /// <summary>
        /// seedを種とした、Well擬似乱数ジェネレーターを初期化します。
        /// </summary>
        public RanrotB( int seed ) {
            UInt32 s = (UInt32)seed;
            randbuffer = new UInt32[KK];
            for (int i = 0; i < KK; i++)
                randbuffer[i] = s = s * 2891336453 + 1;
            p1 = 0; p2 = JJ;
            for (int i = 0; i < 9; i++) NextUInt32();
        }

        /// <summary>
        /// 符号なし32bitの擬似乱数を取得します。
        /// </summary>
        public override uint NextUInt32() {
            UInt32 x;
            x = randbuffer[p1] = ((randbuffer[p2] << R1) | (randbuffer[p2] >> (32 - R1))) + ((randbuffer[p1] << R2) | (randbuffer[p1] >> (32 - R2)));
            if (--p1 < 0) p1 = KK - 1;
            if (--p2 < 0) p2 = KK - 1;
            return x;
        }

    }

}
