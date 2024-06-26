﻿using CURSOVA;
using CURSOVA.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Telegram.Bot.Types;

namespace CURSOVA.DataBase
{
    public class Data
    {
        NpgsqlConnection con=new NpgsqlConnection(Constants.ConnectionString);
        
        public async Task InsertBooks(Book book, string tableName)
        {
            var sql = $"insert into public.\"{tableName}\"(\"user_id\", \"title\", \"authors\", \"book_review\") "
                + $"values (@user_id, @title, @authors, @book_review)";

            NpgsqlCommand comm = new NpgsqlCommand(sql,con);

            comm.Parameters.AddWithValue("title", book.Name);
            comm.Parameters.AddWithValue("authors", string.Join(", ", book.Authors.Select(a => a.Trim('\"', '{', '}'))));
            comm.Parameters.AddWithValue("user_id", book.UserId);
            comm.Parameters.AddWithValue("book_review", book.BookReview);
            
            await con.OpenAsync();
            await comm.ExecuteNonQueryAsync(); 
            await con.CloseAsync();

        }

        public async Task<List<Book>> SelectBooks(long userId, string tableName)
        {
            var sql = $"SELECT \"title\", \"authors\", \"book_review\" FROM public.\"{tableName}\" WHERE \"user_id\" = @user_id";
            List<Book> books = new List<Book>();

            NpgsqlCommand comm = new NpgsqlCommand(sql, con);
            comm.Parameters.AddWithValue("user_id", userId);

            await con.OpenAsync();

            using (var reader = await comm.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var authors = reader.GetString(1).Split(',').Select(a => a.Trim('\"', '{', '}')).ToList();
                    books.Add(new Book
                    {
                        Name = reader.GetString(0),
                        Authors = authors,
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
