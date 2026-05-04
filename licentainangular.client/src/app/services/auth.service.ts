import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { PersoanaService } from '../services/persoana.service';
import { CardService } from '../services/card.service';
import { CosService } from '../services/cos.service';
import { AdresaService } from '../services/adresa.service';
import { SubprodusService } from '../services/subprodus.service';
import { RegisterUserDto, LoginDto, UpdateUserDto, UpdateDetaliiUserDTO } from '../models/NewDTOs';
import { User } from '../models/User'; // Adjust the path if your folder structure is different
import { Cos } from '../models/Cos';
import { Produs } from '../models/Produs';
import { Card_Plati } from '../models/Card_Plati';
import { tap } from 'rxjs/operators';
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7160/api/account'; // Correct API endpoint

  // Store user information directly in the service
  user: User = {
    IdUser: 0,
    Username: '',
    Password: '',
    IdPersoana: 0,
    IdAdresa: 0,
    Persoana: {
      IdPersoana: 0,
      Nume: '',
      Prenume: '',
      Email: '',
      tipPersoana: '',
      Telefon: '',
    },
  };

  private emailApiUrl = 'https://localhost:7160/api/persoana/send-email';
  cos: Cos = {
    idCos: 0,
    CodUnic: '',
    IdUser: 0,
    User: null, // Ideally, replace 'any' with the actual User model
    DataCreare: new Date(),
  };

  card: Card_Plati = {
    IdCard: 0,
    NumarCard: '',
    CVV: '',
    DataExpirare: new Date(),
    IdUser: 0,
    User: null,
  };

  constructor(
    private http: HttpClient,
    private router: Router,
    private persService: PersoanaService,
    private cardService: CardService,
    private cosService: CosService,
    private adresaService: AdresaService,
    private subprodusService: SubprodusService
  ) {
    this.loadUserFromToken(); // Check and load user data when the service is instantiated
  }

  // Get by Username method
  getUserByUsername(username: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/user/${username}`);
  }

  // Register method
  register(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  // Login method
  login(credentials: LoginDto): Observable<any> {
    return this.http
      .post<{ token: string; user: any }>(
        `${this.apiUrl}/login`,
        credentials
      )
      .pipe(
        tap((response) => {
          console.log('🔹 Full Backend Response:', response);

          if (response.token && response.user) {
            localStorage.setItem('authToken', response.token);
            localStorage.setItem('user', JSON.stringify(response.user));
          }
        })
      );
  }

  updateUser(id: number, user: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${id}`, user);
  }
  trimiteEmail(to: string, subject: string, body: string): Observable<any> {
    return this.http.post(this.emailApiUrl, {
      to,
      subject,
      body
    });
  }


  updatePassword(id: number, newPassword: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/updatepassword/${id}`, JSON.stringify(newPassword), {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    });
  }
  // Delete method
  deleteUser(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }

  // Save token to localStorage and store user info in the service and get token from localStorage

  setToken(token: string): void {
    localStorage.setItem('authToken', token);

    this.loadUserFromToken();
  }


  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  // Check if the user is authenticated (i.e., token exists)
  isAuthenticated(): boolean {
    const token = this.getToken();
    return token !== null;
  }
  updateUserRole(idUser: number, newRole: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-rol/${idUser}`, JSON.stringify(newRole), {
      headers: { 'Content-Type': 'application/json' }
    });
  }



  // Get stored user info from the service
  getUserInfo(): any {
    const userRaw = localStorage.getItem('user');
    const user = userRaw ? JSON.parse(userRaw) : null;

    console.log('🟢 AccountService: Retrieved user from localStorage:', user);

    
      return user; // Still return user even if Persoana is missing
    
  }

  // Logout the user and clear the token and user info
  logout(): void {
    localStorage.removeItem('authToken');
    this.user = {
      IdUser: 0,
      Username: '',
      Password: '',
      IdPersoana: 0,
      IdAdresa: 0,
      Persoana: {
        IdPersoana: 0,
        Nume: '',
        Prenume: '',
        Email: '',
        tipPersoana: '',
        Telefon: '',
      },
    };

    this.cos = {
      idCos: 0,
      CodUnic: '',
      IdUser: 0,
      User: null, // Ideally, replace 'any' with the actual User model
      DataCreare: new Date(),
    };

    this.card = {
      IdCard: 0,
      NumarCard: '',
      CVV: '',
      DataExpirare: new Date(),
      IdUser: 0,
      User: null,
    };

    this.router.navigate(['/authentification']); // Redirect to login page
  }

  //Fetch the user if there's a token
  fetchUserAndSet(username: string): void {
    this.getUserByUsername(username).subscribe({
      next: (response) => {
        this.user = response;
      },
      error: (err) => {
        console.error('Error fetching user data: ', err);
      },
    });
  }
  updateDetaliiUser(idUser: number, dto: any) {
    return this.http.put(`${this.apiUrl}/update-detalii/${idUser}`, dto);
  }


  // Load the user info from the token on service initialization
  private loadUserFromToken(): void {
    const token = this.getToken();
    if (token) {
      // Decode token and update the user info in the service
      const decodedToken = jwtDecode<any>(token);

      // this.fetchUserAndSet(decodedToken.Username);

      this.user = {
        IdUser: parseInt(decodedToken.IdUser, 10),
        Username: decodedToken.Username,
        Password: decodedToken.Password,
        IdPersoana: parseInt(decodedToken.IdPersoana, 10),
        IdAdresa: parseInt(decodedToken.IdAdresa, 10),
        Persoana: {
          IdPersoana: parseInt(decodedToken['Persoana.IdPersoana'], 10),
          Nume: decodedToken['Persoana.Nume'],
          Prenume: decodedToken['Persoana.Prenume'],
          Email: decodedToken['Persoana.Email'],
          tipPersoana: decodedToken['Persoana.tipPersoana'],
          Telefon: decodedToken['Persoana.Telefon'],
        },
      };

      this.persService.getPersoanaById(this.user.IdPersoana).subscribe({
        next: (dto) => {
          this.user.Persoana = {
            IdPersoana: this.user.IdPersoana,
            Nume: dto.nume,
            Prenume: dto.prenume,
            Email: dto.email,
            tipPersoana: dto.tipPersoana,
            Telefon: dto.telefon
          };
        },
        error: (err) => { console.error('Eroare la persoana: ', err); },
});

      this.cosService.getCartByUserId(this.user.IdUser).subscribe({
        next: (response) => {
          this.cos = response;
          this.subprodusService.fetchProducts(this.cos.idCos); 
          console.log('Cos utilizator: ', this.cos);
        },
        error: (err) => {
          console.error('Eroare la cos: ', err);
        },
      });

      this.cardService.getCardsByUserId(this.user.IdUser).subscribe({
        next: (response) => {
          this.card = response[0];
          console.log('Card utilizator: ', this.card);
        },
        error: (err) => {
          console.error('Eroare la card: ', err);
        },
      });
    } else {
      this.user = {
        IdUser: 0,
        Username: '',
        Password: '',
        IdPersoana: 0,
        IdAdresa: 0,
        Persoana: {
          IdPersoana: 0,
          Nume: '',
          Prenume: '',
          Email: '',
          tipPersoana: '',
          Telefon: '',
        },
      };
    }
  }
}


