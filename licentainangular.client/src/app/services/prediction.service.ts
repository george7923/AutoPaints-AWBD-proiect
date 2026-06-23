import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PredictionService {

  private apiUrl = 'https://localhost:7160/api/prediction/predict';

  constructor(private http: HttpClient) { }

  predictDefect(imageFile: File): Observable<any> {
    const formData = new FormData();
    formData.append('imageFile', imageFile);
    return this.http.post<any>(this.apiUrl, formData);
  }
}
