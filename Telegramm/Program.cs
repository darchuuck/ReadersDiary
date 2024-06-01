
using Telegramm.Clients;
using Telegramm;

namespace Telegramm
{
    class Program
    {
        static readonly TelegramBookClient _client = new TelegramBookClient(Constants.Address);

        public static TelegramBookClient Client => _client;

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var telegramClient = new ProgramForTelegram(Client);
            await telegramClient.Start(Constants.Token);
        }
    }
}
