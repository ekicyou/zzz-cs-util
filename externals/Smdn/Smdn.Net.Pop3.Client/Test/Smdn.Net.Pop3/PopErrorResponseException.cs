using System;
using NUnit.Framework;

namespace Smdn.Net.Pop3 {
  [TestFixture]
  public class PopErrorResponseExceptionTests {
    [Test]
    public void TestSerializeBinary()
    {
      TestUtils.SerializeBinary(new PopErrorResponseException());
    }
  }
}

