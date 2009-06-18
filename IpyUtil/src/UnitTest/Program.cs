using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil.UnitTest
{
  class Program
  {
    [STAThread]
    static int Main(string[] args)
    {
      return CSUtil.UnitTest.ConsoleRunner.Run(args);
    }
  }
}
