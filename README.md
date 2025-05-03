# Audio to Transcription API using Whisper

## Overview
This project is a locally hosted API that converts audio input into text transcription using OpenAI's Whisper model. It is designed to provide a functional and streamable transcription service, allowing users to receive real-time transcription updates without waiting for the entire process to complete. The project also includes task abortion functionality to stop ongoing transcription tasks when requested.

## Features
1. **Audio to Transcription**: Utilizes the open-source Whisper AI model to transcribe audio files into text.
2. **Streamable API Endpoints**: Users can receive transcription results incrementally, similar to a chatbot experience, without waiting for the entire transcription process to finish.
3. **Task Abortion**: Supports cancellation tokens sent from the frontend to terminate ongoing transcription tasks on the backend.
4. **Internship Preparation**: Built as a rehearsal project for an internship on the same topic, providing a ready-to-run codebase for smoother and more confident development.
5. **Fully Functional**: While the code, performance, and UI may not be optimized, the project is fully functional and serves its intended purpose.

## Tech Stack
- **Languages**: Python (Whisper), TypeScript (React Vite - Frontend), C# (.NET ASP.NET Core - Backend)

## How to Run

### Frontend
1. Navigate to the frontend directory.
2. Install dependencies:
    ```bash
    npm install
    ```
3. Start the development server:
    ```bash
    npm run dev
    ```

### Backend
1. Navigate to the backend directory.
2. Run the backend server:
    ```bash
    dotnet run
    ```

### Python (Whisper)
1. Navigate to the Python directory.
2. Create a virtual environment:
    ```bash
    python -m venv venv
    ```
3. Activate the virtual environment:
    - On Windows:
      ```bash
      venv\Scripts\activate
      ```
    - On Unix or MacOS:
      ```bash
      source venv/bin/activate
      ```
4. Install dependencies:
    ```bash
    pip install -r requirements.txt
    ```

## Notes
- This project is intended for personal use and learning purposes.
- The focus is on functionality rather than performance or user interface design.
- The Whisper model is open-source and runs locally, ensuring privacy and control over the transcription process.

Feel free to explore and modify the codebase to suit your needs!