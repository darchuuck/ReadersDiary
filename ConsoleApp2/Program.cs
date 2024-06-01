using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public class Bot
{
    private readonly HttpClient _httpClient;

    public Bot()
    {
        _httpClient = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    public async Task SearchBooks(string title)
    {
        var response = await _httpClient.GetAsync($"https://localhost:7155/Book/search?name={title}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content); // обробляйте отримані дані, наприклад, виводьте список знайдених книг
        }
        else
        {
            Console.WriteLine($"Failed to search books. Status code: {response.StatusCode}");
        }
    }

    // Додайте інші методи, які будуть взаємодіяти з іншими ендпоінтами вашого API

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var bot = new Bot();

        Console.WriteLine("Enter book title to search:");
        var title = Console.ReadLine();

        await bot.SearchBooks(title);

        // Додайте інші методи бота тут

        bot.Dispose();
    }
}