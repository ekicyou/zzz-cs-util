using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CSUtil.Transactions
{
  public partial class TxF
  {
    /// <summary>
    /// トランザクションファイル操作を行います。
    /// </summary>
    public class TxFile
    {
      private readonly SafeTransactionHandle TxHandle;

      internal TxFile(SafeTransactionHandle txHandle)
      {
        TxHandle = txHandle;
      }



    }
  }
}
