"""
Streaming audio transcription script using Whisper AI model.
Displays transcription results incrementally as they are processed.
Uses a direct file-based chunking approach to avoid pydub dependency.
Supports concurrent processing of multiple audio files.
"""

import sys
import os
import argparse
import time
import torch
import numpy as np
import whisper
from whisper.utils import format_timestamp
import wave
import subprocess
import uuid
import shutil
from pathlib import Path

def parse_args():
    parser = argparse.ArgumentParser(description="Stream transcribe audio using Whisper AI")
    parser.add_argument("audio_path", nargs="?", default="test.mp3", help="Path to the audio file to transcribe")
    parser.add_argument("--model", default="small", choices=["tiny", "base", "small", "medium", "large"],
                        help="Whisper model to use (default: small)")
    parser.add_argument("--language", default="en", help="Language code (e.g., en, fr, es) (default: en)")
    parser.add_argument("--chunk_length", type=float, default=30.0, 
                        help="Length of audio chunks in seconds (default: 30)")
    parser.add_argument("--verbose", action="store_true", help="Print detailed timing info")
    parser.add_argument("--job_id", default=None, help="Custom job ID (defaults to auto-generated UUID)")
    return parser.parse_args()

def get_job_directory(base_dir, audio_path, job_id):
    """
    Create a unique directory for this transcription job.
    """
    # Get the filename without extension
    filename = os.path.splitext(os.path.basename(audio_path))[0]
    
    # Create a sanitized version of the filename (remove special characters)
    safe_filename = ''.join(c if c.isalnum() else '_' for c in filename)
    
    # Create the job directory path
    job_dir = os.path.join(base_dir, f"{safe_filename}_{job_id}")
    
    # Ensure the directory exists
    os.makedirs(job_dir, exist_ok=True)
    
    return job_dir

def split_audio_ffmpeg(input_file, chunk_length_sec=30.0, job_id=None):
    """
    Split audio file into chunks using ffmpeg.
    Returns list of temporary WAV files and their start times.
    """
    # Create a unique job ID if not provided
    if job_id is None:
        job_id = str(uuid.uuid4())[:8]
    
    # Create a base directory for all temporary chunks
    base_temp_dir = "temp_chunks"
    os.makedirs(base_temp_dir, exist_ok=True)
    
    # Get job-specific directory
    job_dir = get_job_directory(base_temp_dir, input_file, job_id)
    
    # Get duration using ffprobe
    cmd = [
        "ffprobe", "-v", "error", "-show_entries", "format=duration",
        "-of", "default=noprint_wrappers=1:nokey=1", input_file
    ]
    try:
        duration = float(subprocess.check_output(cmd).decode('utf-8').strip())
    except (subprocess.SubprocessError, ValueError) as e:
        print(f"Error getting audio duration: {e}", file=sys.stderr)
        sys.exit(1)
    
    chunk_files = []
    start_times = []
    
    # Split into chunks
    for start_time in np.arange(0, duration, chunk_length_sec):
        end_time = min(start_time + chunk_length_sec, duration)
        output_file = os.path.join(job_dir, f"chunk_{start_time:.2f}_{end_time:.2f}.wav")
        
        cmd = [
            "ffmpeg", "-y", "-i", input_file, "-ss", str(start_time),
            "-to", str(end_time), "-c:a", "pcm_s16le", "-ar", "16000", "-ac", "1",
            output_file
        ]
        
        try:
            subprocess.run(cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            chunk_files.append(output_file)
            start_times.append(start_time)
        except subprocess.SubprocessError as e:
            print(f"Error splitting audio at {start_time}: {e}", file=sys.stderr)
    
    return chunk_files, start_times, job_dir

def load_audio_chunk(file_path):
    """Load audio data from a WAV file."""
    try:
        with wave.open(file_path, 'rb') as wav_file:
            # Check sample rate and channels
            if wav_file.getframerate() != 16000 or wav_file.getnchannels() != 1:
                raise ValueError(f"WAV file must be 16kHz mono (got {wav_file.getframerate()}Hz, {wav_file.getnchannels()} channels)")
            
            # Read audio data
            audio_data = wav_file.readframes(wav_file.getnframes())
            audio_array = np.frombuffer(audio_data, dtype=np.int16).astype(np.float32) / 32768.0
            
            return audio_array
    except Exception as e:
        print(f"Error loading audio chunk: {e}", file=sys.stderr)
        return None

def process_audio_chunk(audio_data, model, language, base_start_time=0.0):
    """Process a single audio chunk using Whisper."""
    # Process with Whisper
    result = model.transcribe(
        audio_data, 
        language=language, 
        fp16=torch.cuda.is_available()
    )
    
    # Adjust segment timestamps by adding base_start_time
    for segment in result["segments"]:
        segment["start"] += base_start_time
        segment["end"] += base_start_time
    
    return result["segments"]

def cleanup_job_directory(job_dir):
    """Clean up the job-specific directory."""
    try:
        if os.path.exists(job_dir):
            shutil.rmtree(job_dir)
            
        # Try to remove the parent directory if it's empty
        parent_dir = os.path.dirname(job_dir)
        if os.path.exists(parent_dir) and not os.listdir(parent_dir):
            os.rmdir(parent_dir)
    except Exception as e:
        print(f"Warning: Failed to clean up temporary directory {job_dir}: {e}", file=sys.stderr)

def main():
    args = parse_args()
    
    if not os.path.exists(args.audio_path):
        print(f"Error: Audio file not found at {args.audio_path}", file=sys.stderr)
        sys.exit(1)
    
    # Create or use the provided job ID
    job_id = args.job_id if args.job_id else str(uuid.uuid4())[:8]
    
    if args.verbose:
        print(f"Job ID: {job_id}", file=sys.stderr)
        print(f"Processing file: {args.audio_path}", file=sys.stderr)
    
    # Check if ffmpeg is available
    try:
        subprocess.run(["ffmpeg", "-version"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    except (subprocess.SubprocessError, FileNotFoundError):
        print("Error: ffmpeg not found. Please install ffmpeg.", file=sys.stderr)
        sys.exit(1)
    
    if args.verbose:
        print(f"Loading Whisper model: {args.model}", file=sys.stderr)
    
    # Load the model once to avoid reloading for each chunk
    device = "cuda" if torch.cuda.is_available() else "cpu"
    model = whisper.load_model(args.model, device=device)
    
    if args.verbose:
        print(f"Using device: {device}", file=sys.stderr)
        print(f"Processing audio: {args.audio_path}", file=sys.stderr)
        print(f"Splitting into {args.chunk_length}s chunks", file=sys.stderr)
    
    # Split audio into chunks using ffmpeg
    try:
        chunk_files, start_times, job_dir = split_audio_ffmpeg(
            args.audio_path, 
            args.chunk_length, 
            job_id
        )
    except Exception as e:
        print(f"Error splitting audio: {e}", file=sys.stderr)
        sys.exit(1)
    
    if args.verbose:
        print(f"Total chunks: {len(chunk_files)}", file=sys.stderr)
        print(f"Temporary directory: {job_dir}", file=sys.stderr)
    
    # Set up for real-time flushing of output
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(line_buffering=True)
    
    # Process each chunk and stream results
    print("WEBVTT\n")
    
    try:
        for i, (chunk_file, start_time) in enumerate(zip(chunk_files, start_times)):
            if args.verbose:
                chunk_start = time.time()
                print(f"Processing chunk {i+1}/{len(chunk_files)} (starting at {start_time:.2f}s)...", file=sys.stderr)
            
            # Load audio data
            audio_data = load_audio_chunk(chunk_file)
            if audio_data is None:
                continue
            
            # Process chunk
            segments = process_audio_chunk(audio_data, model, args.language, start_time)
            
            # Output results
            for segment in segments:
                start = format_timestamp(segment["start"], always_include_hours=True)
                end = format_timestamp(segment["end"], always_include_hours=True)
                print(f"\n{start} --> {end}")
                print(segment["text"].strip())
                sys.stdout.flush()  # Force output to be displayed immediately
            
            if args.verbose:
                chunk_elapsed = time.time() - chunk_start
                print(f"Chunk {i+1} processed in {chunk_elapsed:.2f} seconds", file=sys.stderr)
    
    finally:
        # Clean up temporary files
        if args.verbose:
            print(f"Cleaning up temporary directory: {job_dir}", file=sys.stderr)
        cleanup_job_directory(job_dir)

if __name__ == "__main__":
    main()