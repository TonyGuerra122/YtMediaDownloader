using Blazored.Toast;
using Blazored.Toast.Services;
using Microsoft.Extensions.Logging;
using YtLibrary;

namespace GUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        builder.Services.AddBlazoredToast();

        builder.Services.AddSingleton<IToastService, ToastService>();

        builder.Services.AddSingleton<YtDownloader>();

        return builder.Build();
    }
}
