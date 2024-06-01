


using TelegramBook.Clients;
using TelegramBook.DataBase;


namespace TelegramBook
{
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