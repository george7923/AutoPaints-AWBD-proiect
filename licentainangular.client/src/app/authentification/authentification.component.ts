import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth.service'; 

@Component({
  selector: 'app-login',
  templateUrl: './authentification.component.html',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  styleUrls: ['./authentification.component.css'],
})
export class AuthentificationComponent implements OnInit {
  Username: string = '';
  Password: string = '';
  Role: string | null = null; 
  successMessage: string | null = null;
  errorMessage: string | null = null;
  showForgotPassword = false;
  usernameForReset = '';
  emailToSend = '';
  generatedCode = '';
  userInputCode = '';
  codeVerified = false;
  newPassword = '';
  confirmPassword = '';
  codeSent = false;
  emailSentMessage = '';


  constructor(
    private router: Router,
    private accountService: AuthService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {

    this.route.queryParams.subscribe((params) => {
      if (params['success']) {
        this.successMessage = params['success'];
        setTimeout(() => {
          this.successMessage = null;
        }, 5000);
      }
      if (params['username']) {
        this.Username = params['username'];
      }
      if (params['password']) {
        this.Password = params['password'];
      }
    });


    if (this.accountService.isAuthenticated()) {
      const user = this.accountService.getUserInfo();
      console.log('Logged in user:', user);
      this.Role = user?.role || "Unknown"; 
      this.router.navigate(['/home']);
    }
  }

  onSubmit() {
    this.successMessage = null;
    this.errorMessage = null;

    const credentials = { username: this.Username, password: this.Password };


    this.accountService.login(credentials).subscribe({
      next: (response) => {

        this.accountService.setToken(response.token);

        this.Role = response?.user?.role || "Unknown";

        this.successMessage = 'Login successful!';
        this.router.navigate(['/home']);
      },
      error: () => {
        this.errorMessage = 'Invalid credentials. Please try again.';
      },
    });
  }
  toggleForgotPassword() {
    this.showForgotPassword = !this.showForgotPassword;
  }

  sendVerificationCode() {
    if (!this.usernameForReset) return;

    this.accountService.getUserByUsername(this.usernameForReset).subscribe({
      next: (user) => {
        this.emailToSend = user.persoana.email;
        this.generatedCode = Math.floor(100000 + Math.random() * 900000).toString();
        this.codeSent = true;

        const subject = 'Cod pentru resetarea parolei';
        const content = `Bună ziua! Vă atașez următorul cod pentru a schimba parola în următoarele 10 minute, expirând după. Codul: ${this.generatedCode}. EMAIL PRIVAT, A NU SE DISTRIBUI. Mulțumim, AutoPaints Team!`;

        this.accountService.trimiteEmail(this.emailToSend, subject, content).subscribe({
          next: () => this.emailSentMessage = 'Email trimis cu succes!',
          error: () => this.emailSentMessage = 'Eroare la trimiterea emailului.',
        });

        setTimeout(() => { this.generatedCode = ''; }, 10 * 60 * 1000); // Expirare in 10 min
      },
      error: () => alert('Username inexistent.')
    });
  }

  verifyCode() {
    this.codeVerified = this.userInputCode === this.generatedCode;
  }

  resetPassword() {
    const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
    if (!regex.test(this.newPassword)) {
      alert('Parola trebuie să aibă minim 8 caractere, o literă mare, una mică și o cifră.');
      return;
    }
    if (this.newPassword !== this.confirmPassword) {
      alert('Parolele nu coincid.');
      return;
    }

    this.accountService.getUserByUsername(this.usernameForReset).subscribe({
      next: (user) => {
        this.accountService.updatePassword(user.idUser, this.newPassword).subscribe({
          next: () => alert('Parola a fost resetată cu succes!'),
          error: () => alert('Eroare la resetarea parolei.')
        });
      }
    });
  }

}
