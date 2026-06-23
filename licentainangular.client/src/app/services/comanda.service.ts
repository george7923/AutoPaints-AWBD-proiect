import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { Comanda } from '../models/Comanda'; // Ensure this model is defined in your project
import { ComandaDTO, ComandaPaymentDTO, ComandaUserDTO, SimularePlataDTO } from '../models/NewDTOs';

@Injectable({
  providedIn: 'root',
})
export class ComandaService {
  private apiUrl = 'https://localhost:7160/api/comanda'; // Your correct API endpoint for Comanda

  constructor(
    private http: HttpClient,
    private router: Router // <-- Add Router injection
  ) {}

  // Get all Comenzi (Orders)
  getAllComenzi(): Observable<Comanda[]> {
    return this.http.get<Comanda[]>(`${this.apiUrl}`);
  }

  // Get a specific Comanda (Order) by ID
  getComandaById(id: number): Observable<Comanda> {
    return this.http.get<Comanda>(`${this.apiUrl}/${id}`);
  }

  // Get all Comenzi (Orders) for a specific User by User ID
  /*
  getComenziByUserId(userId: number): Observable<Comanda[]> {
    return this.http.get<Comanda[]>(`${this.apiUrl}/user/${userId}`);
  }
  */
  getComenziByUserId(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/afisare/${userId}`);
  }


  getComenziByUser(userId: number): Observable<ComandaDTO[]> {
    return this.http.get<ComandaDTO[]>(`${this.apiUrl}/user/${userId}`);
  }

  getComenziByUserCuNouDTO(userId: number): Observable<ComandaUserDTO[]> {
    return this.http.get<ComandaUserDTO[]>(`${this.apiUrl}/afisare/${userId}`);
  }

  // Create a new Comanda (Order)
  createComanda(comanda: Comanda): Observable<Comanda> {
    return this.http.post<Comanda>(`${this.apiUrl}`, comanda);
  }

  // Update an existing Comanda (Order)
  updateComanda(comanda: Comanda): Observable<Comanda> {
    return this.http.put<Comanda>(
      `${this.apiUrl}/${comanda.IdComanda}`,
      comanda
    );
  }

  // Delete a Comanda (Order) by ID
  deleteComanda(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  submitComandaWithPayment(payload: ComandaPaymentDTO): void {
    this.http.post<any>(`${this.apiUrl}/submit-with-payment`, payload).subscribe({
      next: (response: any) => {
        console.log('Payment and order submitted successfully:', response);


        if (response.comanda?.IdComanda) {
          this.router.navigate([`/comanda`, response.comanda.IdComanda]);
        }
      },
      error: (err: any) => {
        console.error('Error submitting order:', err);
      },
    });
  }
  emitereComanda(payload: {
    idUser: number;
    idAdresa: number;
    idCard: number;
  }): void {
    this.http.post<any>(`${this.apiUrl}/emitere`, payload).subscribe({
      next: (response) => {
        console.log("Comandă emisă cu succes:", response);
        if (response?.idComanda || response?.IdComanda) {
          //this.router.navigate(['/comanda', response.idComanda || response.IdComanda]);
        }
      },
      error: (err) => {
        console.error("Eroare la emiterea comenzii:", err);
      }
    });
  }

  simulatePayment(cardPayload: SimularePlataDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/simulate-payment`, cardPayload);
  }
  emitereComandaCash(payload: {
    idUser: number;
    idAdresa: number;
    idCard: null;
  }): void {
    this.http.post<any>(`${this.apiUrl}/emitere-cash`, payload).subscribe({
      next: (response) => {
        console.log("Comandă (cash) emisă:", response);

      },
      error: (err) => {
        console.error("Eroare la emiterea comenzii (cash):", err);
      }
    });
  }
  descarcaBonFiscal(idComanda: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/generare-pdf/${idComanda}`, {
      responseType: 'blob'
    });
  }


}
