import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms'; // Pentru ngModel
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PersoanaService } from '../services/persoana.service';
import { CosService } from '../services/cos.service';
import { RegisterUserDto } from '../models/NewDTOs'; 

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  // Using the updated RegisterUserDto (without address properties)
  User: RegisterUserDto = {
    Username: '',
    Password: '',
    Persoana: {
      Nume: '',
      Prenume: '',
      Email: '',
      tipPersoana: '',  // "Fizica" or "Juridica"
      Telefon: '',
      Rol: 'Participant'  // Preset to Participant
    }
  };


  Cos = {
    idCos: 0,
    CodUnic: '',
    IdUser: 0,
    User: null,
    DataCreare: new Date()
  };

  formInvalid: boolean = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private router: Router,
    private accountService: AuthService,
    private persService: PersoanaService,
    private cosService: CosService
  ) { }

  generateRandomCode(length: number): string {
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
      const randomIndex = Math.floor(Math.random() * characters.length);
      result += characters.charAt(randomIndex);
    }
    return result;
  }

  onSubmit() {
    if (this.isValid()) {
      const registerDto: RegisterUserDto = {
        Username: this.User.Username.trim(),
        Password: this.User.Password.trim(),
        Persoana: {
          Nume: this.User.Persoana.Nume.trim(),
          Prenume: this.User.Persoana.Prenume.trim(),
          Email: this.User.Persoana.Email.trim(),
          tipPersoana: this.User.Persoana.tipPersoana.trim(),
          Telefon: this.User.Persoana.Telefon.trim(), 
          Rol: this.User.Persoana.Rol 
        }
      };

      this.accountService.register(registerDto).subscribe({
        next: (response) => {
          console.log("Cont creat: ", response);
          this.successMessage = 'Cont creat cu succes!';

          if (response && response.User && response.User.IdUser) {
            this.createCartIfNotExists(response.User.IdUser);
          }

          this.router.navigate(['/authentification'], {
            queryParams: {
              success: this.successMessage,
              username: registerDto.Username,
              password: registerDto.Password
            }
          });
        },
        error: (error) => {
          console.error("Eroare la înregistrare:", error);
          this.errorMessage = 'Înregistrarea a eșuat: ' + (error.error?.error || 'Unknown error');
        }
      });
    } else {
      this.formInvalid = true;
    }
  }

  private createCartIfNotExists(IdUser: number): void {
    this.cosService.getCartByUserId(IdUser).subscribe({
      next: (existingCart) => {
        if (!existingCart) {
          this.Cos.CodUnic = this.generateRandomCode(10);
          this.Cos.IdUser = IdUser;
          this.Cos.DataCreare = new Date();
          this.cosService.createCart(this.Cos).subscribe({
            next: (response) => {
              console.log("Coș creat: ", response);
            },
            error: (error) => {
              console.error("Eroare la crearea coșului", error);
            }
          });
        } else {
          console.log("User already has a cart:", existingCart);
        }
      },
      error: (err) => {
        console.error("Eroare la verificarea coșului:", err);
      }
    });
  }

  private isValid(): boolean {
    return (
      this.User.Username.trim() !== '' &&
      this.User.Password.trim() !== '' &&
      this.User.Persoana.Nume.trim() !== '' &&
      this.User.Persoana.Prenume.trim() !== '' &&
      this.User.Persoana.Email.trim() !== '' &&
      this.User.Persoana.tipPersoana.trim() !== '' &&
      // Telefon is optional: no validation required.
      this.User.Persoana.Rol.trim() !== ''
    );
  }
}
