import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AdminComenziService {
  private apiUrl = 'https://localhost:7160/api/';

  constructor(private http: HttpClient) { }


  getAllUsers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}account`);
  }

  // Comenzi pentru un user
  getComenziByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}comanda/user/${userId}`);
  }
  getComenziScurtByUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}comanda/scurt/user/${userId}`);
  }


  getAllComenzi(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}comanda`);
  }


  marcheazaCaLivrata(id: number, livrata: boolean): Observable<any> {
    return this.http.put(`${this.apiUrl}comanda/livrare/${id}`, { livrata });
  }



}
