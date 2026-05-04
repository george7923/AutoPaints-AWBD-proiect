import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Persoana } from '../models/Persoana'; // Ensure this model is defined in your project
import { PersoanaDTO } from '../models/NewDTOs';

@Injectable({
  providedIn: 'root',
})
export class PersoanaService {
  private apiUrl = 'https://localhost:7160/api/persoana'; // Your correct API endpoint for Persoana

  constructor(private http: HttpClient) {}

  // Get all Persoana records
  getAllPersoane(): Observable<Persoana[]> {
    return this.http.get<Persoana[]>(`${this.apiUrl}`);
  }

  // Get a specific Persoana by ID

  // Update an existing Persoana
  updatePersoanaById(id: number, updatedPersoana: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/updateById/${id}`, updatedPersoana);
  }

  // Delete a Persoana by ID
  deletePersoanaById(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteById/${id}`);
  }

  getPersoanaById(id: number): Observable<PersoanaDTO> {
    return this.http.get<PersoanaDTO>(`${this.apiUrl}/${id}`);
  }

  //Get Person by Email
  getPersoanaByEmail(email: string): Observable<PersoanaDTO> {
    return this.http.get<PersoanaDTO>(`${this.apiUrl}/email/${email}`);
  }

  //Create a New Person
  createPersoana(persoana: PersoanaDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}`, persoana);
  }

  //Update Person by ID
  updatePersoana(id: number, persoana: PersoanaDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/updateById/${id}`, persoana);
  }

  //Delete Person by ID
  deletePersoana(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteById/${id}`);
  }
}
