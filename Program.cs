using BackEndGamesTito.API.Repositories;
using BackEndGamesTito.API.Services; //novo

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<UsuarioRepository>();

builder.Services.AddScoped<EmailService>(); //novo
builder.Services.AddScoped<TelegramService>(); //novo

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



builder.Services.AddScoped<BackEndGamesTito.API.Services.EmailService>();