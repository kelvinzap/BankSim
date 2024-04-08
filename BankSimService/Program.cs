using BankSimService.Actions;
using BankSimService.Models.Database;
using BankSimService.Models.Internals;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
Utilities.Initialize(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers(o => o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter()));
builder.Services.AddMvc(o => { o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter()); });
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//}
    app.UseSwagger();
    app.UseSwaggerUI();


using (var client = new BankSimDbContext())
{
    //client.Database.EnsureCreated(); //No migration | No update
    await client.Database.MigrateAsync(); //Uses Migration, therefore it updates
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
