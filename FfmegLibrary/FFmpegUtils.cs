using System.Diagnostics;
using System.IO.Compression;

namespace FFmpegLibrary;

internal static class FFmpegUtils
{
	private const string DOWNLOAD_FFMPEG_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
	private const string EXTRACTED_FOLDER_NAME = "ffmpeg-master-latest-win64-gpl";

	public static bool IsFfmpegPresent(string ffmpegPath)
	{
		if (!File.Exists(ffmpegPath)) return false;

		var ffmpegProcess = new Process
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = ffmpegPath,
				Arguments = "-version",
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};

		try
		{
			ffmpegProcess.Start();
			ffmpegProcess.WaitForExit();

			return ffmpegProcess.ExitCode == 0;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro ao verificar o FFmpeg: {ex.Message}");
			return false;
		}
	}

	public static async Task InstallFFmpeg(string binFolder)
	{
		string extractPath = Path.Combine(binFolder, "ffmpeg");
		string zipPath = Path.Combine(binFolder, "ffmpeg.zip");

		if (!Directory.Exists(binFolder)) Directory.CreateDirectory(binFolder);

		await DownloadFFmpeg(zipPath);

		ExtractFFmpeg(zipPath, extractPath);

		if (File.Exists(zipPath)) File.Delete(zipPath);

		string extractedFFmpeg = Path.Combine(extractPath, EXTRACTED_FOLDER_NAME);

		if (Directory.Exists(extractedFFmpeg))
		{
			try
			{
				Directory.Delete(extractedFFmpeg, true);
			}
			catch (Exception ex) 
			{
				Console.WriteLine($"Ocorreu um erro ao excluir o diretório: {ex}");
			}
		}
	}

	private static async Task DownloadFFmpeg(string output)
	{
		using var client = new HttpClient
		{
			Timeout = TimeSpan.FromMinutes(5)
		};

		using var response = await client.GetAsync(DOWNLOAD_FFMPEG_URL);

		if (!response.IsSuccessStatusCode)
			throw new Exception($"Falha ao baixar o FFmpeg: {response.StatusCode}");

		await using var fs = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None);
		await response.Content.CopyToAsync(fs);
	}

	private static void ExtractFFmpeg(string zipPath, string extractPath)
	{
		ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);
		MoveFilesToFFmpegFolder(extractPath);
	}

	private static void MoveFilesToFFmpegFolder(string extractPath)
	{
		string binFFmpegFolder = Path.Combine(extractPath, EXTRACTED_FOLDER_NAME, "bin");

		if (!Directory.Exists(binFFmpegFolder))
			throw new Exception($"A pasta de origem '{binFFmpegFolder}' não existe.");

		string[] files = Directory.GetFiles(binFFmpegFolder);

		foreach (string file in files)
		{
			string fileName = Path.GetFileName(file);
			string destinationFile = Path.Combine(extractPath, fileName);

			try
			{
				File.Move(file, destinationFile);
			}
			catch (IOException ex)
			{
				Console.WriteLine($"Erro ao mover o arquivo '{fileName}': {ex.Message}");
			}
		}

		if (Directory.Exists(binFFmpegFolder))
		{
			try
			{
				Directory.Delete(binFFmpegFolder, true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro ao excluir a pasta '{binFFmpegFolder}': {ex.Message}");
			}
		}
	}

	public static string GetFFmpegPath(string binFolder)
	{
		return Path.Combine(binFolder, "ffmpeg.exe");
	}
}
