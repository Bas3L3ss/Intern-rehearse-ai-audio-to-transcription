import { TranscriptionOptions as TranscriptionOptionsTypes } from "../types";
import LanguageSelector from "./LanguageSelector";

interface TranscriptionOptionsProps {
  options: TranscriptionOptionsTypes;
  onChange: (options: TranscriptionOptionsTypes) => void;
}

export default function TranscriptionOptions({
  options,
  onChange,
}: TranscriptionOptionsProps) {
  const handleLanguageChange = (language: string) => {
    onChange({ ...options, language });
  };

  const handleModelChange = (useHighAccuracyModel: boolean) => {
    onChange({ ...options, useHighAccuracyModel });
  };

  return (
    <div className="space-y-6">
      <h3 className="text-lg font-medium text-gray-900">
        Transcription Options
      </h3>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <LanguageSelector
          selectedLanguage={options.language}
          onChange={handleLanguageChange}
        />

        <div className="w-full">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Model Accuracy
          </label>
          <div className="flex items-center space-x-6">
            <label className="inline-flex items-center cursor-pointer">
              <input
                type="radio"
                className="sr-only peer"
                checked={!options.useHighAccuracyModel}
                onChange={() => handleModelChange(false)}
              />
              <span className="w-4 h-4 border rounded-full border-gray-300 peer-checked:border-blue-500 peer-checked:bg-blue-500 peer-checked:after:block peer-checked:after:w-2 peer-checked:after:h-2 peer-checked:after:rounded-full peer-checked:after:bg-white peer-checked:after:translate-x-1 peer-checked:after:translate-y-1"></span>
              <span className="ml-3 text-sm text-gray-700">Standard</span>
            </label>
            <label className="inline-flex items-center cursor-pointer">
              <input
                type="radio"
                className="sr-only peer"
                checked={options.useHighAccuracyModel}
                onChange={() => handleModelChange(true)}
              />
              <span className="w-4 h-4 border rounded-full border-gray-300 peer-checked:border-blue-500 peer-checked:bg-blue-500 peer-checked:after:block peer-checked:after:w-2 peer-checked:after:h-2 peer-checked:after:rounded-full peer-checked:after:bg-white peer-checked:after:translate-x-1 peer-checked:after:translate-y-1"></span>
              <span className="ml-3 text-sm text-gray-700">
                High Accuracy (slower)
              </span>
            </label>
          </div>
        </div>
      </div>
    </div>
  );
}
