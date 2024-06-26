
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBook.Clients;
using TelegramBook.models;
using TelegramBook.DataBase;

namespace TelegramBook
{
    public class ProgramForTelegram

    {
        private readonly TelegramBookClient _client;
        private Dictionary<long, (string state, string tableName)> _userStates = new Dictionary<long, (string, string)>();
        private readonly string[] _genres;

        public ProgramForTelegram(TelegramBookClient client)
        {
            _client = client;
            _genres = LoadGenresFromFile("C:\\Users\\dasha\\source\\repos\\CURSOVA\\TelefgramBooksBot\\Genres.txt");
        }

        public async Task Start(string token)
        {
            var botClient = new TelegramBotClient(token);
            botClient.StartReceiving(Update, Error);
            await Task.Delay(Timeout.Infinite);
        }

        private async Task Update(ITelegramBotClient botclient, Update update, CancellationToken token)
        {
            var message = update.Message;
            if (message?.Text != null && !string.IsNullOrEmpty(message.Text))
            {
                Console.WriteLine($"{message.From.Username}: {message.Text}");
                long userId = message.Chat.Id;

                var commandHandlers = new Dictionary<string, Func<Task>>
                {
                    ["/start"] = async () => await HandleStartCommand(botclient, message),
                    ["/helpwithgenres"] = async () => await HandleGenreHelpCommand(botclient, message),
                    ["/iwantarandombook"] = async () => await HandleRandomBookCommand(botclient, message),
                    ["/findabook"] = async () => await HandleFindBookCommand(botclient, message),
                    ["/addareadbook"] = async () => await HandleAddBookCommand(botclient, message, "read__books"),
                    ["/addafavoritebook"] = async () => await HandleAddBookCommand(botclient, message, "favorite__books"),
                    ["/myreadbooks"] = async () => await HandleGetBooksCommand(botclient, message, "read__books"),
                    ["/favoritebooks"] = async () => await HandleGetBooksCommand(botclient, message, "favorite__books"),
                    ["/removefromfavorites"] = async () => await HandleDeleteFavoriteBookCommand(botclient, message)
                };

                foreach (var command in commandHandlers.Keys)
                {
                    if (message.Text.ToLower().Contains(command))
                    {
                        await commandHandlers[command]();
                        return;
                    }
                }

                if (_userStates.TryGetValue(userId, out var state))
                {
                    switch (state.state)
                    {
                        case "awaiting_genre":
                            await HandleAwaitingGenreState(botclient, message);
                            break;
                        case "awaiting_title":
                            await HandleAwaitingTitleState(botclient, message);
                            break;
                        case "awaiting_book":
                            await HandleAwaitingBookState(botclient, message);
                            break;
                        case "awaiting_book_to_delete":
                            await HandleAwaitingBookToDeleteState(botclient, message);
                            break;
                        default:
                            await botclient.SendTextMessageAsync(message.Chat.Id, "Sorry, I can't understand and respond to your message(( ");
                            break;
                    }
                }
                else
                {
                    await botclient.SendTextMessageAsync(message.Chat.Id, "Sorry, I can't understand and respond to your message(( ");
                }
            }
        }

        private async Task HandleStartCommand(ITelegramBotClient botclient, Message message)
        {
            await botclient.SendTextMessageAsync(message.Chat.Id, "Hello! Here are the available commands:", replyMarkup: GetCommandsKeyboard());
        }

        private async Task HandleGenreHelpCommand(ITelegramBotClient botclient, Message message)
        {
            string genreList = "Possible book genres:\n" + string.Join("\n", _genres);
            genreList += "\n\nYou can find more genres at: https://www.goodreads.com/genres/list";
            await botclient.SendTextMessageAsync(message.Chat.Id, genreList);
        }

        private async Task HandleRandomBookCommand(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            await botclient.SendTextMessageAsync(message.Chat.Id, "Enter the topic or genre of the book you are looking for:");
            _userStates[userId] = ("awaiting_genre", null);
        }

        private async Task HandleFindBookCommand(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            await botclient.SendTextMessageAsync(message.Chat.Id, "Enter the full name of the book you are looking for, or a word contained in it: ");
            _userStates[userId] = ("awaiting_title", null);
        }

        private async Task HandleAddBookCommand(ITelegramBotClient botclient, Message message, string tableName)
        {
            long userId = message.Chat.Id;
            await botclient.SendTextMessageAsync(message.Chat.Id, "Enter the book in the format name~author~review. If there are several authors, write them separated by a tilda; the review is optional.");
            _userStates[userId] = ("awaiting_book", tableName);
        }

        private async Task HandleGetBooksCommand(ITelegramBotClient botclient, Message message, string tableName)
        {
            long userId = message.Chat.Id;

            var client = new TelegramBookClient(new Data());
            var books = await client.GetBooks(userId, tableName);

            if (books.Count == 0)
            {
                await botclient.SendTextMessageAsync(message.Chat.Id, "You have no added books");
            }
            else
            {
                var response = "Here are your books:\n";
                response += string.Join("\n", books.Select(b => $"{b.Name} - {string.Join(", ", b.Authors)}\n{b.BookReview ?? "No review"}"));
                await botclient.SendTextMessageAsync(message.Chat.Id, response);
            }
        }

        private async Task HandleDeleteFavoriteBookCommand(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            await botclient.SendTextMessageAsync(message.Chat.Id, "Enter the full name of the book you want to remove from favorites: ");
            _userStates[userId] = ("awaiting_book_to_delete", "favorite__books");
        }



        private async Task HandleAwaitingGenreState(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            string bookGenre = message.Text.Trim();
            _userStates.Remove(userId);
            try
            {
                var books = await _client.GetBooksByGenre(bookGenre);
                if (books != null && books.Count > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(books.Count);
                    SearchBookRandomTel randomBook = books[randomIndex];
                    var response = $"Your random book: {randomBook.Name}\nYou can find more information about this book here: {randomBook.Url}\n";
                    await botclient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    var errorMessage = "Sorry, I can't find any books with that genre";
                    await botclient.SendTextMessageAsync(message.Chat.Id, errorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the message: {ex.Message}");
                var errorMessage = "Please repeat your request later";
                await botclient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }
        }
        private async Task HandleAwaitingTitleState(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            string bookTitle = message.Text.Trim();
            _userStates.Remove(userId);
            try
            {
                var books = await _client.GetBookByTitle(bookTitle);
                if (books != null && books.Count > 0)
                {
                    var response = "Here is a list of books that match your query:\n";
                    foreach (var book in books)
                    {
                        var authors = book.Authors != null && book.Authors.Count > 0
                            ? string.Join(", ", book.Authors)
                            : "Authors not specified";

                        response += $"\nName: {book.Name}\nAuthors: {authors}\nLink: {book.Url}\n";
                    }
                    await botclient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    var errorMessage = "Sorry, I can't find any books with that title. Please check the accuracy of the title.";
                    await botclient.SendTextMessageAsync(message.Chat.Id, errorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the message: {ex.Message}");
                var errorMessage = "Please repeat your request later";
                await botclient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }
        }

        private async Task HandleAwaitingBookState(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            string userMessage = message.Text;

            if (!_userStates.TryGetValue(userId, out var state) || state.state != "awaiting_book")
            {
                return;
            }

            string tableName = state.tableName;
            string[] parts = userMessage.Split(new[] { '~' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                await botclient.SendTextMessageAsync(message.Chat.Id, "Please provide both the book title and author(s) separated by '-'");
                return;
            }

            string title = parts[0].Trim();
            string authors = parts[1].Trim();
            string bookReview = parts.Length == 3 ? parts[2].Trim() : null; // Check if a review was provided
            Console.WriteLine($"Title: {title}, Authors: {authors}, Review: {(bookReview == null ? "No review provided" : bookReview)}");
            var book = new BookTel
            {
                Name = title,
                Authors = authors.Split(',').Select(a => a.Trim()).ToList(),
                BookReview = bookReview, // Set the review
                UserId = userId
            };

            await _client.AddBook(book, tableName);

            await botclient.SendTextMessageAsync(message.Chat.Id, "The book has been successfully added!");

            _userStates.Remove(userId);
        }

        private async Task HandleAwaitingBookToDeleteState(ITelegramBotClient botclient, Message message)
        {
            long userId = message.Chat.Id;
            string bookTitle = message.Text.Trim();
            var state = _userStates[userId];

            try
            {
                var favoriteBooks = await _client.GetBooks(userId, "favorite__books");
                if (favoriteBooks.Any(book => book.Name.Equals(bookTitle, StringComparison.OrdinalIgnoreCase)))
                {

                    await _client.DeleteBook(bookTitle, userId, "favorite__books");
                    await botclient.SendTextMessageAsync(message.Chat.Id, $"The book '{bookTitle}' has been successfully removed from favorites");
                }
                else
                {
                    await botclient.SendTextMessageAsync(message.Chat.Id, $"The book '{bookTitle}' was not found among the favorite books");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the book: {ex.Message}");
                await botclient.SendTextMessageAsync(message.Chat.Id, "An error occurred while deleting the book. Please try again later.");
            }

            _userStates.Remove(userId);
        }



        private string[] LoadGenresFromFile(string filePath)
        {
            try
            {
                return System.IO.File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load genres from file: {ex.Message}");
                return new string[] { "Failed to load genres" };
            }
        }

        private IReplyMarkup GetCommandsKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
        new KeyboardButton[] { "/iwantarandombook" },
        new KeyboardButton[] { "/helpwithgenres" },
        new KeyboardButton[] { "/findabook" },
        new KeyboardButton[] { "/addareadbook" },
        new KeyboardButton[] { "/myreadbooks" },
        new KeyboardButton[] { "/favoritebooks" },
        new KeyboardButton[] { "/addafavoritebook" },
        new KeyboardButton[] { "/removefromfavorites" }
    })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }

        private Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}