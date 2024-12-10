using System.Diagnostics;
using System.IO.Compression;

namespace FFmpegLibrary;

internal static class FFmpegUtils
{
	private const string DOWNLOAD_FFMPEG_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/latest/download/ffmpeg-master-latest-win64-gpl.zip";

	public static bool IsFfmpegPresent(string ffmpegPath)
	{
		var ffmpegProcess = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = ffmpegPath,
				Arguments = "-version",
				RedirectStandardError = true,
				UseShellExecute = true,
				CreateNoWindow = true
			}
		};

		ffmpegProcess.Start();
		ffmpegProcess.WaitForExit();

		return ffmpegProcess.ExitCode == 0;
	}

	public static async Task InstallFFmpeg(string binFolder)
	{
		string zipFolder = Path.Combine(binFolder, "ffmpeg.zip");

		await DownloadFFmpeg(zipFolder);
		ExtractFFmpeg(zipFolder, Path.Combine(binFolder, "ffmpeg"));
	}
	private static async Task DownloadFFmpeg(string output)
	{
		using var client = new HttpClient();
		using var response = await client.GetAsync(DOWNLOAD_FFMPEG_URL);

		await using var fs = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None);
		await response.Content.CopyToAsync(fs);
	}

	private static void ExtractFFmpeg(string zipPath, string extractPath)
	{
		if(Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

		ZipFile.ExtractToDirectory(zipPath, extractPath);
	}
}
