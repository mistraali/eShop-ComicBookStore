using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Repositories;
using ProductService.Application.Services;
using ProductService.Application.Kafka.Consumers;
using ProductService.Application.Kafka;
using ProductService.Infrastructure.Kafka;
using Microsoft.Extensions.Options;
using ProductService.Application.Kafka.Producers;

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
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();

            // Kafka configuration      
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
            builder.Services.AddHostedService<ProductConsumerHostedService>();
            builder.Services.AddSingleton<CheckIfProductExistsConsumer>();
            builder.Services.AddSingleton<ProductKafkaProducer>(sp =>
            {
                var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
                return new ProductKafkaProducer(kafkaSettings.BootstrapServers);
            });
      


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
