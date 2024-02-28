using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CliWrap.Builders;

namespace GenshinStellaMod.Scripts
{
	internal static class Logic
	{
		public static readonly string FpsUnlockerPath = Path.Combine(Program.AppPath, "data", "unlocker", "gen-fps-unlocker.exe");
		public static readonly string ReShadeDllPath = Path.Combine(Program.AppPath, "data", "reshade", "ReShade64.dll");
		public static readonly string InjectorPath = Path.Combine(Program.AppPath, "data", "reshade", "inject64.exe");

		public static void UnlockFps()
		{
			Console.Write("[i] Waiting for unlocking FPS... ");
			_ = Cmd.Execute(new Cmd.CliWrap { App = FpsUnlockerPath, WorkingDir = Path.GetDirectoryName(FpsUnlockerPath) });


			string targetProcessName = Path.GetFileName(FpsUnlockerPath).Replace(".exe", "");
			bool processFound = false;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			while (!processFound && stopwatch.ElapsedMilliseconds < 6000)
			{
				Process[] processes = Process.GetProcessesByName(targetProcessName);

				if (processes.Length > 0)
				{
					foreach (Process process in processes) Console.WriteLine($"Found process PID{process.Id} ᕱ⑅︎ᕱ!\n[✓] Remember to set the correct FPS limit. Now run the game using an FPS unlocking program.\n");

					processFound = true;
				}

				Thread.Sleep(1000);
			}

			if (!processFound) Log.ThrowErrorString($"[x] Failed. Process {targetProcessName} was not found within 6 seconds.");
		}

		public static async Task InjectReShade()
		{
			string gamePath = Data.GetGamePath();
			string exe = Path.GetFileName(gamePath);

			Console.WriteLine($"[i] Awaiting injection of ReShade for {exe}...");
			await Cmd.Execute(new Cmd.CliWrap
			{
				App = InjectorPath,
				WorkingDir = Path.GetDirectoryName(ReShadeDllPath),
				Arguments = new ArgumentsBuilder()
					.Add(exe)
			});
			Console.WriteLine("[i] Completed\n");
		}

		public static async Task Migoto()
		{
			bool isSubscriber = Data.IsUserMyPatron();
			if (!isSubscriber || !Secret.IsMyPatron)
			{
				Log.ThrowError(new Exception("[x] This action cannot be carried out"));
				MessageBox.Show("You cannot use 3DMigoto in Genshin Stella Mod without having the 'Stella Mod Plus+' version for subscribers or patrons.", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(666);
			}

			string resources = Utils.GetResourcesPath();
			if (!Directory.Exists(resources)) Log.ThrowErrorString($"[x] Directory {resources} was not found");

			string migoto = Path.Combine(resources, "3DMigoto", "loader.exe");
			if (!File.Exists(migoto))
			{
				Log.ThrowErrorString($"[x] File {migoto} was not found");
				MessageBox.Show($"The require file {Path.GetFileName(migoto)} was not found.\nPlease import all the benefits from the Stella Mod Plus subscriber application.", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			string gamePath = Data.GetGamePath();

			Console.WriteLine("[i] Waiting for injecting 3DMigoto...");
			await Cmd.Execute(new Cmd.CliWrap
			{
				App = migoto,
				WorkingDir = Path.GetDirectoryName(migoto),
				Arguments = new ArgumentsBuilder()
					.Add(Path.Combine(gamePath))
			});
			Console.WriteLine("[i] Completed");
		}

		public static void Completed()
		{
			Console.WriteLine("\n=========================================================================================");

			bool isSubscriber = Data.IsUserMyPatron();
			if (!isSubscriber)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(@"       [✓] Success! If you need any assistance, please visit our Discord server.");
				Console.WriteLine(@"[i] This software is provided with continuous technical support only thanks to your support.");
				Console.WriteLine(@"[i] If you like Stella Mod, upgrade to Stella Mod Plus: https://genshin.sefinek.net/subscription");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(@"          [✓] All set! The game should start shortly. Have a great adventure!");
				Console.WriteLine(@"[i] Thank you for supporting me. Your generosity motivates me to further develop this software.");

				Sound.Play("winxp", "print_complete");
			}

			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("\n[i] The application will be closed in 8 seconds...");

			Thread.Sleep(7500);
			Environment.Exit(0);
		}
	}
}
