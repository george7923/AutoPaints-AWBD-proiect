import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produs } from '../models/Produs';
import { ProdusDTO, ProdusReducereDTO, SearchProdusDTO } from '../models/NewDTOs';
import { AdminUpdateProdusDTO } from '../models/DTO_Admin';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = 'https://localhost:7160/api/product'; 

  constructor(private http: HttpClient) { }

  //GET ALL PRODUCTS
  getAllProducts(): Observable<ProdusDTO[]> {
    return this.http.get<ProdusDTO[]>(`${this.apiUrl}`);
  }

  //GET PRODUCT BY ID
  getProductById(id: number): Observable<ProdusDTO> {
    return this.http.get<ProdusDTO>(`${this.apiUrl}/${id}`);
  }

  //GET PRODUCTS BY CATEGORY
  getProductsByCategory(category: string): Observable<ProdusDTO[]> {
    return this.http.get<ProdusDTO[]>(`${this.apiUrl}/category/${category}`);
  }


  createProduct(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}`, formData);
  }




  //DELETE PRODUCT
  deleteProduct(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/dezactiveaza/${id}`, null);
  }

  //SEARCH PRODUCTS BY NAME (revine Produs[] - alt model?)
  searchProductsByName(name: string): Observable<SearchProdusDTO[]> {
    return this.http.get<SearchProdusDTO[]>(
      `${this.apiUrl}/search?name=${encodeURIComponent(name)}`
    );
  }

  //GET DETAILED PRODUCTS
  getDetailedProducts(): Observable<ProdusDTO[]> {
    return this.http.get<ProdusDTO[]>(`${this.apiUrl}/detailed`);
  }

  //GET OFFERED PRODUCTS
  getOfferedProducts(): Observable<ProdusDTO[]> {
    return this.http.get<ProdusDTO[]>(`${this.apiUrl}/offer`);
  }
  adaugaReducere(idProdus: number, pretNou: number, dataExpirare?: string) {
    return this.http.post(`${this.apiUrl}/reducere`, {
      idProdus,
      pretNou,
      dataExpirare: dataExpirare || null
    });
  }
  getToatePreturile() {
    return this.http.get<any[]>(`${this.apiUrl}/toate-preturile`);
  }

  getReducereProdus(idProdus: number) {
    return this.http.get<{ pretNou: any, pretVechi: any }>(`${this.apiUrl}/reducere/${idProdus}`);
  }
  getProduseCuReducere(): Observable<ProdusReducereDTO[]> {
    return this.http.get<ProdusReducereDTO[]>(`${this.apiUrl}/cu-reducere`);
  }

  getAllCategories(): Observable<any[]> {
    return this.http.get<any[]>('https://localhost:7160/api/categories'); 
  }

  updateProduct(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }


  updateSubprodusCount(idProdus: number, cantitate: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/subprodus/${idProdus}`, { cantitate });
  }

  getSubprodusCountByProdusId(idProdus: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/subprodus-count/${idProdus}`);
  }
  adminUpdateProdus(idProdus: number, dto: AdminUpdateProdusDTO) {
    // Forțează denumireCategorie cu litere mici, dacă există
    if (dto.denumireCategorie) {
      dto.denumireCategorie = dto.denumireCategorie.toLowerCase();
    }
    return this.http.put(`${this.apiUrl}/admin-update/${idProdus}`, dto);
  }
  getPreturiByProdus(idProdus: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/preturi/${idProdus}`);
  }

  updatePret(idPP: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/modifica-pret/${idPP}`, dto);
  }

  deletePret(idPP: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/sterge-pret/${idPP}`);
  }

}
