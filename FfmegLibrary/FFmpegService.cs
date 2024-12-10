using FFmpegLibrary.Errors;
using System.Diagnostics;

namespace FFmpegLibrary;

public class FFmpegService(string ffmpegFolder)
{
	private readonly string _ffmpegFolder = ffmpegFolder;
	private readonly string _ffmpegPath = Path.Combine(ffmpegFolder, "ffmpeg.exe");

	public async Task JoinStreamsToFile(Stream videoStream, Stream audioStream, string outputFile)
	{
		if (!FFmpegUtils.IsFfmpegPresent(_ffmpegFolder))
			await FFmpegUtils.InstallFFmpeg(_ffmpegFolder);

		try
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = _ffmpegPath,
					Arguments = $"-y -i pipe:0 -i pipe:1 -c:v copy -c:a aac {outputFile}",
					RedirectStandardInput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};

			process.Start();

			var inputStream = process.StandardInput.BaseStream;
			var videoTask = videoStream.CopyToAsync(inputStream);
			var audioTask = audioStream.CopyToAsync(inputStream);

			await Task.WhenAll(videoTask, audioTask);

			inputStream.Close();

			string errorOutput = await process.StandardError.ReadToEndAsync();

			process.WaitForExit();

			if (process.ExitCode != 0)
				throw new Exception($"Erro no FFmpeg: {errorOutput}");
		}
		catch (Exception ex)
		{
			throw new FFmpegException($"Erro ao executar o FFmpeg: {ex.Message}");
		}
	}
}
