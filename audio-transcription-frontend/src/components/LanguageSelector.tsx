interface LanguageSelectorProps {
  selectedLanguage: string;
  onChange: (language: string) => void;
}

const languages = [
  { code: "", name: "Auto-detect" },
  { code: "en", name: "English" },
  { code: "vi", name: "Vietnamese" },
  { code: "es", name: "Spanish" },
  { code: "fr", name: "French" },
  { code: "de", name: "German" },
  { code: "it", name: "Italian" },
  { code: "pt", name: "Portuguese" },
  { code: "ru", name: "Russian" },
  { code: "zh", name: "Chinese" },
  { code: "ja", name: "Japanese" },
  { code: "ko", name: "Korean" },
  { code: "ar", name: "Arabic" },
];

export default function LanguageSelector({
  selectedLanguage,
  onChange,
}: LanguageSelectorProps) {
  return (
    <div className="w-full">
      <label
        htmlFor="language-select"
        className="block text-sm font-medium mb-2"
      >
        Audio Language
      </label>
      <select
        id="language-select"
        value={selectedLanguage}
        onChange={(e) => onChange(e.target.value)}
        className="block w-full rounded-md border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 px-3 py-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
      >
        {languages.map((lang) => (
          <option key={lang.code} value={lang.code}>
            {lang.name}
          </option>
        ))}
      </select>
    </div>
  );
}
