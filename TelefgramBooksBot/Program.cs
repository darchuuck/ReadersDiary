using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBooksBot.DataBase;
using TelegramBooksBot.Model;
using TelegramBooksBot.Clients;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using System.Net.Http;

namespace TelegramBooksBot
{

    
        //    static async Task Main(string[] args)                                         можливо це правильно, але не запускається
        //    { 
        //        var serviceProvider = new ServiceCollection()
        //.AddHttpClient() // Реєстрація HttpClient
        //.AddSingleton<Data>()
        //.AddSingleton<TelegramBookClient>(provider =>
        //    new TelegramBookClient(provider.GetRequiredService<IHttpClientFactory>().CreateClient(), Constants.Address))
        //.BuildServiceProvider();


        //        var bookApiClient = serviceProvider.GetRequiredService<TelegramBookClient>();
        //        var botService = new ProgramForTelegram(bookApiClient);
        //        await botService.Start(Constants.Token);

        //        Console.WriteLine("Press any key to exit...");
        //        Console.ReadKey();
        //    }


        class Program
        {
            static readonly Data _data = new Data();
            static readonly TelegramBookClient _client = new TelegramBookClient(_data);

            static async System.Threading.Tasks.Task Main(string[] args)
            {
                var telegramClient = new ProgramForTelegram(_client);
                await telegramClient.Start(Constants.Token);
            }
        }
    
}
