using ClassLibrary.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using WebAPI.Business;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB CONTEXT
builder.Services.AddDbContext<ValaisContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ValaisDB")));

// BUSINESS LAYER
builder.Services.AddScoped<IValaisBusiness, ValaisBusiness>();

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

