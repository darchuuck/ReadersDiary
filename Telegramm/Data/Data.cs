using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegramm.models;

namespace Telegramm.Data
{
    public class Data
    {
        NpgsqlConnection con = new NpgsqlConnection(Constants.ConnectionString);

        public async Task InsertBooks(BookTel book, string tableName)
        {
            var sql = $"insert into public.\"{tableName}\"(\"user_id\", \"title\", \"authors\", \"book_review\") "
                + $"values (@user_id, @title, @authors, @book_review)";

            NpgsqlCommand comm = new NpgsqlCommand(sql, con);

            comm.Parameters.AddWithValue("title", book.Name);
            comm.Parameters.AddWithValue("authors", book.Authors);
            comm.Parameters.AddWithValue("user_id", book.UserId);
            if (book.BookReview != null)
            {
                comm.Parameters.AddWithValue("book_review", book.BookReview);
            }
            else
            {
                comm.Parameters.AddWithValue("book_review", DBNull.Value);
            }
            await con.OpenAsync();
            await comm.ExecuteNonQueryAsync();
            await con.CloseAsync();

        }

        public async Task<List<BookTel>> SelectBooks(long userId, string tableName)
        {
            var sql = $"SELECT \"title\", \"authors\", \"book_review\" FROM public.\"{tableName}\" WHERE \"user_id\" = @user_id";
            List<BookTel> books = new List<BookTel>();

            NpgsqlCommand comm = new NpgsqlCommand(sql, con);
            comm.Parameters.AddWithValue("user_id", userId);

            await con.OpenAsync();

            using (var reader = await comm.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    books.Add(new BookTel
                    {
                        Name = reader.GetString(0),
                        Authors = reader.GetString(1).Split(',').ToList(),
                        BookReview = reader.IsDBNull(2) ? null : reader.GetString(2),
                        UserId = userId
                    });
                }
            }

            await con.CloseAsync();

            return books;
        }
        public async Task DeleteBook(string title, long userId, string tableName)
        {
            var sql = $"DELETE FROM public.\"{tableName}\" WHERE \"title\" = @title AND \"user_id\" = @user_id";

            using (NpgsqlCommand comm = new NpgsqlCommand(sql, con))
            {
                comm.Parameters.AddWithValue("title", title);
                comm.Parameters.AddWithValue("user_id", userId);

                await con.OpenAsync();
                await comm.ExecuteNonQueryAsync();
                await con.CloseAsync();
            }
        }
    }
}
