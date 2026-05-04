import { Component } from '@angular/core';
import { ChatService } from '../services/chat.service';

interface Message {
  role: 'user' | 'bot';
  content: string;
}

@Component({
  selector: 'app-talk-chat-bot',
  templateUrl: './talk-chat-bot.component.html',
  styleUrls: ['./talk-chat-bot.component.css'],
})
export class TalkChatBotComponent {
  prompt: string = '';

  messages: Message[] = [
    { role: 'bot', content: "Salut! Cu ce te pot ajuta azi?" }
  ];
  connectionStatus: string = '';
  loading: boolean = false;

  constructor(private chatService: ChatService) { }

  sendPrompt(): void {
    if (!this.prompt.trim()) return;


    const userMsg: Message = { role: 'user', content: this.prompt };
    this.messages.push(userMsg);
    const currentPrompt = this.prompt;
    this.prompt = ''; 
    this.loading = true;

    // Trimite prompt-ul la backend
    this.chatService.sendPrompt(currentPrompt).subscribe(
      (res) => {

        const botResponse: string = res.Response || res.response;
        this.messages.push({ role: 'bot', content: botResponse });
        this.loading = false;

        this.speakText(botResponse);
      },
      (err) => {
        console.error('Eroare API:', err);
        this.messages.push({
          role: 'bot',
          content: 'A apărut o eroare. Încercați din nou.',
        });
        this.loading = false;
      }
    );
  }

  checkConnection(): void {
    this.connectionStatus = 'Verific conexiunea...';
    this.chatService.sendPrompt('Test conexiune').subscribe(
      () => {
        this.connectionStatus = 'Conexiunea cu backend-ul este activă!';
      },
      (err) => {
        console.error('Eroare conexiune:', err);
        this.connectionStatus = `Eroare la conectarea cu backend-ul: ${err.message || 'Detalii indisponibile'}`;
      }
    );
  }

  speakText(text: string): void {
    if ('speechSynthesis' in window) {
      const utterance = new SpeechSynthesisUtterance(text);

      utterance.rate = 1;   
      utterance.pitch = 1;  
      window.speechSynthesis.speak(utterance);
    } else {
      console.warn("Text-to-Speech nu este suportat în acest browser.");
    }
  }
}
