import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Subcomanda } from '../models/Subcomanda'; // Define this model based on your Subcomanda class
import { Produs } from '../models/Produs'; // Define this model based on your Produs class
import {
  SubcomandaDTO,
  FetchSubcomandaDTO,
  DeleteSubcomandaDTO,
} from '../models/NewDTOs';

@Injectable({
  providedIn: 'root',
})
//NEFOLOSIT
export class SubcomandaService {
  constructor(private http: HttpClient) {}

  private apiUrl = 'https://localhost:7160/api/subcomanda'; // Update with your correct API endpoint

  // Fetch products by ComandaId
  getProdusByComandaId(idComanda: number): Observable<Produs[]> {
    return this.http.get<Produs[]>(`${this.apiUrl}/by-comanda/${idComanda}`);
  }

  getAllProductsFromSubcomenzi(): Observable<Produs[]> {
    return this.http.get<Produs[]>(`${this.apiUrl}/all-products`);
  }

  getProdusByUserId(idUser: number): Observable<Produs[]> {
    return this.http.get<Produs[]>(`${this.apiUrl}/by-user/${idUser}`);
  }

  // Create a new Subcomanda

  // Delete Subcomanda by Id
  deleteSubcomandaById(idSubcomanda: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${idSubcomanda}`);
  }

  // Delete Subcomanda by ComandaId and ProdusId

  //Get All Suborders
  getAllSubcomenzi(): Observable<SubcomandaDTO[]> {
    return this.http.get<SubcomandaDTO[]>(`${this.apiUrl}`);
  }

  //Get Suborder by ID
  getSubcomandaById(id: number): Observable<SubcomandaDTO> {
    return this.http.get<SubcomandaDTO>(`${this.apiUrl}/${id}`);
  }

  //Get Products by Order ID
  getProductsByOrder(idComanda: number): Observable<SubcomandaDTO[]> {
    return this.http.get<SubcomandaDTO[]>(
      `${this.apiUrl}/by-comanda/${idComanda}`
    );
  }

  //Get Products by User ID
  getProductsByUser(idUser: number): Observable<SubcomandaDTO[]> {
    return this.http.get<SubcomandaDTO[]>(`${this.apiUrl}/by-user/${idUser}`);
  }

  //Create a New Subcomanda
  createSubcomanda(subcomanda: SubcomandaDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}`, subcomanda);
  }

  //Delete a Subcomanda by ID
  deleteSubcomanda(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  //Delete a Subcomanda by Order and Product
  deleteSubcomandaByComandaAndProdus(
    request: DeleteSubcomandaDTO
  ): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/delete-subcomanda?idComanda=${request.idComanda}&idProdus=${request.idProdus}`
    );
  }
}
