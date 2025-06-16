
using CartService.Application.Services;
using CartService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using CartService.Kafka;

namespace CartService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //Database context
        builder.Services.AddDbContext<CartDataContext>(x => x.UseInMemoryDatabase("CartDatabase"), ServiceLifetime.Transient);
        builder.Services.AddScoped<ICartRepository, CartRepository>();

        // Add services to the container.
        builder.Services.AddScoped<ICartService, CartService.Application.Services.CartService>();
        
        //Kafka
        try
        {
            builder.Services.AddHostedService<KafkaUserEventsConsumer>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Nie uda³o siê dodaæ KafkaUserEventsConsumer: {ex.Message}");
        }


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
