using DinkToPdf.Contracts;
using DinkToPdf;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add converter to DI
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

// Add services to the container.

var app = builder.Build();

app.MapControllers();
app.Run();