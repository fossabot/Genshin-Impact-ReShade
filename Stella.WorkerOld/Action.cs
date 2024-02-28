using System;
using System.IO;
using System.Threading.Tasks;
using GenshinStellaMod.Scripts;

namespace GenshinStellaMod
{
	internal static class Action
	{
		public static async Task Run(string launchMode)
		{
			string gamePath = Data.GetGamePath();
			if (string.IsNullOrEmpty(gamePath))
			{
				Log.ThrowErrorString("[x] The game path was not found");

				Utils.Pause();
			}

			// Check if the required files exist
			if (!File.Exists(Logic.FpsUnlockerPath))
			{
				Log.ThrowErrorString($"[x] Failed to start. File {Path.GetFileName(Logic.FpsUnlockerPath)} was not found.");
				return;
			}

			if (!File.Exists(Logic.ReShadeDllPath))
			{
				Log.ThrowErrorString($"[x] Failed to start. File {Path.GetFileName(Logic.ReShadeDllPath)} was not found.");
				return;
			}

			if (!File.Exists(Logic.InjectorPath))
			{
				Log.ThrowErrorString($"[x] Failed to start. File {Path.GetFileName(Logic.InjectorPath)} was not found.");
				return;
			}

			Console.WriteLine("[✓] Found required files");


			// Check processes
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("\n2/3 - Checking processes...");
			Console.ResetColor();


			// Stella
			Data.CheckProcess("Stella Mod Launcher.exe");
			Data.CheckProcess("Prepare Stella Mod.exe");

			// FPS Unlock
			Data.CheckProcess("gen-fps-unlocker.exe");

			// Game
			Data.CheckProcess("launcher.exe");
			Data.CheckProcess("Genshin.exe");
			Data.CheckProcess("GenshinImpact.exe");
			Data.CheckProcess("YuanShen.exe");

			// Injectors
			Data.CheckProcess("loader.exe");
			Data.CheckProcess("inject.exe");
			Data.CheckProcess("inject64.exe");
			Data.CheckProcess("inject32.exe");

			Data.CheckProcess("injector.exe");
			Data.CheckProcess("injector64.exe");
			Data.CheckProcess("injector32.exe");

			// 3DMigoto
			if (!Secret.IsMyPatron)
			{
				Data.CheckProcess("3DMigoto Loader.exe");
				Data.CheckProcess("3DMigoto loader.exe");
				Data.CheckProcess("3DMigoto.exe");
				Data.CheckProcess("3Dmigoto.exe");
				Data.CheckProcess("3dmigoto.exe");
				Data.CheckProcess("3dm.exe");
			}


			Console.WriteLine("[✓] Completed");


			// Inject
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine($"\n3/3 - Preparing to start (launch mode {launchMode})...");
			Console.ResetColor();

			switch (launchMode)
			{
				case "1":
				{
					Logic.UnlockFps();

					await Logic.InjectReShade();
					await Logic.Migoto();
					break;
				}
				case "3":
				{
					await Logic.InjectReShade();
					break;
				}
				case "4":
				{
					Logic.UnlockFps();

					Console.WriteLine("~~ Please start the game now. ~~");
					Console.WriteLine();
					break;
				}
				case "5":
				{
					await Logic.Migoto();
					break;
				}
				case "6":
				{
					Logic.UnlockFps();
					await Logic.InjectReShade();
					break;
				}
				default:
					Console.WriteLine("[x] Failed to start. Invalid launch mode.");
					return;
			}

			Logic.Completed();
			Utils.Pause();
		}
	}
}
