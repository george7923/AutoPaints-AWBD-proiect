// src/app/services/cont-administrator.service.ts

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  RegisterUserDto,
  LoginDto,
  UpdateUserDto
} from '../models/NewDTOs';
import { User } from '../models/User';

@Injectable({
  providedIn: 'root'
})
export class ContAdministratorService {
  private readonly apiUrl = 'https://localhost:7160/api/Account';

  constructor(private http: HttpClient) { }

  /**
   * Obține IdUser după username
   * GET /api/Account/get-id/{username}
   */
  getUserIdByUsername(username: string): Observable<{ IdUser: number }> {
    return this.http.get<{ IdUser: number }>(
      `${this.apiUrl}/get-id/${encodeURIComponent(username)}`
    );
  }

  /**
   * Obține detaliile unui utilizator după username
   * GET /api/Account/User/{username}
   */
  getUserByUsername(username: string): Observable<User> {
    return this.http.get<User>(
      `${this.apiUrl}/User/${encodeURIComponent(username)}`
    );
  }

  /**
   * Înregistrează un utilizator și datele asociate Persoana
   * POST /api/Account/Register
   */
  register(registerDto: RegisterUserDto): Observable<{
    message: string;
    user: { IdUser: number; Username: string; Persoana: any };
  }> {
    return this.http.post<{
      message: string;
      user: { IdUser: number; Username: string; Persoana: any };
    }>(`${this.apiUrl}/Register`, registerDto);
  }

  /**
   * Autentifică un utilizator
   * POST /api/Account/Login
   */
  login(loginDto: LoginDto): Observable<{
    token: string;
    user: any;
  }> {
    return this.http.post<{ token: string; user: any }>(
      `${this.apiUrl}/Login`,
      loginDto
    );
  }

  /**
   * Actualizează un utilizator (inclusiv date Persoana)
   * PUT /api/Account/Update/{id}
   */
  updateUser(
    id: number,
    updateDto: UpdateUserDto
  ): Observable<{ message: string; user: any }> {
    return this.http.put<{ message: string; user: any }>(
      `${this.apiUrl}/Update/${id}`,
      updateDto
    );
  }

  /**
   * Șterge un utilizator
   * DELETE /api/Account/Delete/{id}
   */
  deleteUser(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.apiUrl}/Delete/${id}`
    );
  }
}
