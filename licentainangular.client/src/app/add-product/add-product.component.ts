import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { ProductService } from '../services/product.service';
import { SubprodusService } from '../services/subprodus.service';
import { AuthService } from '../services/auth.service';
import { CategoryService } from '../services/category.service';
import { CategoryDTO, ProdusIncarcatDTO } from '../models/NewDTOs';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.css']
})
export class AddProductComponent implements OnInit {
  // Inițializam produsul folosind  DTO-ul ProdusIncarcatDTO
  product: ProdusIncarcatDTO = {
    Nume: '',
    Descriere: '',
    EsteSprayText: '',
    Imagine: null,
    Pret: 0,
    Valabil: true,
    Categorie: '',
    Vendor: '',
    IdUser: 0 // Va fi setat din AuthService
  };

  spraySelection: string = '';
  categories: CategoryDTO[] = [];
  newCategory: string = '';
  subprodusCount: number = 0;

  imageFile: File | null = null;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private productService: ProductService,
    private subprodusService: SubprodusService,
    private authService: AuthService,
    private categoryService: CategoryService,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.categoryService.getAllCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Eroare la getAllCategories:', err);
      }
    });
  }

  handleImageInput(event: any): void {
    const file = event.target.files[0] || null;
    this.imageFile = file;
    if (file) {
      //citim base64:
      const reader = new FileReader();
      reader.onload = () => {
        this.product.Imagine = null;

      };
      reader.readAsDataURL(file);
    }
  }

  addProduct(): void {
    this.successMessage = null;
    this.errorMessage = null;

    // Verificam permisiunile
    if (
      !this.authService.user ||
      (this.authService.user?.Persoana?.tipPersoana === 'Fizica' &&
        this.authService.user?.Username !== 'admin')
    ) {
      this.errorMessage = 'Nu aveți permisiunea să adăugați un produs!';
      return;
    }

    // Convertim "Spray"/"Non-spray" => 'DA' / 'NU'
    this.product.EsteSprayText =
      this.spraySelection.toLowerCase() === 'spray' ? 'DA' : 'NU';

    // Adaugam ID-ul userului curent in produs
    this.product.IdUser = this.authService.user.IdUser;

    // Verificam daca userul introduce o categorie noua
    if (this.newCategory.trim().length > 0) {
      const newCatLower = this.newCategory.trim().toLowerCase();
      const alreadyExists = this.categories.some(
        cat => cat.denumireCategorie?.toLowerCase() === newCatLower
      );

      if (alreadyExists) {
        // Daca exista deja, folosim direct
        this.product.Categorie = newCatLower;
        this.createProdus();
      } else {
        // Cream mai intai categoria noua
        const catDTO: CategoryDTO = {
          denumireCategorie: newCatLower,
          descriereCategorie: 'Categorie creată automat'
        };

        this.categoryService.addCategory(catDTO).subscribe({
          next: (resp) => {
            // Odata creata categoria, reincarcam listele:
            this.categoryService.getAllCategories().subscribe({
              next: allCats => {
                this.categories = allCats;
                this.product.Categorie = newCatLower;
                this.createProdus();
              },
              error: e => {
                console.error('Eroare reload categories:', e);
                this.errorMessage = 'Eroare la reîncărcarea categoriilor.';
              }
            });
          },
          error: (err) => {
            console.error('Eroare la crearea categoriei:', err);
            let msg = '';
            if (err.error?.message) {
              msg = err.error.message;
            } else if (err.status) {
              msg = `HTTP ${err.status} - ${err.statusText} (url: ${err.url})`;
            } else {
              msg = JSON.stringify(err);
            }
            this.errorMessage = 'Eroare la crearea categoriei: ' + msg;
          }
        });
      }
    } else {
      // Daca nu a introdus nimic in newCategory, folosim selectul existent
      if (this.product.Categorie) {
        this.product.Categorie = this.product.Categorie.toLowerCase();
      }
      this.createProdus();
    }
  }

  // Funcaia care trimite efectiv produsul catre backend
  private createProdus(): void {
    // Folosim FormData pentru a trimite imaginea sub forma de fisier
    const formData = new FormData();
    formData.append('nume', this.product.Nume);
    formData.append('descriere', this.product.Descriere || '');
    formData.append('esteSprayText', this.product.EsteSprayText);
    formData.append('pret', String(this.product.Pret));
    formData.append('valabil', String(this.product.Valabil));
    formData.append('categorie', this.product.Categorie);
    formData.append('vendor', this.product.Vendor || '');
    formData.append('idUser', String(this.product.IdUser));

    if (this.imageFile) {
      formData.append('imagine', this.imageFile, this.imageFile.name);
    }

    // Apelam serviciul care trimite "formData" la backend
    this.productService.createProduct(formData).subscribe({
      next: (created) => {
        if (!created?.idProdus) {
          this.errorMessage = 'Produsul a fost creat, dar nu s-a întors un ID valid.';
          return;
        }

        // Daca avem subproduse de creat
        if (this.subprodusCount > 0) {
          for (let i = 0; i < this.subprodusCount; i++) {
            const newSub = {
              idProdus: created.idProdus,
              valabil: true
            };
            this.subprodusService.createSubprodus(newSub).subscribe({
              next: subResp => console.log('Subprodus creat:', subResp),
              error: subErr => console.error('Eroare subprodus:', subErr)
            });
          }
        }

        this.successMessage = 'Produs adăugat cu succes!';
        setTimeout(() => this.router.navigate(['/catalog']), 1500);
      },
      error: (err) => {
        console.error('Eroare la crearea produsului:', err);
        this.errorMessage = err?.error?.message || 'Eroare la crearea produsului.';
      }
    });
  }
}
