using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace IpyUtil
{
  /// <summary>
  /// pythonスクリプト実行のための規定のホストです。
  /// </summary>
  public static class IPyHost
  {
    /// <summary>
    /// 規定のスクリプトエンジンを返します。
    /// </summary>
    public static readonly ScriptEngine Engine = InitEngine();

    private static ScriptEngine InitEngine()
    {
      ScriptEngine engine = IronPython.Hosting.PythonEngine.CurrentEngine;
      engine.Runtime.LoadAssembly(IronPython.Modules.PythonDateTime.time.max.GetType().Assembly);
      foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
        engine.Runtime.LoadAssembly(a);
      }
      return engine;
    }

    /// <summary>
    /// アセンブリを読み込みます。
    /// </summary>
    /// <param name="assembly"></param>
    public static void LoadAssembly(Assembly assembly){
      Engine.Runtime.LoadAssembly(assembly);
    }

    /// <summary>
    /// グローバルスコープでスクリプトを実行し、結果を返します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="script"></param>
    /// <returns></returns>
    public static T EvaluateAs<T>(string script)
    {
      try {
        ScriptSource src = Engine.CreateScriptSourceFromString(script);
        return src.Execute<T>(Engine.Runtime.Globals);
      }
      catch (SyntaxErrorException ex) {
        Debug.WriteLine("*------- eval error!");
        Debug.WriteLine(string.Format("({0}:{1})", ex.Source, ex.RawSpan));
        Debug.WriteLine(script);
        Debug.WriteLine(ex);
        throw;
      }
      catch (Exception ex) {
        Debug.WriteLine(ex);
        throw;
      }
    }

    /// <summary>
    /// グローバルスコープにスクリプトを読み込みます。
    /// </summary>
    /// <param name="script"></param>
    public static void LoadScript(string script)
    {
      try {
        ScriptSource src = Engine.CreateScriptSourceFromString(script, SourceCodeKind.Statements);
        CompiledCode code = src.Compile();
        code.Execute(Engine.Runtime.Globals);
      }
      catch (SyntaxErrorException ex) {
        Debug.WriteLine("*------- eval error!");
        Debug.WriteLine(string.Format("({0}:{1})", ex.Source, ex.RawSpan));
        Debug.WriteLine(script);
        Debug.WriteLine(ex);
        throw;
      }
      catch (Exception ex) {
        Debug.WriteLine(ex);
        throw;
      }
    }



  }
}
