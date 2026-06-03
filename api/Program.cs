var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => "Hello World!");

app.Run();
