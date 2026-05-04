import { Component, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ProductService } from '../services/product.service'; // Import ProductService
import { SubprodusService } from '../services/subprodus.service';
import { CosService } from '../services/cos.service';
import { AuthService } from '../services/auth.service';
import { Produs } from '../models/Produs'; // Import Produs model

@Component({
  selector: 'app-favorite',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './favorite.component.html',
  styleUrls: ['./favorite.component.css'],
})
export class FavoriteComponent {
  favorites = [
    {
      name: 'Șampon auto Premium',
      description: 'Șampon pentru curățarea vopselei auto.',
      price: 100,
      image: 'assets/Imagine1Prezentare.png',
    },
    {
      name: 'Perie auto profesională',
      description: 'Perie specială pentru curățarea interiorului auto.',
      price: 150,
      image: 'assets/Imagine1Prezentare.png',
    },
    {
      name: 'Protecție vopsea',
      description: 'Protecție de lungă durată pentru vopseaua mașinii.',
      price: 200,
      image: 'assets/Imagine1Prezentare.png',
    },
    {
      name: 'Set curățare jante',
      description: 'Set complet pentru curățarea jantelor auto.',
      price: 120,
      image: 'assets/Imagine1Prezentare.png',
    },
    // Add more products as necessary
  ];

  removeFromFavorites(produs: any) {
    this.favorites = this.favorites.filter((favorite) => favorite !== produs);
    alert(`${produs.name} a fost îndepărtat din lista de favorite.`);
  }
}
