using DSE.MobileTrackingApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;

namespace DSE.MobileTrackingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        // Demo mock data
        //builder.Services.AddSingleton<ITrackingDataService, MockTrackingDataService>();

        // Live API data
        builder.Services.AddHttpClient<ITrackingDataService, ApiTrackingDataService>(client =>
        {
            client.BaseAddress = new Uri("http://10.0.2.2:5116/");
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
