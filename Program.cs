using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using System.Configuration;
using System.Linq;
using LibGit2Sharp;


namespace ConsoleApp1
{
    class Program
    {
        private static string NoteDirectory;
        private static string GitExeLocation;
        private static string GitBranch;
        private static string CommitInfo;
        private static long FileChangeCount = 0;
        private static AsyncAutoResetEvent fileChangeEvent = new (false);

        static async Task Main(string[] args)
        {
            NoteDirectory = ConfigurationManager.AppSettings["NoteDirectory"];
            GitExeLocation = ConfigurationManager.AppSettings["GitExeLocation"];
            CommitInfo = ConfigurationManager.AppSettings["CommitInfo"];
            if (!Directory.Exists(NoteDirectory))
            {
                Console.Write("未找到"+ NoteDirectory);
                return;
            }
            using var repo = new Repository(NoteDirectory);
            foreach (Branch b in repo.Branches.Where(b => !b.IsRemote))
            {
                if (b.IsCurrentRepositoryHead) GitBranch = b.FriendlyName;
            }

            // 创建一个新的FileSystemWatcher实例
            FileSystemWatcher watcher = new FileSystemWatcher();

            // 设置要监控的文件夹路径
            watcher.Path = NoteDirectory;

            // 监控子文件夹的变化
            watcher.IncludeSubdirectories = true;

            // 监控文件的类型，可以根据需要进行调整
            watcher.Filter = "*.*";

            // 添加事件处理程序
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.Deleted += OnFileChanged;
            watcher.Renamed += OnFileChanged;

            // 启动监控
            watcher.EnableRaisingEvents = true;
            Task.Run(CommitThread);
            Console.WriteLine("正在监控文件夹：" + NoteDirectory);
            Console.WriteLine("按任意键退出。");
            Console.ReadKey();

            // 停止文件监控和定时器
            watcher.EnableRaisingEvents = false;
        }

        // 文件变动事件处理程序
        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.Contains(".git"))
                return;
            var uncommitedCount = Interlocked.Increment(ref FileChangeCount);
            fileChangeEvent.Set();
            Console.WriteLine($"当前待提交更改：{uncommitedCount}");
        }

        private static async Task CommitThread()
        {
            while (true)
            {
                if (Interlocked.Exchange(ref FileChangeCount, 0) > 0)
                {
                    await Commit();
                }
                await fileChangeEvent.WaitAsync();
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        private static async Task Commit()
        {
            Console.WriteLine($"发起提交");
            await RunGitCommand("add -A");
            await RunGitCommand("commit -m \""+ CommitInfo + "\"");
            await RunGitCommand("pull origin "+GitBranch);
            await RunGitCommand("push -u origin " + GitBranch);
        }

        private static async Task RunGitCommand(string arguments)
        {
            var processStartInfo = new ProcessStartInfo(GitExeLocation, arguments);
            processStartInfo.WorkingDirectory = NoteDirectory;
            var process = Process.Start(processStartInfo);
            if (process is null)
            {
                Console.WriteLine("Git Process Is Null");
                return;
            }
            await process.WaitForExitAsync();
        }
    }
}
