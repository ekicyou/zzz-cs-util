using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSUtil.IO
{
    /// <summary>
    /// File操作ユーティリティ(.net 3.0以降)
    /// </summary>
    public static class FileUtil3
    {
        /// <summary>
        /// 指定されたディレクトリ配下の全てのファイルをFileInfoとして列挙します。
        /// </summary>
        /// <param name="baseDir">列挙するディレクトリ</param>
        /// <returns>FileInfoの列挙子</returns>
        public static IEnumerable<FileInfo> EnFileInfo(string baseDir)
        {
            foreach (string path in Directory.GetFiles(
                baseDir, "*.*", SearchOption.AllDirectories)) {

                yield return new FileInfo(path);
            }
        }
    }
}
