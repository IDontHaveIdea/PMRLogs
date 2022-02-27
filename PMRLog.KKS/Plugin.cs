using System;
using System.IO;
using System.Text;

using BepInEx;

using KKAPI;

namespace IDHIPlugins
{
    //
    // TODO:
    // Only load for main game no Studio
    // Not loading any type of animations in Studio the loading is just taking time at the moment
    // Regex reg = new Regex(@"^^(?!p_|t_).*");

    //var files = Directory.GetFiles(yourPath, "*.png; *.jpg; *.gif")
    //                     .Where(path => reg.IsMatch(path))
    //                     .ToList();

    // [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    public partial class PMRLogs : BaseUnityPlugin
    {
        private void Awake()
        {
            KoikatuAPI.Quitting += OnGameExit;
        }

        /// <summary>
        /// Game exit event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static internal void OnGameExit(object sender, EventArgs e)
        {
            var fileName = $"output_log.txt";
            var strLogs = new StringBuilder();
            FileInfo file = new(fileName);
            var totalFiles = 10;

            try
            {
                if (file.Exists)
                {
                    using (var stream = File.Open(
                        file.FullName,
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
                        if (totalFiles > 0)
                        {
                            var strTmp = $"[Info:  PMRLog] 0001: File {logOutputFileName} already exits rotate.";
                            strLogs.Append($"{strTmp}\n");
                            Console.WriteLine(strTmp);
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

        /*
        public abstract class Logger : IDisposable
        {
            private LogVerbosity _verbosity;
            private Queue<Action> _queue = new Queue<Action>();
            private ManualResetEvent _hasNewItems = new ManualResetEvent(false);
            private ManualResetEvent _terminate = new ManualResetEvent(false);
            private ManualResetEvent _waiting = new ManualResetEvent(false);
            private Thread _loggingThread;

            private static readonly Lazy<Logger> _lazyLog = new Lazy<Logger>(() => {
                switch (Settings.Default.LogFlow)
                {
                    case (int)LogFlow.Local:
                        return new LocalLogger();
                    case (int)LogFlow.Remote:
                        return new RemoteLogger();
                    default:
                        throw new InvalidOperationException("LogFlow value is invalid. Set valid value in settings based on LogFlow enum.");
                }
            });

            public static Logger Current => _lazyLog.Value;

            protected Logger()
            {
                _verbosity = (LogVerbosity)Settings.Default.LogVerbosity;
                _loggingThread = new Thread(new ThreadStart(ProcessQueue)) { IsBackground = true };
                _loggingThread.Start();
            }

            public void Info(string message)
            {
                Log(message, LogType.INF);
            }

            public void Debug(string message)
            {
                Log(message, LogType.DBG);
            }

            public void Error(string message)
            {
                Log(message, LogType.ERR);
            }

            public void Error(Exception e)
            {
                if (_verbosity != LogVerbosity.None)
                {
                    Log(UnwrapExceptionMessages(e), LogType.ERR);
                }
            }

            public override string ToString() => $"Logger settings: [Type: {this.GetType().Name}, Verbosity: {_verbosity}, ";

            protected abstract void CreateLog(string message);

            public void Flush() => _waiting.WaitOne();

            public void Dispose()
            {
                _terminate.Set();
                _loggingThread.Join();
            }

            protected virtual string ComposeLogRow(string message, LogType logType) =>
                $"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)} {logType}] - {message}";

            protected virtual string UnwrapExceptionMessages(Exception ex)
            {
                if (ex == null)
                    return string.Empty;

                return $"{ex}, Inner exception: {UnwrapExceptionMessages(ex.InnerException)} ";
            }

            private void ProcessQueue()
            {
                while (true)
                {
                    _waiting.Set();
                    int i = WaitHandle.WaitAny(new WaitHandle[] { _hasNewItems, _terminate });
                    if (i == 1) return;
                    _hasNewItems.Reset();
                    _waiting.Reset();

                    Queue<Action> queueCopy;
                    lock (_queue)
                    {
                        queueCopy = new Queue<Action>(_queue);
                        _queue.Clear();
                    }

                    foreach (var log in queueCopy)
                    {
                        log();
                    }
                }
            }

            private void Log(string message, LogType logType)
            {
                if (string.IsNullOrEmpty(message))
                    return;

                var logRow = ComposeLogRow(message, logType);
                System.Diagnostics.Debug.WriteLine(logRow);

                if (_verbosity == LogVerbosity.Full)
                {
                    lock (_queue)
                        _queue.Enqueue(() => CreateLog(logRow));

                    _hasNewItems.Set();
                }
            }
        }

        class LocalLogger : Logger
        {
            private const string LogFolderName = "EDC";
            private const string LogFileName = "EDC.log";
            private readonly int _logChunkSize = Settings.Default.LogChunkSize;
            private readonly int _logChunkMaxCount = Settings.Default.LogChunkMaxCount;
            private readonly int _logArchiveMaxCount = Settings.Default.LogArchiveMaxCount;
            private readonly int _logCleanupPeriod = Settings.Default.LogCleanupPeriod;

            protected override void CreateLog(string message)
            {
                var logFolderPath = Path.Combine(Path.GetTempPath(), LogFolderName);
                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);

                var logFilePath = Path.Combine(logFolderPath, LogFileName);

                Rotate(logFilePath);

                using (var sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine(message);
                }
            }

            private void Rotate(string filePath)
            {
                if (!File.Exists(filePath))
                    return;

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length < _logChunkSize)
                    return;

                var fileTime = DateTime.Now.ToString("dd_MM_yy_h_m_s");
                var rotatedPath = filePath.Replace(".log", $".{fileTime}");
                File.Move(filePath, rotatedPath);

                var folderPath = Path.GetDirectoryName(rotatedPath);
                var logFolderContent = new DirectoryInfo(folderPath).GetFileSystemInfos();

                var chunks = logFolderContent.Where(x => !x.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase));

                if (chunks.Count() <= _logChunkMaxCount)
                    return;

                var archiveFolderInfo = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(rotatedPath), $"{LogFolderName}_{fileTime}"));

                foreach (var chunk in chunks)
                {
                    Directory.Move(chunk.FullName, Path.Combine(archiveFolderInfo.FullName, chunk.Name));
                }

                ZipFile.CreateFromDirectory(archiveFolderInfo.FullName, Path.Combine(folderPath, $"{LogFolderName}_{fileTime}.zip"));
                Directory.Delete(archiveFolderInfo.FullName, true);

                var archives = logFolderContent.Where(x => x.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase)).ToArray();

                if (archives.Count() <= _logArchiveMaxCount)
                    return;

                var oldestArchive = archives.OrderBy(x => x.CreationTime).First();
                var cleanupDate = oldestArchive.CreationTime.AddDays(_logCleanupPeriod);
                if (DateTime.Compare(cleanupDate, DateTime.Now) <= 0)
                {
                    foreach (var file in logFolderContent)
                    {
                        file.Delete();
                    }
                }
                else
                    File.Delete(oldestArchive.FullName);

            }

            public override string ToString() => $"{base.ToString()}, Chunk Size: {_logChunkSize}, Max chunk count: {_logChunkMaxCount}, Max log archive count: {_logArchiveMaxCount}, Cleanup period: {_logCleanupPeriod} days]";
        }

        class RemoteLogger : Logger
        {
            protected async override void CreateLog(string message)
            {
                using (var httpClient = HttpClientProvider.CreateHttpClient(await AuthorizationProvider.Instance.GetAccessToken()))
                {
                    var param = JsonConvert.SerializeObject(new { Message = message });
                    var content = new StringContent(param, Encoding.UTF8, "application/json"); //TODO: OData?

                    try
                    {
                        await httpClient.PostAsync(Settings.Default.LogUrl, new StringContent(param))
                                        .ConfigureAwait(false);
                    }
                    catch (HttpRequestException e)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sending log to remote server: {e}");
                    }
                }
            }

            public override string ToString() => $"{base.ToString()}, Log URL: {Settings.Default.LogUrl}]";
        }

        enum LogVerbosity
        {
            None = 0,
            Exceptions,
            Full
        }

        public enum LogType
        {
            INF,
            DBG,
            ERR
        }

        enum LogFlow
        {
            Local = 0,
            Remote
        }*/
    }
}
