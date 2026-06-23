import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ProductService } from '../services/product.service';
import { SubprodusService } from '../services/subprodus.service';
import { CosService } from '../services/cos.service';
import { AuthService } from '../services/auth.service';

import { ProdusDTO } from '../models/NewDTOs';
import { Cos } from '../models/Cos';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css'],
})
export class ProductComponent implements OnInit {
  product!: ProdusDTO;
  totalSubproduseDisponibile: number = 0;
  quantity: number = 1;
  errorMessage: string | null = null;
  successMessage: string | null = null;


  categoryImages: { [key: string]: string } = {
    'Accesorii Vopsitorie': 'assets/images/accesorii-vopsitorie.png',
    'Antifon - Insonorizanti': 'assets/images/antifon-insonorizanti.png',
    'Chit Auto': 'assets/images/chit-auto.png',
    'Diluant Auto': 'assets/images/diluant-auto.png',
    'Lac Auto': 'assets/images/lac-auto.png',
    'Polish Auto': 'assets/images/polish-auto.png',
    'Spray Vopsea Auto Diverse Aplicatii': 'assets/images/spray-vopsea-diverse.png',
    'Vopsea Auto Preparata': 'assets/images/vopsea-auto-preparata.png',
    'Vopsea Auto Ready Mix': 'assets/car-paint.png'
  };

  constructor(
    private productService: ProductService,
    private subprodusService: SubprodusService,
    private cosService: CosService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    // 1) Preluam idProdus din URL
    this.route.paramMap.subscribe((params) => {
      const idParam = params.get('idProdus');
      const productId = idParam ? Number(idParam) : null;
      console.log('Product ID from URL:', idParam);

      if (productId) {
        // 2) Mai intai preluam toata lista de produse (detailed)
        this.productService.getDetailedProducts().subscribe({
          next: (products: any[]) => {
            // 3) Facem mapping pentru a crea array de ProdusDTO
            const mappedProducts = products.map((p) => this.mapToProdusDTO(p));
            // 4) Gasim produsul cu id = productId
            const foundProduct = mappedProducts.find((prod) => prod.ProdusId === productId);

            if (foundProduct) {
              this.product = foundProduct;
              console.log('Found product:', this.product);

              // 5) Aflam cate subproduse disponibile exista
              this.subprodusService.countAvailableSubproduseByProdusId(this.product.ProdusId).subscribe({
                next: (count: number) => {
                  this.totalSubproduseDisponibile = count;
                  console.log(`Subproduse disponibile pt produs ${this.product.ProdusId}:`, count);
                  if (count < 1) {
                    this.errorMessage = "Nu sunt disponibile subproduse pentru acest produs.";
                  }
                },
                error: (err: any) => {
                  console.error('Eroare la countAvailableSubproduseByProdusId:', err);
                },
              });
            } else {
              console.error('Product not found for id:', productId);
              this.errorMessage = 'Produs inexistent.';
            }
          },
          error: (err: any) => {
            console.error('Error fetching detailed products:', err);
            this.errorMessage = 'Eroare la încărcarea produselor.';
          }
        });
      } else {
        console.error('Invalid product ID param:', idParam);
        this.errorMessage = 'Produs invalid.';
      }
    });
  }

  // Metoda helper pentru a converti un obiect raw intr-un ProdusDTO
  private mapToProdusDTO(p: any): ProdusDTO {
    const temp: ProdusDTO = {
      ProdusId: p.produsId,
      Nume: p.nume,
      Descriere: p.descriere,
      EsteSprayText: p.esteSprayText,
      Imagine: p.imagine,
      Pret: p.pret,
      Valabil: p.valabil,
      Categorie: p.categorie,
      Vendor: p.vendor,
    };

    // Prelucrare imagine (uint8array vs string)
    if (temp.Imagine) {
      if (Array.isArray(temp.Imagine) && temp.Imagine.length > 0) {
        const uint8Array = new Uint8Array(temp.Imagine);
        temp.Imagine = 'data:image/png;base64,' + this.arrayBufferToBase64(uint8Array);
      } else if (typeof temp.Imagine === 'string') {
        if (!temp.Imagine.startsWith('data:image')) {
          temp.Imagine = 'data:image/png;base64,' + temp.Imagine;
        }
      } else {
        temp.Imagine = 'assets/logoR.png';
      }
    } else {
      temp.Imagine = 'assets/logoR.png';
    }
    return temp;
  }

  private arrayBufferToBase64(buffer: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < buffer.byteLength; i++) {
      binary += String.fromCharCode(buffer[i]);
    }
    return window.btoa(binary);
  }

  increaseQuantity(): void {
    if (this.quantity < this.totalSubproduseDisponibile) {
      this.quantity++;
      console.log('Quantity increased to:', this.quantity);
    } else {
      console.warn('Nu mai sunt subproduse disponibile.');
    }
  }

  decreaseQuantity(): void {
    if (this.quantity > 1) {
      this.quantity--;
      console.log('Quantity decreased to:', this.quantity);
    } else {
      console.warn('Cantitatea nu poate fi mai mică de 1.');
    }
  }

  addToCart(product: ProdusDTO): void {
    this.errorMessage = null;
    this.successMessage = null;

    if (this.quantity < 1) {
      this.errorMessage = "Cantitatea trebuie să fie minim 1.";
      return;
    }

    const userId = this.authService.user?.IdUser;
    if (!userId) {
      this.errorMessage = "Trebuie să fii autentificat pentru a adăuga în coș.";
      console.error('⛔ User nu e logat.');
      return;
    }

    // Verificam daca user are cos
    this.cosService.getCartByUserId(userId).subscribe({
      next: (existingCart: Cos) => {
        console.log('Coș existent găsit:', existingCart);
        // Adaugam subproduse (cantitatea) la cosul existent
        this.addMultipleSubproduse(existingCart.idCos, product.ProdusId, this.quantity, userId);
      },
      error: (err) => {
        if (err.status === 404) {
          // Nu exista cos => cream cos
          console.log('Utilizatorul nu are coș. Creăm unul nou...');
          const newCart: Cos = {
            idCos: 0,
            CodUnic: this.generateUniqueCartCode(),
            IdUser: userId,
            User: null,
            DataCreare: new Date()
          };
          this.cosService.createCart(newCart).subscribe({
            next: (createdCart: Cos) => {
              console.log('Coș creat cu succes:', createdCart);
              // Dupa crearea cosului, adaugam subprodusele
              this.addMultipleSubproduse(createdCart.idCos, product.ProdusId, this.quantity, userId);
            },
            error: (createError) => {
              this.errorMessage = "Eroare la crearea coșului.";
              console.error('Eroare la createCart:', createError);
            }
          });
        } else {
          console.error('Eroare la verificare coș:', err);
          this.errorMessage = "Eroare la verificarea coșului.";
        }
      }
    });
  }

  private addMultipleSubproduse(idCos: number, idProdus: number, quantity: number, userId: number): void {
    this.cosService.addSubproduseToCart(idCos, idProdus, quantity, userId).subscribe({
      next: (response: any) => {
        console.log(`✅ ${quantity} subproduse adăugate în coșul ${idCos}.`);
        this.successMessage = response.Message || `Am adăugat ${quantity} subproduse în coș.`;
        setTimeout(() => {
          // Poti redirectiona catre cos, catalog, etc.:
          this.router.navigate(['/catalog']);
        }, 2000);
      },
      error: (error: any) => {
        console.error('Eroare la adăugarea subproduselor în coș:', error);
        this.errorMessage = "Eroare la adăugarea subproduselor.";
      }
    });
  }

  generateUniqueCartCode(): string {
    return 'CART-' + Math.random().toString(36).substring(2, 10).toUpperCase();
  }
}
