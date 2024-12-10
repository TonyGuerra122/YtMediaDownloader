using FFmpegLibrary.Errors;
using System.Diagnostics;

namespace FFmpegLibrary;

public class FFmpegService
{
	private readonly string _binariesFolder;
	private readonly string _ffmpegFolder;
	private readonly string _ffmpegPath;

	public FFmpegService(string binariesFolder)
	{
		_binariesFolder = binariesFolder;
		_ffmpegFolder = Path.Combine(binariesFolder, "ffmpeg");
		_ffmpegPath = Path.Combine(_ffmpegFolder, "ffmpeg.exe");
	}

	public async Task JoinStreamsToFile(Stream videoStream, Stream audioStream, string outputFile)
	{
		string tempVideoPath = Path.Combine(_ffmpegFolder, "temp_video.mp4");
		string tempAudioPath = Path.Combine(_ffmpegFolder, "temp_audio.aac");

		try
		{
			if (!FFmpegUtils.IsFfmpegPresent(_ffmpegPath)) await FFmpegUtils.InstallFFmpeg(_binariesFolder);

			await SaveStreamToFile(videoStream, tempVideoPath);
			await SaveStreamToFile(audioStream, tempAudioPath);

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = _ffmpegPath,
					Arguments = $"-y -loglevel debug -i \"{tempVideoPath}\" -i \"{tempAudioPath}\" -c:v copy -c:a aac \"{outputFile}\"",
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();

			string errorOutput = await process.StandardError.ReadToEndAsync();

			process.WaitForExit();

			if (process.ExitCode != 0)
				throw new FFmpegException($"Erro no FFmpeg: {errorOutput}");
		}
		catch (Exception ex)
		{
			throw new FFmpegException($"Erro ao executar o FFmpeg: {ex.Message}");
		}
		finally
		{
			if (File.Exists(tempVideoPath))
				File.Delete(tempVideoPath);

			if (File.Exists(tempAudioPath))
				File.Delete(tempAudioPath);

			videoStream?.Dispose();
			audioStream?.Dispose();
		}
	}

	private static async Task SaveStreamToFile(Stream inputStream, string filePath)
	{
		await using var fileStream = File.Create(filePath);
		await inputStream.CopyToAsync(fileStream);
	}
}
