using System;
using System.Collections.Generic;
using System.Text;

namespace CSUtil
{
  public static class EnumUtil<T>
  {
    public static T Parse(string value)
    {
      return (T)Enum.Parse(typeof(T), value);
    }
  }
}
