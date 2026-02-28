using Microsoft.Extensions.Logging;

namespace Session_5_Dennis_Hilfinger
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("texgyreadventor-bold.otf", "TexBold");
                    fonts.AddFont("texgyreadventor-bolditalic.otf", "TexBoldItalic");
                    fonts.AddFont("texgyreadventor-italic.otf", "TexItalic");
                    fonts.AddFont("texgyreadventor-regular.otf", "Tex");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
