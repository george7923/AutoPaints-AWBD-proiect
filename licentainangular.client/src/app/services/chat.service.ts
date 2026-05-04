import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ChatService {

  private apiUrl = 'https://localhost:7160/api/gpt/chat';

  constructor(private http: HttpClient) { }

  sendPrompt(prompt: string): Observable<any> {
    return this.http.post(this.apiUrl, { prompt });
  }
}
