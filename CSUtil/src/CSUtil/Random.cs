/* C# Version Copyright (C) 2001-2004 Akihilo Kramot (Takel).  */
/* C# porting from a C-program for MT19937, originaly coded by */
/* Takuji Nishimura, considering the suggestions by            */
/* Topher Cooper and Marc Rieffel in July-Aug. 1997.           */
/* This library is free software under the Artistic license:   */
/*                                                             */
/* You can find the original C-program at                      */
/*     http://www.math.keio.ac.jp/~matumoto/mt.html            */
/*                                                             */

using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil
{
    /// <summary>
    /// メルセンヌツイスタ乱数生成器クラス。
    /// </summary>
    public class Random : System.Random
    {
        /* Period parameters */
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0df; /* constant vector a */
        private const uint UPPER_MASK = 0x80000000; /* most significant w-r bits */
        private const uint LOWER_MASK = 0x7fffffff; /* least significant r bits */

        /* Tempering parameters */
        private const uint TEMPERING_MASK_B = 0x9d2c5680;
        private const uint TEMPERING_MASK_C = 0xefc60000;

        private static uint TEMPERING_SHIFT_U(uint y) { return (y >> 11); }
        private static uint TEMPERING_SHIFT_S(uint y) { return (y << 7); }
        private static uint TEMPERING_SHIFT_T(uint y) { return (y << 15); }
        private static uint TEMPERING_SHIFT_L(uint y) { return (y >> 18); }

        private uint[] mt = new uint[N]; /* the array for the state vector  */

        private short mti;

        private static uint[] mag01 = { 0x0, MATRIX_A };

        /* initializing the array with a NONZERO seed */

        /// <summary>
        /// コンストラクタ。
        /// 指定したシードで乱数生成器を作成します。
        /// </summary>
        /// <param name="seed"></param>
        public Random(uint seed)
        {
            /* setting initial seeds to mt[N] using         */
            /* the generator Line 25 of Table 1 in          */
            /* [KNUTH 1981, The Art of Computer Programming */
            /*    Vol. 2 (2nd Ed.), pp102]                  */
            mt[0] = seed & 0xffffffffU;
            for (mti = 1; mti < N; ++mti) {
                mt[mti] = (69069 * mt[mti - 1]) & 0xffffffffU;
            }
        }

        /// <summary>
        /// コンストラクタ。
        /// デフォルトのシードで乱数生成器を作成します。
        /// </summary>
        public Random()
            : this(4357) /* a default initial seed is used   */
        {
        }

        /// <summary>
        /// ランダムなuint値を生成します。乱数発生ルーチンの基礎部分です。
        /// </summary>
        /// <returns></returns>
        protected uint GenerateUInt()
        {
            uint y;

            /* mag01[x] = x * MATRIX_A  for x=0,1 */
            if (mti >= N) /* generate N words at one time */ {
                short kk = 0;

                for (; kk < N - M; ++kk) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                for (; kk < N - 1; ++kk) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }

                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= TEMPERING_SHIFT_U(y);
            y ^= TEMPERING_SHIFT_S(y) & TEMPERING_MASK_B;
            y ^= TEMPERING_SHIFT_T(y) & TEMPERING_MASK_C;
            y ^= TEMPERING_SHIFT_L(y);

            return y;
        }

        /// <summary>
        /// ランダムなuint値を生成します。
        /// </summary>
        /// <returns></returns>
        public virtual uint NextUInt()
        {
            return this.GenerateUInt();
        }

        /// <summary>
        /// ０から指定した値を超えないランダムなuint値を生成します。
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public virtual uint NextUInt(uint maxValue)
        {
            return (uint)(this.GenerateUInt() / ((double)uint.MaxValue / maxValue));
        }

        /// <summary>
        /// 指定した範囲のuint値を生成します。
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public virtual uint NextUInt(uint minValue, uint maxValue) /* throws ArgumentOutOfRangeException */
        {
            if (minValue >= maxValue) {
                throw new ArgumentOutOfRangeException();
            }

            return (uint)(this.GenerateUInt() / ((double)uint.MaxValue / (maxValue - minValue)) + minValue);
        }

        /// <summary>
        /// ランダムなint値を生成します。
        /// </summary>
        /// <returns></returns>
        public override int Next()
        {
            return this.Next(int.MaxValue);
        }

        /// <summary>
        /// ０から指定値を超えないランダムな値を生成します。
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int maxValue) /* throws ArgumentOutOfRangeException */
        {
            if (maxValue <= 1) {
                if (maxValue < 0) {
                    throw new ArgumentOutOfRangeException();
                }

                return 0;
            }

            return (int)(this.NextDouble() * maxValue);
        }

        /// <summary>
        /// 指定した範囲内でランダムなint値を生成します。
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public override int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue) {
                throw new ArgumentOutOfRangeException();
            }
            else if (maxValue == minValue) {
                return minValue;
            }
            else {
                return this.Next(maxValue - minValue) + minValue;
            }
        }

        /// <summary>
        /// ランダムなbyte値を生成するジェネレータです。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> EnRandomBytes()
        {
            while (true) {
                uint value = GenerateUInt();
                byte[] bytes = BitConverter.GetBytes(value);
                for (int i = 0; i < bytes.Length; i++) yield return bytes[i];
            }
        }

        /// <summary>
        /// 指定したbyte配列にランダムな値を設定します。
        /// </summary>
        /// <param name="buffer"></param>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException();
            int bufLen = buffer.Length;
            IEnumerator<byte> gen = EnRandomBytes().GetEnumerator();

            for (int idx = 0; idx < bufLen; ++idx) {
                gen.MoveNext();
                buffer[idx] = gen.Current;
            }
        }

        /// <summary>
        /// 0≦x＜1であるランダムなdouble値を返します。
        /// </summary>
        /// <returns></returns>
        public override double NextDouble()
        {
            return (double)this.GenerateUInt() / ((ulong)uint.MaxValue + 1);
        }
    }
}