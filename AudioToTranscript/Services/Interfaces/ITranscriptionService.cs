using AudioToTranscript.Models;

namespace AudioToTranscript.Services.Interfaces;

/// <summary>
/// Service for handling audio transcription
/// </summary>
public interface ITranscriptionService
{
    /// <summary>
    /// Transcribes an audio file
    /// </summary>
    /// <param name="request">The transcription request</param>
    /// <returns>The transcription result</returns>
    Task<TranscriptionResult> TranscribeAudioAsync(TranscriptionRequest request, CancellationToken cancellationToken);
    IAsyncEnumerable<string> TranscribeAudioStreamAsync(
        TranscriptionRequest request, CancellationToken ct);
}