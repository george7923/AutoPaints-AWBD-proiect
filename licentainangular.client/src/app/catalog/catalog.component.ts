import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../services/product.service';
import { CategoryService } from '../services/category.service';
import { ProdusDTO, CategoryDTO } from '../models/NewDTOs';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.css'],
})
export class CatalogComponent implements OnInit {
  allProducts: (ProdusDTO & { pretVechi?: number, pretReducere?: number })[] = [];
  filteredProducts: (ProdusDTO & { pretVechi?: number, pretReducere?: number })[] = [];

  selectedCategory: string = '';
  categories: string[] = [];

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  
   //Incarca produsele din backend
  loadProducts(): void {
    this.productService.getDetailedProducts().subscribe({
      next: (products) => {
        const asAnyArray = products as any[];

        // 1. Iancarca reducerile si le mapeaza la produse
        this.productService.getProduseCuReducere().subscribe({
          next: (reduceri) => {
            this.allProducts = asAnyArray.map((p: any) => {
              // Mapare imagine
              if (!p.Imagine && !p.imagine) {
                p.Imagine = 'assets/logoR.png';
              } else if (p.imagine && !p.Imagine) {
                p.Imagine = p.imagine;
              }
              if (Array.isArray(p.Imagine) && p.Imagine.length > 0) {
                const uint8Array = new Uint8Array(p.Imagine);
                p.Imagine = 'data:image/png;base64,' + this.arrayBufferToBase64(uint8Array);
              } else if (typeof p.Imagine === 'string' && p.Imagine && !p.Imagine.startsWith('data:image')) {
                p.Imagine = 'data:image/png;base64,' + p.Imagine;
              }

              //  Mapare chei mari/mici 
              if (p.nume && !p.Nume) { p.Nume = p.nume; }
              if (p.descriere && !p.Descriere) { p.Descriere = p.descriere; }
              if (p.categorie && !p.Categorie) { p.Categorie = p.categorie; }
              if (p.vendor && !p.Vendor) { p.Vendor = p.vendor; }
              if (p.pret && !p.Pret) { p.Pret = p.pret; }
              if (p.produsId && !p.ProdusId) { p.ProdusId = p.produsId; }

              //  Integrare reduceri
              const reducere = reduceri.find((r: any) =>
                (r.idProdus === p.ProdusId || r.idProdus === p.produsId)
              );
              if (reducere) {
                p.pretVechi = reducere.pretVechi;
                p.pretReducere = reducere.pretReducere ?? reducere.pretReducere; // accepta ambele chei
              }

              return p as ProdusDTO & { pretVechi?: number, pretReducere?: number };
            });

            this.filteredProducts = this.allProducts;
          },
          error: (err) => {
            // Daca nu merge endpointul de reduceri, arata produsele fara reduceri
            this.allProducts = asAnyArray;
            this.filteredProducts = this.allProducts;
          }
        });
      },
      error: (err) => {
        console.error('Eroare la încărcarea produselor:', err);
      },
    });
  }


  //Incarca categoriile din backend
  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (data: CategoryDTO[]) => {
        console.log('Received categories:', data);
        this.categories = data.map(cat => cat.denumireCategorie);
      },
      error: (err) => {
        console.error('Eroare la încărcarea categoriilor:', err);
      },
    });
  }

  filterByCategory(): void {
    if (this.selectedCategory) {
      this.filteredProducts = this.allProducts.filter(
        (product) => product.Categorie === this.selectedCategory
      );
    } else {
      this.filteredProducts = this.allProducts;
    }
  }

  navigateToProduct(product: ProdusDTO): void {

    const id = product.ProdusId || 0;
    this.router.navigate(['/product', id]);
  }

  navigateToVopsea(): void {
    this.router.navigate(['/vopsea-auto-preparata-dupa-codul-de-culoare']);
  }

  private arrayBufferToBase64(buffer: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < buffer.byteLength; i++) {
      binary += String.fromCharCode(buffer[i]);
    }
    return window.btoa(binary);
  }
}
