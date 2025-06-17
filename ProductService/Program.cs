using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Repositories;
using ProductService.Application.Services;

namespace ProductService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ProductContext>(x => x.UseInMemoryDatabase("ProductDb"));

            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddScoped<IBookService, BookService>();

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
                // Wymuszenie utworzenia bazy i za³adowania danych (InMemory robi to przy pierwszym dostêpie)
                context.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}
