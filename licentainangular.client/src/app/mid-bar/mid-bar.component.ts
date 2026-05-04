import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { Produs } from '../models/Produs';
import { AuthService } from '../services/auth.service';
import { ProductService } from '../services/product.service';
import { Persoana } from '../models/Persoana'; 
import { SearchProdusDTO } from '../models/NewDTOs';

@Component({
  selector: 'app-mid-bar',
  templateUrl: './mid-bar.component.html',
  styleUrls: ['./mid-bar.component.css'],
  standalone: true,
  imports: [FormsModule, RouterLink, CommonModule],
})
export class MidBarComponent implements OnInit {
  searchTerm: string = '';

  searchResults: SearchProdusDTO[] = [];

  currentPersoana: Persoana | null = null;
  currentUsername: string = '';

  categoryImages: { [key: string]: string } = {
    'Accesorii Vopsitorie': 'assets/images/accesorii-vopsitorie.png',
    'Antifon - Insonorizanti': 'assets/images/antifon-insonorizanti.png',
    'Chit Auto': 'assets/images/chit-auto.png',
    'Diluant Auto': 'assets/images/diluant-auto.png',
    'Lac Auto': 'assets/images/lac-auto.png',
    'Polish Auto': 'assets/images/polish-auto.png',
    'Spray Vopsea Auto Diverse Aplicatii': 'assets/images/spray-vopsea-diverse.png',
    'Vopsea Auto Preparata': 'assets/images/vopsea-auto-preparata.png',
    'Vopsea Auto Ready Mix': 'assets/car-paint.png',
    'Creion Corector Vopsea Auto': 'assets/images/creion-corector.png',
    'Recipient Pensula Retus': 'assets/images/recipient-pensula.png',
    'Spray Vopsea Auto Preparat': 'assets/images/spray-vopsea-preparat.png',
  };

  constructor(
    public authService: AuthService,
    public productService: ProductService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const userInfo = this.authService.getUserInfo();
    if (userInfo) {

      this.currentUsername = userInfo.username;

      if (userInfo.persoana) {
        this.currentPersoana = userInfo.persoana;
      } else {

        this.currentPersoana = userInfo;
      }
      console.log('Loaded Persoana:', this.currentPersoana);
    } else {
      console.warn('No user info found in AccountService.');
    }
  }

  onSearchInput(): void {
    if (this.searchTerm.trim()) {
      this.productService.searchProductsByName(this.searchTerm).subscribe({
        next: (products: SearchProdusDTO[]) => {
          this.searchResults = products.filter(p => p.idUser !== null);
        },
        error: (err) => {
          console.error('Search error:', err);
          this.searchResults = [];
        },
      });
    } else {
      this.searchResults = [];
    }
  }


  onSearch(event: Event): void {
    event.preventDefault();
    this.onSearchInput();
  }

  navigateToHome(): void {
    this.router.navigate(['/home']);
  }

  navigateToProfile(): void {
    this.router.navigate(['/profil']);
  }

  navigateToLogin(): void {
    this.router.navigate(['/authentification']);
  }

  navigateToIstoricComenzi(): void {
    this.router.navigate(['/istoric-comenzi']);
  }

  navigateToProduct(product: { idProdus: number }): void {
    this.router.navigate(['/product', product.idProdus]);
  }


  logout(): void {
    this.authService.logout();
  }
}
