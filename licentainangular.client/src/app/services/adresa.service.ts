import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { AdresaDTO, AdresaNestedDTO, UserAddressDTO } from '../models/NewDTOs';
import { Adresa } from '../models/Adresa';

@Injectable({
  providedIn: 'root',
})
export class AdresaService {
  private apiUrl = 'https://localhost:7160/api/adresa';

  constructor(private http: HttpClient) { }


  getAdresaById(id: number): Observable<AdresaDTO> {
    return this.http.get<AdresaDTO>(`${this.apiUrl}/${id}`);
  }


  deleteAdresa(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }

  // GET toate adresele asociate unui user
  getAddressesByUser(userId: number): Observable<Adresa[]> {
    return this.http.get<{ addresses: any[] }>(`${this.apiUrl}/user/${userId}`)
      .pipe(
        map(response => {
          return response.addresses.map((item: any) => ({
            IdAdresa: item.idAdresa,
            Tara: item.strazi?.localitati?.judete?.tari?.denumireTara,
            Judet: item.strazi?.localitati?.judete?.denumireJudet,
            Localitate: item.strazi?.localitati?.denumireLocalitate,
            Strada: item.strazi?.denumireStrada,
            Nr: item.strazi?.nr,
            Bloc: item.bloc,
            Scara: item.scara,
            Etaj: item.etaj,
            Apartament: item.apartament
          }));
        })
      );
  }


  createAdresaForUser(userId: number, adresa: AdresaNestedDTO): Observable<any> {
    // Controller: [HttpPost("createForUser/{userId}")]
    return this.http.post<any>(`${this.apiUrl}/createForUser/${userId}`, adresa);
  }


  updateAdresaNestedById(id: number, adresa: AdresaNestedDTO): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/update/${id}`, adresa);
  }



  assignAddressToUser(payload: UserAddressDTO): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/assign/`, payload);
  }



  removeAddressFromUser(payload: UserAddressDTO): Observable<any> {
    // Controller: [HttpDelete("remove")]
    return this.http.delete<any>(`${this.apiUrl}/remove`, { body: payload });
  }

  getSimplifiedAddressesByUser(userId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/user/${userId}/simplified`);
  }

}
