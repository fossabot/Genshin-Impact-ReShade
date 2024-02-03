using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using ByteSizeLib;
using ClassLibrary;
using StellaLauncher.Forms;
using StellaLauncher.Properties;
using StellaLauncher.Scripts.Forms.MainForm;

namespace StellaLauncher.Scripts.Download
{
	internal static class DownloadResources
	{
		// Files
		private static string _stellaResZip;

		// Download
		private static double _downloadSpeed;
		private static long _lastBytesReceived;
		private static DateTime _lastUpdateTime = DateTime.Now;


		public static async void Run(string localResVersion, string remoteResVersion, string remoteResDate)
		{
			// 1
			Default._version_LinkLabel.Text = $@"v{localResVersion} → v{remoteResVersion}";

			// 2
			Default._updates_LinkLabel.LinkColor = Color.Cyan;
			Default._updates_LinkLabel.Text = Resources.NormalRelease_ClickHereToUpdate;
			Default._updateIco_PictureBox.Image = Resources.icons8_download_from_the_cloud;
			Utils.RemoveClickEvent(Default._updates_LinkLabel);
			Default._updates_LinkLabel.Click += Update_Event;

			// Hide and show elements
			Default._progressBar1.Hide();
			Default._preparingPleaseWait.Hide();
			Default._preparingPleaseWait.Text = Resources.NormalRelease_Preparing_IfProcessIsStuckReopenLauncher;

			Default._progressBar1.Value = 0;

			// BalloonTip
			BalloonTip.Show($"📥 {Resources.NormalRelease_WeFoundNewUpdates}", Resources.NormalRelease_NewReleaseIsAvailableDownloadNow);


			// Date
			DateTime date = DateTime.Parse(remoteResDate, null, DateTimeStyles.RoundtripKind).ToUniversalTime().ToLocalTime();

			// Log
			Program.Logger.Info($"Found the new update of resources from {date} - {remoteResDate}");

			Default._status_Label.Text += $"[i] {string.Format(Resources.StellaResources_NewResourcesUpdateIsAvailable, date)}\n";
			_stellaResZip = Path.Combine(Default.ResourcesPath, $"Stella resources - v{remoteResVersion}.zip");


			// Check update size
			using (WebClient wc = new WebClient())
			{
				wc.Headers.Add("User-Agent", Program.UserAgent);
				await wc.OpenReadTaskAsync("https://github.com/sefinek24/Genshin-Stella-Resources/releases/latest/download/resources.zip");
				string updateSize = ByteSize.FromBytes(Convert.ToInt64(wc.ResponseHeaders["Content-Length"])).MegaBytes.ToString("00.00");
				Default._status_Label.Text += $"[i] {string.Format(Resources.StellaResources_UpdateSize, $"{updateSize} MB")}\n";

				// Final
				TaskbarProgress.SetProgressState(TaskbarProgress.Flags.Paused);
				Program.Logger.Info(string.Format(Resources.StellaResources_UpdateSize, $"{updateSize} MB"));
			}

			// Taskbar
			TaskbarProgress.SetProgressValue(100);
		}

		private static async void Update_Event(object sender, EventArgs e)
		{
			Program.Logger.Info(Resources.NormalRelease_PreparingToDownloadNewUpdate);
			TaskbarProgress.SetProgressState(TaskbarProgress.Flags.Normal);

			Default._updates_LinkLabel.LinkColor = Color.DodgerBlue;
			Default._updates_LinkLabel.Text = Resources.NormalRelease_UpdatingPleaseWait;
			Utils.RemoveClickEvent(Default._updates_LinkLabel);

			Default._progressBar1.Show();
			Default._preparingPleaseWait.Show();

			Default._discordServerIco_Picturebox.Hide();
			Default._discordServer_LinkLabel.Hide();
			Default._supportMeIco_PictureBox.Hide();
			Default._supportMe_LinkLabel.Hide();
			Default._youtubeIco_Picturebox.Hide();
			Default._youTube_LinkLabel.Hide();

			try
			{
				Program.Logger.Info("Starting...");
				await StartDownload();
			}
			catch (Exception ex)
			{
				Default._preparingPleaseWait.Text = $@"😥 {Resources.NormalRelease_SomethingWentWrong}";
				Log.ThrowError(ex);
			}

			Program.Logger.Info($"Output: {_stellaResZip}");
		}


		private static async Task StartDownload()
		{
			if (File.Exists(Default.ResourcesPath)) File.Delete(Default.ResourcesPath);

			Program.Logger.Info("Downloading in progress...");
			TaskbarProgress.SetProgressState(TaskbarProgress.Flags.Normal);

			using (WebClient client = new WebClient())
			{
				client.Headers.Add("User-Agent", Program.UserAgent);
				client.DownloadProgressChanged += Client_DownloadProgressChanged;
				client.DownloadFileCompleted += Client_DownloadFileCompleted;
				await client.DownloadFileTaskAsync(new Uri("https://github.com/sefinek24/Genshin-Stella-Resources/releases/latest/download/resources.zip"), _stellaResZip);
			}
		}

		private static void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			int progress = (int)Math.Floor(e.BytesReceived * 100.0 / e.TotalBytesToReceive);
			Default._progressBar1.Value = progress;
			TaskbarProgress.SetProgressValue((ulong)progress);

			DateTime currentTime = DateTime.Now;
			TimeSpan elapsedTime = currentTime - _lastUpdateTime;
			long bytesReceived = e.BytesReceived - _lastBytesReceived;

			if (!(elapsedTime.TotalMilliseconds > 1000)) return;

			_lastUpdateTime = currentTime;
			_lastBytesReceived = e.BytesReceived;

			double bytesReceivedMb = ByteSize.FromBytes(e.BytesReceived).MegaBytes;
			double bytesReceiveMb = ByteSize.FromBytes(e.TotalBytesToReceive).MegaBytes;

			_downloadSpeed = bytesReceived / elapsedTime.TotalSeconds;
			double downloadSpeedInMb = _downloadSpeed / (1024 * 1024);

			Default._preparingPleaseWait.Text = $@"{string.Format(Resources.StellaResources_DownloadingResources, $"{bytesReceivedMb:00.00}", $"{bytesReceiveMb:00.00}")} [{downloadSpeedInMb:00.00} MB/s]";

			Program.Logger.Info($"Downloading new update... {bytesReceivedMb:00.00} MB of {bytesReceiveMb:000.00} MB / {downloadSpeedInMb:00.00} MB/s");
		}

		private static async void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			// Unpack new mods
			Default._preparingPleaseWait.Text = Resources.StellaResources_UnpackingFiles;
			await UnzipWithProgress(_stellaResZip, Default.ResourcesPath);

			// Done!
			Default._progressBar1.Hide();
			Default._preparingPleaseWait.Hide();

			Default._discordServerIco_Picturebox.Show();
			Default._discordServer_LinkLabel.Show();
			Default._supportMeIco_PictureBox.Show();
			Default._supportMe_LinkLabel.Show();
			Default._youtubeIco_Picturebox.Show();
			Default._youTube_LinkLabel.Show();

			Default._status_Label.Text += $"[✓] {Resources.StellaResources_SuccessfullyUpdatedResources}\n";
			await CheckForUpdates.Analyze();
		}

		public static async Task UnzipWithProgress(string zipFilePath, string extractPath)
		{
			FileInfo fileInfo = new FileInfo(zipFilePath);
			if (!fileInfo.Exists)
			{
				string msg = $"The downloaded file cannot be unzipped because it does not exist. This is really strange. Please check your antivirus software. Contact the application developer for assistance.\n\nPath: {zipFilePath}";
				Program.Logger.Error(msg);
				MessageBox.Show(msg, Program.AppNameVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(412411);
			}

			if (fileInfo.Length == 0)
			{
				const string msg =
					"The downloaded ZIP file is empty and cannot be unzipped. This situation might be due to a problem with the download process or a corruption of the archive. Please verify your antivirus software or check your internet connection. The file will be automatically deleted from your hard drive once this message is closed. If you require further assistance, do not hesitate to contact the developer.";
				Program.Logger.Error(msg);
				MessageBox.Show(msg, Program.AppNameVer, MessageBoxButtons.OK, MessageBoxIcon.Error);
				File.Delete(fileInfo.FullName);
				Environment.Exit(412412);
			}

			using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
			{
				int totalEntries = archive.Entries.Count;
				int currentEntry = 0;
				long totalBytes = 0;
				long totalBytesToExtract = archive.Entries.Sum(entry => entry.Length);

				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					string entryPath = Path.Combine(extractPath, entry.FullName);

					if (entry.FullName.EndsWith("/"))
					{
						Directory.CreateDirectory(entryPath);
						continue;
					}

					Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

					using (Stream source = entry.Open())
					using (Stream destination = File.Create(entryPath))
					{
						byte[] buffer = new byte[8192];
						int bytesRead;

						while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
						{
							await destination.WriteAsync(buffer, 0, bytesRead);
							totalBytes += bytesRead;

							int progressPercentage = (int)((double)totalBytes / totalBytesToExtract * 100);
							Default._progressBar1.Value = progressPercentage;
							TaskbarProgress.SetProgressValue((ulong)progressPercentage);
						}
					}

					currentEntry++;

					Default._preparingPleaseWait.Text = string.Format(Resources.StellaResources_UnpackingFiles_From_, currentEntry, totalEntries);
				}

				Program.Logger.Info($"Successfully unpacked; totalEntries {totalEntries}; totalBytes: {totalBytes}; totalBytesToExtract: {totalBytesToExtract};");
			}
		}
	}
}
