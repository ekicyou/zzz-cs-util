using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace CSUtil.Reflection
{
  public static class AssemblyUtil
  {
    /// <summary>
    /// 指定されたアセンブリのPathを返します。
    /// 返すPathはシャドーコピーではなく、本体のPathです。
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetAssemblyPath(Assembly assembly)
    {
      Uri binURI = new Uri(assembly.CodeBase);
      return Path.GetFullPath(binURI.AbsolutePath);
    }

    /// <summary>
    /// 呼び出し元のアセンブリのPathを返します。
    /// 返すPathはシャドーコピーではなく、本体のPathです。
    /// </summary>
    /// <returns></returns>
    public static string GetCallingAssemblyPath()
    {
      return GetAssemblyPath(Assembly.GetCallingAssembly());
    }



    /// <summary>
    /// 指定されたアセンブリのDirecryPathを返します。
    /// 返すPathはシャドーコピーではなく、本体のPathです。
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetAssemblyDirctory(Assembly assembly)
    {
      return Path.GetDirectoryName(GetAssemblyPath(assembly));
    }

    /// <summary>
    /// 呼び出し元のアセンブリの DirecryPathを返します。
    /// 返すPathはシャドーコピーではなく、本体のPathです。
    /// </summary>
    /// <returns></returns>
    public static string GetCallingAssemblyDirctory()
    {
      return GetAssemblyDirctory(Assembly.GetCallingAssembly());
    }

  }
}
