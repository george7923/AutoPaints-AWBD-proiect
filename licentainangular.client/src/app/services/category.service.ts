import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryDTO } from '../models/NewDTOs';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = 'https://localhost:7160/api/Categories';


  constructor(private http: HttpClient) { }

  //GET ALL CATEGORIES
  getAllCategories(): Observable<CategoryDTO[]> {
    return this.http.get<CategoryDTO[]>(`${this.apiUrl}`);
  }

  //GET CATEGORY BY ID
  getCategoryById(id: number): Observable<CategoryDTO> {
    return this.http.get<CategoryDTO>(`${this.apiUrl}/${id}`);
  }

  //ADD CATEGORY (Without Primary Key) 
  addCategory(categoryData: CategoryDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/without_primarykey`, categoryData);
  }

  //UPDATE CATEGORY (Without Primary Key)
  updateCategory(categoryData: CategoryDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/without_primarykey`, categoryData);
  }

  //DELETE CATEGORY
  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
