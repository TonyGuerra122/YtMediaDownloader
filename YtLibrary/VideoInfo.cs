namespace YtLibrary;

public record VideoInfo(
    string Title,
    string Author,
    TimeSpan Duration,
    string ThumbUrl
);
