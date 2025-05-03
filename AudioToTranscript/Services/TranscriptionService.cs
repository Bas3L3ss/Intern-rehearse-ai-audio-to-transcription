using System.Diagnostics;
using System.Runtime.CompilerServices;
using AudioToTranscript.Configuration;
using AudioToTranscript.Models;
using AudioToTranscript.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AudioToTranscript.Services;

/// <summary>
/// Implementation of the transcription service using Whisper AI
/// </summary>
public class TranscriptionService : ITranscriptionService
{
    private readonly ILogger<TranscriptionService> _logger;
    private readonly TranscriptionSettings _settings;
    private readonly IWebHostEnvironment _environment;

    public TranscriptionService(
        ILogger<TranscriptionService> logger,
        IOptions<TranscriptionSettings> settings,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _settings = settings.Value;
        _environment = environment;
    }

    /// <inheritdoc />
    public async Task<TranscriptionResult> TranscribeAudioAsync(TranscriptionRequest request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Create a unique file name for the uploaded audio
            var fileName = $"{Guid.NewGuid()}_{request.AudioFile.FileName}";
            // var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            var tempPath = Path.Combine(_environment.ContentRootPath, "TempFiles", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath) ?? throw new InvalidOperationException("Failed to create directory for temp files."));
            
            _logger.LogInformation("Saving uploaded file to {FilePath}", tempPath);
            
            // Save the uploaded file to disk
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await request.AudioFile.CopyToAsync(stream, cancellationToken);
            }
            
            // Build additional arguments if needed
            var additionalArgs = BuildWhisperArguments(request);

            // Call the Whisper Python script
            var transcription = TranscribeWithWhisper(tempPath, additionalArgs, cancellationToken);
            
            stopwatch.Stop();
            
            // Parse duration from the filename - if available
            double durationInSeconds = 0;
            try
            {
                var fileInfo = new FileInfo(tempPath);
                durationInSeconds = fileInfo.Length / (16000.0 * 2); // Rough estimate for 16kHz 16-bit audio
            }
            catch
            {
                // Ignore errors in duration estimation
            }
            
            // Clean up the temporary file
            try
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file {FilePath}", tempPath);
            }
            
            return new TranscriptionResult
            {
                Transcription = transcription.Trim(),
                DurationInSeconds = durationInSeconds,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Model = request.UseHighAccuracyModel ? "whisper-large" : "whisper-base",
                Success = true
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error transcribing audio file");
            
            return new TranscriptionResult
            {
                Transcription = string.Empty,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Success = false,
                ErrorMessage = $"Transcription failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Calls the Python script to transcribe the audio
    /// </summary>
    private string TranscribeWithWhisper(string audioPath, string additionalArgs = "", CancellationToken cancellationToken = default)
    {

        // Find the script path relative to the application
        var projectRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, ".."));
        _logger.LogInformation("Project root is {ProjectRoot}", projectRoot);
        var scriptPath = Path.Combine(projectRoot, "transcribe.py");

        
        if (!File.Exists(scriptPath))
        {
            _logger.LogError("Transcription script not found at: {ScriptPath}", scriptPath);
            throw new FileNotFoundException("Transcription script not found", scriptPath);
        }
        
        var pythonExecutable = _settings.PythonPath ?? "python3";
        
        var arguments = $"\"{scriptPath}\" \"{audioPath}\" {additionalArgs}";
        _logger.LogInformation("Executing: {Python} {Arguments}", pythonExecutable, arguments);
        
        var psi = new ProcessStartInfo
        {
            FileName = pythonExecutable,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = psi };

        process.Start();
        
        cancellationToken.Register(() => {
            if (!process.HasExited) 
                if (File.Exists(audioPath)){

                    File.Delete(audioPath);
                };
                process.Kill(entireProcessTree: true);
            }
            
        );
            


        // Read both standard output and error asynchronously
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        process.WaitForExit();
        
        if (process.ExitCode != 0)
        {
            _logger.LogError("Whisper transcription failed. Exit code: {ExitCode}. Error: {Error}", 
                process.ExitCode, error);
            throw new Exception($"Whisper transcription failed with exit code {process.ExitCode}: {error}");
        }
        
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Whisper transcription warning: {Warning}", error);
        }
        
        return output;
    }

    /// <summary>
    /// Builds command-line arguments for the Whisper script based on the request options
    /// </summary>
    private static string BuildWhisperArguments(TranscriptionRequest request)
    {
        var args = new List<string>();
        
        // Add language if specified
        if (!string.IsNullOrEmpty(request.Language))
        {
            args.Add($"--language {request.Language}");
        }
        // Add model option
        if (request.UseHighAccuracyModel)
        {
            args.Add("--model large");
        }
        else
        {
            args.Add("--model base");
        }
        
        return string.Join(" ", args);
    }
    public async IAsyncEnumerable<string> TranscribeAudioStreamAsync(
            TranscriptionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // 1) Save to temp path
            var fileName = $"{Guid.NewGuid()}_{request.AudioFile.FileName}";
            var tempDir = Path.Combine(_environment.ContentRootPath, "TempFiles");
            Directory.CreateDirectory(tempDir);
            var tempPath = Path.Combine(tempDir, fileName);

            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await request.AudioFile.CopyToAsync(fs, cancellationToken);
            }

            // 2) Build whisper args & locate script
            var additionalArgs = BuildWhisperArguments(request);
            var projectRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, ".."));
            var scriptPath  = Path.Combine(projectRoot, "transcribe.py");
            var pythonExe   = _settings.PythonPath ?? "python3";
            var args        = $"\"{scriptPath}\" \"{tempPath}\" {additionalArgs}";

            // 3) Prepare and start process
            var psi = new ProcessStartInfo
            {
                FileName              = pythonExe,
                Arguments             = args,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute       = false,
                CreateNoWindow        = true
            };

            using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            process.Start();

            // 4) Read stdout as a stream
            //    We yield each line + newline immediately
            try
            {
                // Asynchronously read each line until the script ends
                while (!process.HasExited || !process.StandardOutput.EndOfStream)
                {
                    var line = await process.StandardOutput.ReadLineAsync();
                    if (line == null) 
                        break;

                    yield return line + "\n";

                    // Respect cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        try { process.Kill(entireProcessTree: true); }
                        catch { /* ignore */ }
                        yield break;
                    }
                }

                await process.WaitForExitAsync(cancellationToken);

                // If the script wrote to stderr, you might choose to forward it too:
                var stderr = await process.StandardError.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    _logger.LogWarning("Whisper stderr: {Stderr}", stderr);
                }
            }
            finally
            {
                // 5) Cleanup temp file
                try { File.Delete(tempPath); } catch { /* swallow */ }
            }
        }

}