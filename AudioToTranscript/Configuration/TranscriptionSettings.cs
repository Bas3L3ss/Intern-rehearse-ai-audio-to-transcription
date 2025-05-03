namespace AudioToTranscript.Configuration;

/// <summary>
/// Settings for the audio transcription service
/// </summary>
public class TranscriptionSettings
{
    /// <summary>
    /// Path to the Python executable
    /// </summary>
    public string? PythonPath { get; set; }
    
    /// <summary>
    /// Maximum file size for uploaded audio in MB
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 25;
    
    /// <summary>
    /// Timeout for transcription process in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
    
    /// <summary>
    /// Allowed audio file extensions
    /// </summary>
    public string[] AllowedFileExtensions { get; set; } = { ".mp3", ".wav", ".m4a", ".mp4", ".mpeg", ".mpga", ".webm" };
}