import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Card_Plati } from '../models/Card_Plati'; // Ensure you have the Card model defined
import { CardPlatiDTO } from '../models/CardPlatiDTO';
import { CardDTO, CardulDTO, UserCardDTO } from '../models/NewDTOs';

@Injectable({
  providedIn: 'root', // This automatically provides the service at the root level (singleton pattern)
})
export class CardService {
  private apiUrl = 'https://localhost:7160/api/card'; // The API URL for Card

  constructor(private http: HttpClient) {}

  // Get all cards
  getAllCards(): Observable<Card_Plati[]> {
    return this.http.get<Card_Plati[]>(`${this.apiUrl}`);
  }

  // Get card by ID
  getCardById(id: number): Observable<Card_Plati> {
    return this.http.get<Card_Plati>(`${this.apiUrl}/${id}`);
  }

  // Get all cards for a user by User ID
  getCardsByUserId(userId: number): Observable<Card_Plati[]> {
    return this.http.get<Card_Plati[]>(`${this.apiUrl}/user/${userId}`);
  }

  // Create a new card
  createCard(card: CardDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, card);
  }

  // Update an existing card
  updateCardsByUserId(idUser: number, updatedCard: any): Observable<any> {
    return this.http.put(
      `${this.apiUrl}/updateByUserId/${idUser}`,
      updatedCard
    );
  }
  updateCard(id: number, card: CardDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${id}`, card);
  }

  // Delete a card by ID
  deleteCardsByUserId(idUser: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteByUserId/${idUser}`);
  }
  deleteCard(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }

  getCardsByUser(userId: number): Observable<CardulDTO[]> {
    return this.http.get<CardulDTO[]>(`${this.apiUrl}/user/${userId}`);
  }


  //Delete All Cards of a User
  deleteAllCardsByUser(userId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/user/${userId}/delete-all`);
  }

  //Assign Card to User
  assignCardToUser(userCard: UserCardDTO): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign`, userCard);
  }

  // Remove Card from User
  removeCardFromUser(userCard: UserCardDTO): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove`, { body: userCard });
  }

  //Remove All Cards from User
  removeAllCardsFromUser(userId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove-all/${userId}`);
  }
}
