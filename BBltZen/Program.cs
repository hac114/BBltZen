
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.Interface;
using Repository.Service;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BBltZen
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<Database.BubbleTeaContext>();
            builder.Services.AddScoped<IOrdineRepository, OrdineRepository>();
            builder.Services.AddScoped<IDolceRepository, DolceRepository>();

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
