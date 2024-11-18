namespace GUI.Helpers;

public static class PathHelper
{
    public static string GetMediaFolder()
    {
        string videosPath;

#if ANDROID
        videosPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMovies)?.AbsolutePath ?? "", "YtMediaDownloader");
#elif WINDOWS
        videosPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "YtMediaDownloader");
#else
        videosPath = Path.Combine(FileSystem.AppDataDirectory, "Videos", "YtMediaDownloader");
#endif

        if (!Directory.Exists(videosPath)) Directory.CreateDirectory(videosPath);

        return videosPath;
    }
}
