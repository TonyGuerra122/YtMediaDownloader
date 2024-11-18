using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace YtLibrary;

public class YtDownloader
{
    private readonly YoutubeClient _youtubeClient = new();

    public async Task<string> DownloadMediaAsync(string url, string filePath, MediaType mediaType)
    {
        try
        {
            var ytVideo = await _youtubeClient.Videos.GetAsync(url);

            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(url);

            var streamInfo = mediaType == MediaType.AUDIO
                ? streamManifest.GetAudioStreams().GetWithHighestBitrate()
                : streamManifest.GetVideoOnlyStreams()
                    .Where(s => s.Container == Container.Mp4)
                    .GetWithHighestVideoQuality();

            string sanitizedTitle = string.Join("_", ytVideo.Title.Split(Path.GetInvalidFileNameChars()));
            string filePathName = Path.Combine(filePath, $"{sanitizedTitle}.{streamInfo.Container}");

            await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, filePathName);

            return filePathName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao baixar mídia: {ex.Message}");
            throw;
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
