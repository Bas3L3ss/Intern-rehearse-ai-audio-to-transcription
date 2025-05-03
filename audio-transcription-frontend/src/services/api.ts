import axios from "axios";
import { TranscriptionOptions, TranscriptionResult } from "../types";

const API_BASE_URL = "https://localhost:7225/api/Transcription";

export const transcribeAudio = async (
  audioFile: File,
  options?: TranscriptionOptions
): Promise<TranscriptionResult> => {
  const formData = new FormData();
  formData.append("audioFile", audioFile);

  if (options) {
    if (options.language) {
      formData.append("language", options.language);
    }
    formData.append(
      "useHighAccuracyModel",
      options.useHighAccuracyModel.toString()
    );
  }

  try {
    const response = await axios.post<TranscriptionResult>(
      `${API_BASE_URL}/transcribe`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error) && error.response) {
      throw new Error(
        error.response.data.detail || "Failed to transcribe audio"
      );
    }
    throw new Error("Failed to transcribe audio");
  }
};
