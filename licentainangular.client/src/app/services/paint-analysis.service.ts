import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaintAnalysisService {

  private apiUrl = 'https://localhost:7160/api/paintanalysis/analyze';

  constructor(private http: HttpClient) { }

  analyzePaint(imageFile: File): Observable<any> {
    const formData = new FormData();
    formData.append('imageFile', imageFile);
    return this.http.post<any>(this.apiUrl, formData);
  }
}
