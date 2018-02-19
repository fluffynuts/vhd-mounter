using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PeanutButter.INIFile;
using PeanutButter.ServiceShell;
using PeanutButter.Utils;

namespace VHDMounter
{
    public class VhdMounterShell : Shell
    {
        private INIFile _ini;

        public VhdMounterShell()
        {
            Interval = 3600;
            ServiceName = "vhdmounter";
            DisplayName = "VHD Mounter";
        }

        protected override void RunOnce()
        {
            if (!RunningOnceFromCLI)
                return;

            var action = (Environment.GetEnvironmentVariable("action") ?? "attach")
                .ToLowerInvariant();
            if (!new[] {"attach", "detach"}.Contains(action))
            {
                Console.WriteLine($"Unknown action {action} (try one of 'attach' or 'detach')");
                return;
            }

            var ini = ReadLocalSetup();
            DoAction(action, ini);
        }

        protected override void OnStart(string[] args)
        {
            _ini = ReadLocalSetup(); // store for same config at stopping time
            DoAction("attach", _ini);
            base.OnStart(args);
        }

        private INIFile ReadLocalSetup()
        {
            var iniPath = Path.Combine(
                Path.GetDirectoryName(
                    new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath
                ), "vhdmounter.ini");
            return new INIFile(iniPath);
        }

        protected override void OnStop()
        {
            DoAction("detach", _ini);
            _ini = null;
            base.OnStop();
        }

        private const string KEY = "path";

        private void DoAction(string action, INIFile ini)
        {
            ini.Sections.ForEach(section =>
            {
                if (!ini[section].ContainsKey(KEY))
                {
                    LogWarning($"{section} has no {KEY} setting");
                    return;
                }

                var vhdPath = ini[section]["path"];
                if (!File.Exists(vhdPath))
                {
                    LogWarning($"VHD {vhdPath} not found");
                    return;
                }

                var tempFile = Path.GetTempFileName();
                using (new AutoDeleter(tempFile))
                {
                    CreateDispartFile(tempFile, vhdPath, action);
                    RunDiskpartWith(tempFile);
                }
            });
        }

        private void RunDiskpartWith(string tempFile)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "diskpart.exe",
                    Arguments = $"/s \"{tempFile}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                LogWarning(new[]
                {
                    "Unable to mount VHD: diskpart is unhappy:",
                    $"Exit code: {process.ExitCode}",
                    $"StdOut: {process.StandardOutput.ReadToEnd()}",
                    $"StdErr: {process.StandardError.ReadToEnd()}"
                }.JoinWith(Environment.NewLine));
            }
        }

        private static void CreateDispartFile(
            string tempFile,
            string vhdPath,
            string command
        )
        {
            var cmd = new[]
            {
                $"select vdisk file=\"{vhdPath}\"",
                $"{command} vdisk"
            }.JoinWith(Environment.NewLine);
            File.WriteAllBytes(tempFile, Encoding.UTF8.GetBytes(cmd));
        }
    }
}