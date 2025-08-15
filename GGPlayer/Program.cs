using Microsoft.Extensions.Logging;
using Serilog;
using Shared;
using System.IO;
using Velopack;
using Shared.Utilities;

namespace GGPlayer
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (FontInstaller.IsWindows7())
            {
                FontInstaller.RunFontInstallScript();
            }

            SerilogHelper.Configure();
            Translations.SetToCulture("uk");
            VelopackApp.Build().Run(LoggingInstance<Program>());

            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        public static Microsoft.Extensions.Logging.ILogger LoggingInstance<T>()
        {
            ILoggerFactory factory = new LoggerFactory().AddSerilog(Log.Logger);
            return factory.CreateLogger<T>();
        }
    }
}
