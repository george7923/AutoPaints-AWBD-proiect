import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// DTO pentru datele trimise spre server, analog cu CreateVopseaDto din C#
export interface CreateVopseaDto {
  tipVopsea: string;           // "spray" sau "vopsea"
  marcaMasinii: string;        // Marca
  codCuloare: string;          // Codul culorii
  model: string;               // Modelul mașinii
  an: string;                  // An fabricație
  serieCaroserie: string;      // Serie caroserie
  detaliiSuplimentare: string; // Orice detalii
  cantitate: number;           // Câte subproduse generăm
}

// DTO pentru raspunsul primit de la server, ex. {"message": "...", "IdProdus": 123, "IdVopsea": 45, "SubproduseCreate": N}
export interface VopseaResponseDto {
  message: string;
  idProdus: number;
  idVopsea: number;
  subproduseCreate: number;
}

export interface CreateVopseaSiAdaugaInCosDto {
  IdUser: number;
  tipVopsea: string;
  marcaMasinii: string;
  codCuloare: string;
  model: string;
  an: number; 
  serieCaroserie: string;
  detaliiSuplimentare: string;
  cantitate: number;
}


export interface VopseaCreareCosResponseDto {
  message: string;
  idProdus: number;
  idVopsea: number;
  idCos: number;
  subproduseCreateInCos: number;
}


@Injectable({
  providedIn: 'root'
})
export class VopseaService {
  private apiUrl = 'https://localhost:7160/api/vopsea'; 

  constructor(private http: HttpClient) { }

  // Apel la POST /api/vopsea/creare
  creareVopseaSiAdaugaInCos(payload: CreateVopseaSiAdaugaInCosDto): Observable<VopseaCreareCosResponseDto> {
    return this.http.post<VopseaCreareCosResponseDto>(
      `${this.apiUrl}/creare-si-adauga-in-cos`,
      payload
    );
  }

}
