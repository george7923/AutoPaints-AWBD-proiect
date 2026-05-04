import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Subprodus } from '../models/Subprodus'; // Define this model based on your Subprodus class
import { Produs } from '../models/Produs';
import { map } from 'rxjs/operators';
import { ProdusCosDTO } from '../models/ProdusCosDTO';
import {
  SubprodusDTO,
  FetchSubprodusDTO,
  DeleteSubprodusDTO,
} from '../models/NewDTOs';

@Injectable({
  providedIn: 'root',
})
export class SubprodusService {
  constructor(private http: HttpClient) {}

  private apiUrl = 'https://localhost:7160/api/subprodus'; 

  products: Produs[] = []; // Array to hold products
  IsInCos: { [id: number]: number } = {}; // Mapping for product ID to flag

  fetchProducts(idCos: number): void {
    this.getProdusByCosId(idCos).subscribe(
      (data) => {
        this.products = data;
        console.log('All products in cos fetched:', data);

        this.IsInCos = {}; 
        data.forEach((product) => {
          this.IsInCos[product.IdProdus] = 1; // Mark as in the cart
        });

        //console.log('IsInCos:', this.IsInCos);
      },
      (error) => {
        console.error('Error fetching all products:', error);
      }
    );
  }


  getProdusByCosId(idCos: number): Observable<Produs[]> {
    return this.http.get<Produs[]>(`${this.apiUrl}/by-cos/${idCos}`);
  }

  deleteSubprodusById(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  deleteSubprodusByCosProd(idCos: number, idProdus: number): Observable<void> {
    const url = `${this.apiUrl}/delete-subprodus?idCos=${idCos}&idProdus=${idProdus}`;
    return this.http.delete<void>(url);
  }

  deleteSubproduseByProdusId(idProdus: number): Observable<void> {
    const url = `${this.apiUrl}/delete-by-produs/${idProdus}`;
    return this.http.delete<void>(url);
  }


  deleteSubproduseByCosId(idCos: number): Observable<void> {
    const url = `${this.apiUrl}/delete-by-cos/${idCos}`;
    return this.http.delete<void>(url);
  }

  // Get Subprodus by Produs Id
  getSubprodusByProdusId(idProdus: number): Observable<Subprodus[]> {
    return this.http.get<Subprodus[]>(`${this.apiUrl}/by-produs/${idProdus}`);
  }
  countSubproduseByProdusId(idProdus: number): Observable<number> {
    return this.http
      .get<{ Count: number }>(`${this.apiUrl}/count-by-produs/${idProdus}`)
      .pipe(map((response) => response.Count));
  }
  countAvailableSubproduseByProdusId(produsId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/countAvailable/${produsId}`);
  }
  getProdusByCosJoin(idCos: number): Observable<ProdusCosDTO[]> {
    return this.http.get<ProdusCosDTO[]>(
      `${this.apiUrl}/join-la-toate-produsele/${idCos}`
    );
  }

  getAllSubproduse(): Observable<SubprodusDTO[]> {
    return this.http.get<SubprodusDTO[]>(`${this.apiUrl}`);
  }

  //Get Subproduct by ID
  getSubprodusById(id: number): Observable<SubprodusDTO> {
    return this.http.get<SubprodusDTO>(`${this.apiUrl}/${id}`);
  }

  //Get Products by Cart ID
  getProductsByCart(idCos: number): Observable<SubprodusDTO[]> {
    return this.http.get<SubprodusDTO[]>(`${this.apiUrl}/by-cos/${idCos}`);
  }

  //Create a New Subprodus
  createSubprodus(subprodus: SubprodusDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}`, subprodus);
  }

  //Delete a Subprodus by ID
  deleteSubprodus(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  //Delete a Subprodus by Cart and Product
  deleteSubprodusByCosAndProdus(request: DeleteSubprodusDTO): Observable<any> {
    return this.http.delete(
      `${this.apiUrl}/delete?idCos=${request.idCos}&idProdus=${request.idProdus}`
    );
  }
}
