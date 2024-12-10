using FFmpegLibrary;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YtLibrary;

public class YtDownloader(string binFolder)
{
	private readonly FFmpegService _ffmpegService = new(binFolder);

	private readonly YoutubeClient _youtubeClient = new();

	public async Task<string> DownloadMediaAsync(string url, string filePath, MediaType mediaType)
	{
		try
		{
			var ytVideo = await _youtubeClient.Videos.GetAsync(url);

			var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(url);

			string filePathName = await WriteStreamToFile(streamManifest, mediaType, filePath, ytVideo);

			return filePathName;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro ao baixar mídia: {ex.Message}");
			throw;
		}
	}

	private async Task<string> WriteStreamToFile(StreamManifest streamManifest, MediaType mediaType, string filePath, Video video)
	{
		Stream? videoStream = null;
		Stream? audioStream = null;

		try
		{
			string fileExt = mediaType.Equals(MediaType.VIDEO) ? "mp4" : "mp3";

			string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
			string filePathName = Path.Combine(filePath, $"{sanitizedTitle}.{fileExt}");

			audioStream = await _youtubeClient.Videos.Streams.GetAsync(streamManifest.GetAudioStreams().GetWithHighestBitrate());

			if (mediaType == MediaType.VIDEO)
			{
				videoStream = await _youtubeClient.Videos.Streams.GetAsync
				(
					streamManifest.GetVideoOnlyStreams()
						.Where(s => s.Container == Container.Mp4)
						.GetWithHighestVideoQuality()
				);

				await _ffmpegService.JoinStreamsToFile(videoStream, audioStream, filePathName);
			}
			else
			{
				await using var fileStream = File.Create(filePathName);
				await audioStream.CopyToAsync(fileStream);
			}

			return filePathName;
		}
		finally
		{
			videoStream?.Dispose();
			audioStream?.Dispose();	
		}
	}

	public async Task<VideoInfo> GetVideoInfo(string url)
	{
		var ytVideo = await _youtubeClient.Videos.GetAsync(url);

		var highestThumbnail = ytVideo.Thumbnails.GetWithHighestResolution();

		return new VideoInfo(
			ytVideo.Title,
			ytVideo.Author.ChannelTitle,
			ytVideo.Duration ?? TimeSpan.Zero,
			highestThumbnail.Url
		);
	}

}
