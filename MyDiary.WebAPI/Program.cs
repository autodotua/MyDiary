using Microsoft.AspNetCore.Http.Json;
using MyDiary.Managers.Services;
using MyDiary.Models.Converters;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DocumentManager>();
builder.Services.AddScoped<BinaryManager>();
builder.Services.AddScoped<TagManager>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "cors",
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5000");
                      });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("cors");

app.UseAuthorization();

app.MapControllers();

app.Run();
