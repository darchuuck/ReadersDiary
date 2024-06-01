using Botukbooks.Client;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Botukbooks
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<BookApiClient>(client =>
            {
                client.BaseAddress = new Uri(Constants.Address);
            });
            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(Constants.Token));
            services.AddTransient<TelegramBookBot>(); 
        }

      
    }
    public class Program
    {

        private static IHost host;

        public static async Task Main(string[] args)
        {
            host = CreateHostBuilder(args).Build();

            var botClient = host.Services.GetRequiredService<ITelegramBotClient>();
            var bot = host.Services.GetRequiredService<TelegramBookBot>();

            using var cts = new CancellationTokenSource();

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
     updateHandler: async (client, update, cancellationToken) => await bot.HandleUpdateAsync(client, update, cancellationToken),
     pollingErrorHandler: (client, exception, cancellationToken) => HandleErrorAsync(client, exception, cancellationToken),
     receiverOptions: receiverOptions,
     cancellationToken: cts.Token
 );

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            cts.Cancel();
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient<BookApiClient>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri("https://localhost:7155");
                            });
                    services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient(Constants.Token));
                    services.AddTransient<TelegramBookBot>();
                    services.AddLogging();
                });

    }


}
