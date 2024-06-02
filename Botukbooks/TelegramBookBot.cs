using Botukbooks.Client;
using Botukbooks.Model;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Args;
using Microsoft.Extensions.Logging;
using static System.Net.WebRequestMethods;


namespace Botukbooks
{
    public class TelegramBookBot
    {
        private readonly ITelegramBotClient _botClient;
        private readonly BookApiClient _bookClient;
        private readonly ILogger<TelegramBookBot> _logger;
        private Dictionary<long, (string state, string tableName)> _userStates = new Dictionary<long, (string, string)>();
        private readonly string[] _genres;

        public TelegramBookBot(ITelegramBotClient botClient, BookApiClient bookClient, ILogger<TelegramBookBot> logger)
        {
            _botClient = botClient;
            _bookClient = bookClient;
            _logger = logger;
            _genres = LoadGenresFromFile("C:\\Users\\dasha\\source\\repos\\CURSOVA\\Botukbooks\\Genres.txt");
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            if (message?.Text != null && !string.IsNullOrEmpty(message.Text))
            {
                _logger.LogInformation($"{message.From.Username}: {message.Text}");
                long userId = message.Chat.Id;

                var commandHandlers = new Dictionary<string, Func<Task>>
                {
                    ["/start"] = async () => await HandleStartCommand(message),
                    ["/helpwithgenres"] = async () => await HandleGenreHelpCommand(message),
                    ["/iwantarandombook"] = async () => await HandleRandomBookCommand(message),
                    ["/findabook"] = async () => await HandleFindBookCommand(message),
                    ["/addareadbook"] = async () => await HandleAddBookCommand(message, "read__books"),
                    ["/addafavoritebook"] = async () => await HandleAddBookCommand(message, "favorite__books"),
                    ["/myreadbooks"] = async () => await HandleGetBooksCommand(message, "read__books"),
                    ["/favoritebooks"] = async () => await HandleGetBooksCommand(message, "favorite__books"),
                    ["/removefromfavorites"] = async () => await HandleDeleteFavoriteBookCommand(message)
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
                            await HandleAwaitingGenreState(message);
                            break;
                        case "awaiting_title":
                            await HandleAwaitingTitleState(message);
                            break;
                        case "awaiting_book":
                            await HandleAwaitingBookState(message);
                            break;
                        case "awaiting_book_to_delete":
                            await HandleAwaitingBookToDeleteState(message);
                            break;
                        default:
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, I can't understand and respond to your message(( ");
                            break;
                    }
                }
                else
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Sorry, I can't understand and respond to your message(( ");
                }
            }
        }

        private async Task HandleStartCommand(Message message)
        {
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Hello! Welcome to the reader bot. You can choose the functions you need in the commands and please pay attention to the instructions I am attaching)", replyMarkup: GetCommandsKeyboard());
        }

        private async Task HandleGenreHelpCommand(Message message)
        {
            string genreList = "Possible book genres:\n" + string.Join("\n", _genres);
            genreList += "\n\nYou can find more genres at: https://www.goodreads.com/genres/list";
            await _botClient.SendTextMessageAsync(message.Chat.Id, genreList);
        }

        private async Task HandleRandomBookCommand(Message message)
        {
            long userId = message.Chat.Id;
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Enter in English the topic or genre of the book you are looking for :");
            _userStates[userId] = ("awaiting_genre", null);
        }

        private async Task HandleFindBookCommand(Message message)
        {
            long userId = message.Chat.Id;
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Enter the full name in English of the book you are looking for, or a word contained in it: ");
            _userStates[userId] = ("awaiting_title", null);
        }

        private async Task HandleAddBookCommand(Message message, string tableName)
        {
            long userId = message.Chat.Id;
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Enter the book in the format name~author~review. If there are several authors, write them separated by a comma");
            _userStates[userId] = ("awaiting_book", tableName);
        }

        private async Task HandleGetBooksCommand(Message message, string tableName)
        {
            long userId = message.Chat.Id;

            var books = await _bookClient.GetUserBooksAsync(userId, tableName);

            if (books.Count == 0)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "You have no added books");
            }
            else
            {
                var response = "Here are your books:\n";
                response += string.Join("\n", books.Select(b => $"{b.Name} - {string.Join(", ", b.Authors)}\n{b.BookReview}"));
                await _botClient.SendTextMessageAsync(message.Chat.Id, response);
            }
        }

        private async Task HandleDeleteFavoriteBookCommand(Message message)
        {
            long userId = message.Chat.Id;
            await _botClient.SendTextMessageAsync(message.Chat.Id, "Enter the full name of the book you want to remove from favorites: ");
            _userStates[userId] = ("awaiting_book_to_delete", "favorite__books");
        }

        private async Task HandleAwaitingGenreState(Message message)
        {
            long userId = message.Chat.Id;
            string bookGenre = message.Text.Trim();
            _userStates.Remove(userId);

            try
            {
                var randomBook = await _bookClient.GetRandomBookByGenreAsync(bookGenre);

                if (randomBook != null)
                {
                    var response = $"Your random book: {randomBook.Name}\nYou can find more information about this book here: {randomBook.Url}\n";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    var errorMessage = "Sorry, I can't find any books with that genre";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
                }
            }
            catch (Exception ex) when (ex is BookNotFoundException || ex is GenreNotFoundException)
            {
                var errorMessage = "Sorry, I can't find any books with that genre";
                await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while sending the message: {ex.Message}");
                var errorMessage = "Please repeat your request later";
                await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }
        }

        private async Task HandleAwaitingTitleState(Message message)
        {
            long userId = message.Chat.Id;
            string bookTitle = message.Text.Trim();
            _userStates.Remove(userId);
            try
            {
                var books = await _bookClient.SearchBooksAsync(bookTitle);
                if (books != null && books.Count > 0)
                {
                    var response = "Here is a list of books that match your query:\n";
                    foreach (var book in books)
                    {
                        var authors = book.Authors != null && book.Authors.Count > 0
                            ? string.Join(", ", book.Authors)
                            : "Authors not specified";

                        response += $"\nName: {book.Name}\nAuthors: {authors}\nLink:{book.Url} \n";
                    }
                    await _botClient.SendTextMessageAsync(message.Chat.Id, response);
                }
                else
                {
                    var errorMessage = "Sorry, I can't find any books with that title. Please check the accuracy of the title.";
                    await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while sending the message: {ex.Message}");
                var errorMessage = "Please repeat your request later";
                await _botClient.SendTextMessageAsync(message.Chat.Id, errorMessage);
            }
        }

        private async Task HandleAwaitingBookState(Message message)
        {
            long userId = message.Chat.Id;
           
            string userMessage = message.Text;

            if (!_userStates.TryGetValue(userId, out var state) || state.state != "awaiting_book")
            {
                return;
            }

            string tableName = state.tableName;
            string[] parts = userMessage.Split(new[] { '~' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
            {
                await _botClient.SendTextMessageAsync(message.Chat.Id, "Please provide the book title, author(s), and review separated by '~'");
                return;
            }

            string title = parts[0].Trim();
            string authors = parts[1].Trim();
            string bookReview = parts[2].Trim();

            var book = new BookTel
            {
                Name = title,
                Authors = authors.Split(',').Select(a => a.Trim()).ToList(),
                BookReview = bookReview,
                UserId = userId,
                Url= "https://www.goodreads.com"
            };

            var result = await _bookClient.AddBookAsync(book, tableName);

            await _botClient.SendTextMessageAsync(message.Chat.Id, result);

            _userStates.Remove(userId);
        }

        private async Task HandleAwaitingBookToDeleteState(Message message)
        {
            long userId = message.Chat.Id;
            string bookTitle = message.Text.Trim();
            var state = _userStates[userId];

            try
            {
                var favoriteBooks = await _bookClient.GetUserBooksAsync(userId, "favorite__books");
                if (favoriteBooks.Any(book => book.Name.Equals(bookTitle, StringComparison.OrdinalIgnoreCase)))
                {

                    await _bookClient.DeleteBookAsync(bookTitle, userId, "favorite__books");
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"The book '{bookTitle}' has been successfully removed from favorites");
                }
                else
                {
                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"The book '{bookTitle}' was not found among the favorite books");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting the book: {ex.Message}");
                await _botClient.SendTextMessageAsync(message.Chat.Id, "An error occurred while deleting the book. Please try again later.");
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
                _logger.LogError($"Failed to load genres from file: {ex.Message}");
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
        public class GenreNotFoundException : Exception
        {
            public GenreNotFoundException(string message, Exception innerException = null)
                : base(message, innerException)
            {
            }
        }

        public class BookNotFoundException : Exception
        {
            public BookNotFoundException(string message, Exception innerException = null)
                : base(message, innerException)
            {
            }
        }
    }

}

