using System.Text.Json.Serialization;

namespace AudioToTranscript.Models;

/// <summary>
/// Represents the result of an audio transcription
/// </summary>
public class TranscriptionResult
{
    /// <summary>
    /// The transcribed text
    /// </summary>
    public string Transcription { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the audio in seconds
    /// </summary>
    public double DurationInSeconds { get; set; }

    /// <summary>
    /// The duration of the transcription process in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }

    /// <summary>
    /// The model used for transcription
    /// </summary>
    public string Model { get; set; } = "whisper";

    /// <summary>
    /// When true, indicates successful transcription
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message, if any
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }
}