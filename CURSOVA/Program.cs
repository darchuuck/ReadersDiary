//using CURSOVA.Clients;
//using CURSOVA.Model;


//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

////builder.Services.AddSingleton<GoogleBooksClient>();
////builder.Services.AddSingleton<SearchBookClient>();

//var app = builder.Build();
//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using CURSOVA;
using CURSOVA.Clients;
using ReadingDiary;
using ReadingDiary.DataBase;

using System;
using System.Reflection.Metadata;

namespace APIprot
{
    class Program
    {
        static readonly Data _data = new Data();
        static readonly SearchBookClient _client = new SearchBookClient(_data);

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var telegramClient = new ProgramForTelegram(_client);
            await telegramClient.Start(Constants.Token);
        }
    }
}