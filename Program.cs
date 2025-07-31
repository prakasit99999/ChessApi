
using ChessApi.Data;
using ChessApi.DbContext;
using ChessApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChessApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("ChessApiContextConnection") ?? throw new InvalidOperationException("Connection string 'ChessApiContextConnection' not found.");
            // Add services to the container.
            builder.Services.AddDbContext<ChessApiContext>(options => options.UseSqlite(connectionString));
            builder.Services.AddDefaultIdentity<ChessDbContext>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChessApiContext>();
            builder.Services.AddScoped<IPasswordHasher<user>, PasswordHasher<user>>();


            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
