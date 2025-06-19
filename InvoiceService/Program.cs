
using InvoiceService.Application.Services;
using InvoiceService.Domain.Repositories;
using InvoiceService.Kafka;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Database context
            builder.Services.AddDbContext<InvoiceDataContext>(x => x.UseInMemoryDatabase("CartDatabase"), ServiceLifetime.Transient);
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

            // Add services to the container.
            builder.Services.AddScoped<IInvoiceService, InvoiceService.Application.Services.InvoiceService>();

            //Kafka for User (consumer)
            try
            {
                builder.Services.AddHostedService<KafkaCartEventsConsumer>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nie uda³o siê dodaæ KafkaCartEventsConsumer: {ex.Message}");
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
}
