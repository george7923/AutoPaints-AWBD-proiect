import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../services/product.service';
import { ProdusReducereDTO } from '../models/NewDTOs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  produseReducere: ProdusReducereDTO[] = [];

  constructor(
    private productService: ProductService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadReduceri();
  }

  loadReduceri(): void {
    this.productService.getProduseCuReducere().subscribe({
      next: (produse: any[]) => {
        this.produseReducere = produse.map(prod => {
          let img = prod.imagine;

          // 1. Array de bytes
          if (Array.isArray(img) && img.length > 0) {
            const uint8Array = new Uint8Array(img);
            img = 'data:image/png;base64,' + this.arrayBufferToBase64(uint8Array);
          }
          // 2. String fara prefix base64
          else if (typeof img === 'string' && img && !img.startsWith('data:image')) {
            img = 'data:image/png;base64,' + img;
          }
          // 3. Fallback daca e null sau gol
          else if (!img) {
            img = 'assets/logoR.png';
          }

          return {
            ...prod,
            imagine: img
          };
        });
      },
      error: (err: any) => {
        this.produseReducere = [];
        console.error('Eroare la încărcarea produselor cu reducere:', err);
      }
    });
  }


  navigateToProduct(product: ProdusReducereDTO): void {
    if (product.idProdus != null) {
      this.router.navigate(['/product', product.idProdus]);
    }
  }
  private arrayBufferToBase64(buffer: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < buffer.byteLength; i++) {
      binary += String.fromCharCode(buffer[i]);
    }
    return window.btoa(binary);
  }

}
