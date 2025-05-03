export interface TranscriptionOptions {
  language: string;
  useHighAccuracyModel: boolean;
}

export interface TranscriptionResult {
  transcription: string;
  durationInSeconds: number;
  processingTimeMs: number;
  model: string;
  success: boolean;
  errorMessage?: string;
}
