//
// Poor man rotating log
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using BepInEx;

using KKAPI;

#if DEBUG
using Newtonsoft.Json;
#endif


namespace IDHIPlugins
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    public partial class PMRLogs : BaseUnityPlugin
    {
        private const int _totalFiles = 10;

        private void Awake()
        {
            KoikatuAPI.Quitting += OnGameExit;
        }

        /// <summary>
        /// On a clean exit this plugin will save the output_log.txt in UserData/Logs
        /// It will rotate 10 files i.e., after a full rotation there should be 11 files
        /// in the drectory
        ///
        /// TODO: Take care of missing files in the order
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnGameExit(object sender, EventArgs e)
        {
            var fileName = $"output_log.txt";
            var strLogs = new StringBuilder();
            Regex reNumbersEx = new("([0-9]+)", RegexOptions.Compiled);
            FileInfo logFile = new(fileName);

            try
            {
                if (logFile.Exists)
                {
                    using (var stream = File.Open(
                        logFile.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        using StreamReader reader = new(stream);
                        strLogs.Append(reader.ReadToEnd());
                    }

                    var path = Path.Combine(UserData.Path, "Logs");
                    var logOutputFileName = $"{path}/output_log.log";
                    FileInfo logOutputFile = new(logOutputFileName);

                    // Create directory where to save the file.
                    // Safe to do even if directory exists.
                    logOutputFile.Directory.Create();
                    if (logOutputFile.Exists)
                    {
                        if (_totalFiles > 0)
                        {
                            var logsDir = new DirectoryInfo(path);
                            
                            var files = logsDir.GetFiles("output*.log.*")
                                .OrderBy(x => x.Name, new NaturalSortComparer<string>())
                                .ToArray();
                            for (var i = (files.Length - 1); i >= 0; i--)
                            {
                                if (i == _totalFiles)
                                { 
                                    files[i].Delete();
                                    continue;
                                }
                                files[i].MoveTo($"{path}/output_log.log.{i+1}");
                            }
                        }
                    }

                    using (var stream = File.Create(logOutputFile.FullName))
                    {
                        using StreamWriter writer = new(stream);
                        writer.Write(strLogs);
                        writer.Flush();
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"0002: Log file not found {ex}");
            }
        }
    }

    /// <summary>
    /// James McCormack
    /// https://zootfroot.blogspot.com/2009/09/natural-sort-compare-with-linq-orderby.html
    ///
    /// Modified to handle FieldInfo types
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NaturalSortComparer<T> : IComparer<FileInfo>, IComparer<string>, IDisposable
    {
        private readonly bool _isAscending;
        private static readonly Regex reNumbersEx = new("([0-9]+)", RegexOptions.Compiled);

        public NaturalSortComparer(bool inAscendingOrder = true)
        {
            _isAscending = inAscendingOrder;
        }

        public int Compare(string x, string y)
        {
            throw new NotImplementedException();
        }

        internal int CompareString(string x, string y)
        {
            if (x == y)
            {
                return 0;
            }

            if (!table.TryGetValue(x, out var x1))
            {
                //x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                x1 = reNumbersEx.Split(x.Replace(" ", ""));
                table.Add(x, x1);
            }

            if (!table.TryGetValue(y, out var y1))
            {
                //y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
                y1 = reNumbersEx.Split(y.Replace(" ", ""));
                table.Add(y, y1);
            }

            int returnVal;

            for (var i = 0; i < x1.Length && i < y1.Length; i++)
            {

                if (x1[i] == y1[i])
                {
                    continue;
                }

                returnVal = PartCompare(x1[i], y1[i]);
                return _isAscending ? returnVal : -returnVal;
            }

            if (y1.Length > x1.Length)
            {
                returnVal = 1;
            }
            else if (x1.Length > y1.Length)
            {
                returnVal = -1;
            }
            else
            {
                returnVal = 0;
            }

            return _isAscending ? returnVal : -returnVal;

        }

        int IComparer<FileInfo>.Compare(FileInfo x, FileInfo y)
        {
            return CompareString(x.Name, y.Name);
        }

        int IComparer<string>.Compare(string x, string y)
        {
            return CompareString(x, y);
        }

        private static int PartCompare(string left, string right)
        {
            if (!int.TryParse(left, out var x))
            {
                return string.Compare(left, right, StringComparison.InvariantCulture);
            }

            return !int.TryParse(right, out var y)
                ? string.Compare(left, right, StringComparison.InvariantCulture)
                : x.CompareTo(y);
        }

        private Dictionary<string, string[]> table = new();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (table != null)
                {
                    table.Clear();
                    table = null;
                }
            }
        }
    }
}
