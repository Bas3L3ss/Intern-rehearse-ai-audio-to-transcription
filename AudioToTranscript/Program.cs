using AudioToTranscript.Configuration;
using AudioToTranscript.Middleware;
using AudioToTranscript.Services;
using AudioToTranscript.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Default Vite dev server port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure settings from appsettings.json
builder.Services.Configure<TranscriptionSettings>(
    builder.Configuration.GetSection("TranscriptionSettings"));

// Register services
builder.Services.AddScoped<ITranscriptionService, TranscriptionService>();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Add global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use CORS
app.UseCors("ReactAppPolicy");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();