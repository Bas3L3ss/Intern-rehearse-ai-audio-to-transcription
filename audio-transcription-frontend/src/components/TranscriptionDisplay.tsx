import { TranscriptionResult } from "../types";

interface TranscriptionDisplayProps {
  result: TranscriptionResult | null;
  isLoading: boolean;
}

export default function TranscriptionDisplay({
  result,
  isLoading,
}: TranscriptionDisplayProps) {
  if (isLoading) {
    return (
      <div className="mt-8 p-6 rounded-lg border border-gray-300 dark:border-gray-700">
        <div className="animate-pulse flex flex-col space-y-4">
          <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-1/4"></div>
          <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-3/4"></div>
          <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-1/2"></div>
          <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-2/3"></div>
        </div>
      </div>
    );
  }

  if (!result) {
    return null;
  }

  if (!result.success) {
    return (
      <div className="mt-8 p-6 rounded-lg border border-red-300 bg-red-50 dark:border-red-700 dark:bg-red-900/20">
        <h3 className="text-lg font-medium text-red-800 dark:text-red-400">
          Transcription Failed
        </h3>
        <p className="mt-2 text-red-700 dark:text-red-300">
          {result.errorMessage ||
            "An unknown error occurred during transcription."}
        </p>
      </div>
    );
  }

  return (
    <div className="mt-8">
      <div className="rounded-lg border border-gray-300 dark:border-gray-700 overflow-hidden">
        <div className="bg-gray-100 dark:bg-gray-800 px-4 py-3 border-b border-gray-300 dark:border-gray-700">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100">
            Transcription Result
          </h3>
        </div>
        <div className="p-6">
          <div className="bg-white dark:bg-gray-900 rounded p-4 border border-gray-200 dark:border-gray-800 mb-6 max-h-96 overflow-y-auto">
            <p className="whitespace-pre-wrap text-gray-900 dark:text-gray-100">
              {result.transcription}
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm mt-4">
            <div className="bg-gray-50 dark:bg-gray-800 p-3 rounded">
              <span className="block text-gray-500 dark:text-gray-400">
                Duration
              </span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {result.durationInSeconds.toFixed(2)}s
              </span>
            </div>
            <div className="bg-gray-50 dark:bg-gray-800 p-3 rounded">
              <span className="block text-gray-500 dark:text-gray-400">
                Processing Time
              </span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {result.processingTimeMs}ms
              </span>
            </div>
            <div className="bg-gray-50 dark:bg-gray-800 p-3 rounded">
              <span className="block text-gray-500 dark:text-gray-400">
                Model
              </span>
              <span className="font-medium text-gray-900 dark:text-gray-100">
                {result.model}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
