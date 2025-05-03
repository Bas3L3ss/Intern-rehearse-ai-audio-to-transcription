import { useState } from "react";
import FileUploader from "./components/FileUploader";
import TranscribeOptions from "./components/TranscribeOptions";
import TranscriptionDisplay from "./components/TranscriptionDisplay";
import "./App.css";
import { TranscriptionOptions, TranscriptionResult } from "./types";

function App() {
  const [file, setFile] = useState<File | null>(null);
  const [option, setOption] = useState<TranscriptionOptions>({
    language: "en",
    useHighAccuracyModel: false,
  });
  const [isTranscribing, setIsTranscribing] = useState<boolean>(false);
  const [isStreaming, setIsStreaming] = useState<boolean>(false);
  const [result, setResult] = useState<TranscriptionResult | null>(null);
  const [streamingText, setStreamingText] = useState<string>("");
  const [error, setError] = useState<string | null>(null);
  const [abortController, setAbortController] =
    useState<AbortController | null>(null);

  const handleFileChange = (selectedFile: File | null) => {
    setFile(selectedFile);
    setResult(null);
    setStreamingText("");
    setError(null);
  };

  const handleOptionChange = (newOption: TranscriptionOptions) => {
    setOption(newOption);
  };

  const handleAbort = () => {
    if (abortController) {
      abortController.abort();
      setIsTranscribing(false);
      setIsStreaming(false);
      setError("Transcription cancelled");
      setAbortController(null);
    }
  };

  const handleTranscribe = async () => {
    if (!file) {
      setError("Please select an audio file first");
      return;
    }

    setIsTranscribing(true);
    setError(null);
    setResult(null);

    const controller = new AbortController();
    setAbortController(controller);

    try {
      const formData = new FormData();
      formData.append("file", file, file.name);
      formData.append("language", option.language);
      formData.append(
        "useHighAccuracyModel",
        String(option.useHighAccuracyModel)
      );

      const response = await fetch(
        "http://localhost:5108/api/Transcription/upload",
        {
          method: "POST",
          body: formData,
          signal: controller.signal,
        }
      );

      if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `HTTP ${response.status}`);
      }

      const data: TranscriptionResult = await response.json();
      setResult(data);
    } catch (err) {
      if ((err as Error).name === "AbortError") return;
      setError((err as Error).message);
    } finally {
      setIsTranscribing(false);
      setAbortController(null);
    }
  };

  const handleStreamTranscribe = async () => {
    if (!file) {
      setError("Please select an audio file first");
      return;
    }

    setIsStreaming(true);
    setError(null);
    setResult(null);
    setStreamingText("");

    const controller = new AbortController();
    setAbortController(controller);

    try {
      const formData = new FormData();
      formData.append("file", file, file.name);
      formData.append("language", option.language);
      formData.append(
        "useHighAccuracyModel",
        String(option.useHighAccuracyModel)
      );

      const response = await fetch(
        "http://localhost:5108/api/Transcription/upload-stream",
        {
          method: "POST",
          body: formData,
          signal: controller.signal,
        }
      );

      if (!response.ok || !response.body) {
        const text = await response.text();
        throw new Error(text || `HTTP ${response.status}`);
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder("utf-8");

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        const chunk = decoder.decode(value, { stream: true });
        setStreamingText((prev) => prev + chunk);
      }
    } catch (err) {
      if ((err as Error).name === "AbortError") return;
      setError((err as Error).message);
    } finally {
      setIsStreaming(false);
      setAbortController(null);
    }
  };

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <header className="mb-10 text-center">
        <h1 className="text-4xl font-bold text-gray-800 mb-2">
          Audio Transcription Tool
        </h1>
        <p className="text-lg text-gray-600">
          Upload an audio file to generate a text transcription using AI
        </p>
      </header>

      <main className="mb-12">
        <div className="bg-white rounded-xl shadow-sm p-8 mb-8">
          <FileUploader onFileSelected={handleFileChange} />

          <div className="mt-6">
            <TranscribeOptions onChange={handleOptionChange} options={option} />
          </div>

          <div className="flex flex-wrap gap-4 mt-6">
            <button
              className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
              onClick={handleTranscribe}
              disabled={!file || isTranscribing || isStreaming}
            >
              {isTranscribing ? "Transcribing..." : "Transcribe Audio"}
            </button>

            <button
              className="flex-1 px-6 py-3 bg-green-600 text-white rounded-lg font-semibold hover:bg-green-700 transition-colors disabled:bg-gray-400 disabled:cursor-not-allowed"
              onClick={handleStreamTranscribe}
              disabled={!file || isStreaming || isTranscribing}
            >
              {isStreaming ? "Streaming..." : "Stream Transcription"}
            </button>

            {(isTranscribing || isStreaming) && (
              <button
                className="px-6 py-3 bg-red-600 text-white rounded-lg font-semibold hover:bg-red-700 transition-colors"
                onClick={handleAbort}
              >
                Cancel
              </button>
            )}
          </div>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700">
            <p>{error}</p>
          </div>
        )}

        {result && !isStreaming && (
          <TranscriptionDisplay result={result} isLoading={isTranscribing} />
        )}

        {(isStreaming || streamingText) && (
          <div className="bg-white rounded-xl shadow-sm p-8">
            <h2 className="text-2xl font-semibold text-gray-800 mb-4">
              Streaming Transcription
            </h2>
            <pre className="bg-gray-50 p-4 rounded-lg overflow-x-auto">
              {streamingText}
            </pre>
          </div>
        )}
        {isStreaming && !streamingText && (
          <div className="mt-8 p-6 rounded-lg border border-gray-300 dark:border-gray-700">
            <div className="animate-pulse flex flex-col space-y-4">
              <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-1/4"></div>
              <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-3/4"></div>
              <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-1/2"></div>
              <div className="h-4 bg-gray-300 dark:bg-gray-700 rounded w-2/3"></div>
            </div>
          </div>
        )}
      </main>

      <footer className="text-center text-gray-500 text-sm">
        <p>Powered by Whisper AI</p>
      </footer>
    </div>
  );
}

export default App;
