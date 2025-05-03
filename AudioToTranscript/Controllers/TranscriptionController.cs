using Microsoft.AspNetCore.Mvc;
using AudioToTranscript.Models;
using AudioToTranscript.Services.Interfaces;

namespace AudioToTranscript.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranscriptionController : ControllerBase
{
    private readonly ITranscriptionService _transcriptionService;
    private readonly ILogger<TranscriptionController> _logger;

    public TranscriptionController(
        ITranscriptionService transcriptionService,
        ILogger<TranscriptionController> logger)
    {
        _transcriptionService = transcriptionService;
        _logger = logger;
    }

   [HttpPost("upload")]
    public async Task<IActionResult> UploadAndTranscribe([FromForm] IFormFile file, [FromForm] string? language, [FromForm] string? useHighAccuracyModel,CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        _logger.LogInformation("Received audio file: {FileName}, Size: {FileSize} bytes, Language: {Language}", 
            file.FileName, file.Length, language ?? "not specified");

        var request = new TranscriptionRequest
        {
            AudioFile = file,
            Language = language,
            UseHighAccuracyModel = useHighAccuracyModel == "true"
        };

        var result = await _transcriptionService.TranscribeAudioAsync(request, cancellationToken);

        return Ok(result);
    }   
    [HttpPost("upload-stream")]
    public async Task UploadAndTranscribeStream(
        [FromForm] IFormFile file,
        [FromForm] string? language,
        [FromForm] string? useHighAccuracyModel,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            await Response.WriteAsync("No file uploaded.", cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Received audio file (stream): {FileName}, Size: {FileSize} bytes, Language: {Language}",
            file.FileName, file.Length, language ?? "not specified");

        var request = new TranscriptionRequest
        {
            AudioFile = file,
            Language = language,
            UseHighAccuracyModel = useHighAccuracyModel == "true"
        };

        Response.ContentType = "text/plain; charset=utf-8";
        // ASP .NET Core will automatically use chunked Transfer-Encoding
        // as long as you don't set a Content-Length.

        // Assume TranscribeAudioStreamAsync yields each segment as soon as it's ready.
        await foreach (var segment in _transcriptionService
                               .TranscribeAudioStreamAsync(request, cancellationToken)
                               .WithCancellation(cancellationToken))
        {
            // write the raw text (could include timestamps, newlines, etc.)
            await Response.WriteAsync(segment, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        // no explicit return; the stream ends here
    }
 
}