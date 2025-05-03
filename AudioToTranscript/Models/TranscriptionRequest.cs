namespace AudioToTranscript.Models;

/// <summary>
/// Represents a request to transcribe an audio file
/// </summary>
public class TranscriptionRequest
{
    /// <summary>
    /// The audio file to transcribe
    /// </summary>
    public IFormFile AudioFile { get; set; } = null!;

    /// <summary>
    /// Optional language code for the audio (e.g., "en", "fr", "es")
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Whether to use a more accurate but slower model
    /// </summary>
    public bool UseHighAccuracyModel { get; set; } = false;
}