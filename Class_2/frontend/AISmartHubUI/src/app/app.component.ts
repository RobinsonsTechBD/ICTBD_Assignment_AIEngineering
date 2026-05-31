import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';

@Component({
  selector: 'app-root',
  standalone: false, // Changed from true to false!
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  apiUrl = 'https://localhost:7246/api/AiHub'; // Adjust to match your .NET Core listening port
  
  userText: string = '';
  aiResponse: string = '';
  isListening: boolean = false;
  historyLogs: any[] = [];
  
  private recognition: any;

  constructor(private http: HttpClient) {
    // Initialize Webkit Native Browser Speech Recognition Engine
    const { webkitSpeechRecognition } = window as any;
    if (webkitSpeechRecognition) {
      this.recognition = new webkitSpeechRecognition();
      this.recognition.continuous = false;
      this.recognition.lang = 'en-US';

      this.recognition.onresult = (event: any) => {
        this.userText = event.results[0][0].transcript;
        this.isListening = false;
        // Automatically send the transcribed text to database log history
        this.logToDatabase('SpeechToText', '[Voice Input Audio]', this.userText);
      };

      this.recognition.onerror = () => {
        this.isListening = false;
      };
    }
  }

  ngOnInit() {
    this.loadHistory();
  }

  // 1. TEXT TO TEXT (Ollama Execution)
  sendTextToAI() {
  if (!this.userText.trim()) return;

  // Pass an object that matches our new C# TextRequestDto backend parameters
  const body = { prompt: this.userText };

  this.http.post<any>(`${this.apiUrl}/text-to-text`, body).subscribe({
    next: (res) => {
      this.aiResponse = res.outputData;
      this.userText = ''; // Clear text area inputs dynamically
      this.loadHistory();
    },
    error: (err) => console.error('API Error:', err)
  });
}
  // sendTextToAI() {
  //   if (!this.userText.trim()) return;

  //   this.http.post<any>(`${this.apiUrl}/text-to-text`, `"${this.userText}"`, {
  //     headers: { 'Content-Type': 'application/json' }
  //   }).subscribe({
  //     next: (res) => {
  //       this.aiResponse = res.outputData;
  //       this.loadHistory();
  //     },
  //     error: (err) => console.error('API Error:', err)
  //   });
  // }

  // 2. NATIVE FRONTEND SPEECH TO TEXT
  toggleListening() {
    if (!this.recognition) {
      alert('Speech recognition is not supported in this browser. Try Chrome!');
      return;
    }
    if (this.isListening) {
      this.recognition.stop();
      this.isListening = false;
    } else {
      this.isListening = true;
      this.recognition.start();
    }
  }

  // 3. NATIVE FRONTEND TEXT TO SPEECH
  speakOutput(textToSpeak: string) {
    if (!textToSpeak) return;
    window.speechSynthesis.cancel(); // Clear old queuing streams
    const utterance = new SpeechSynthesisUtterance(textToSpeak);
    utterance.lang = 'en-US';
    window.speechSynthesis.speak(utterance);

    this.logToDatabase('TextToSpeech', textToSpeak, '[Spoken Audio Output]');
  }

  // Helpers to fetch and log transactions
  loadHistory() {
    this.http.get<any[]>(`${this.apiUrl}/history`).subscribe(res => this.historyLogs = res);
  }

  logToDatabase(type: string, input: string, output: string) {
    const body = { interactionType: type, inputData: input, outputData: output };
    this.http.post(`${this.apiUrl}/log-interaction`, body).subscribe(() => this.loadHistory());
  }
}
