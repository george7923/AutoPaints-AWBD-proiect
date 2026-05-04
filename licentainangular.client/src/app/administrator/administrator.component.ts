import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminComenziService } from '../services/admin-comenzi.service';
import { ProductService } from '../services/product.service';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service'; 

@Component({
  selector: 'app-administrator',
  templateUrl: './administrator.component.html',
  styleUrl: './administrator.component.css',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [AdminComenziService]
})
export class AdministratorComponent implements OnInit {
  users: any[] = [];
  userComenzi: { [userId: number]: any[] } = {};
  loading: boolean = false;
  error: string = "";
  produse: any[] = [];
  reducereProdusId: number | null = null;
  pretNou: number | null = null;
  dataExpirare: string | null = null;
  mesajReducere: string = "";
  toatePreturile: any[] = [];
  pretReducere: { pretNou: any, pretVechi: any } | null = null;
  produsSelectatId: number | null = null;
  produsEdit: any = null;
  categorii: string[] = [];
  imgPreview: string | null = null;
  cantitateSubprodus: number = 0;
  mesajProdus: string = "";
  roluri: string[] = ['Participant', 'Administrator'];
  roleUpdateStatus: { [userId: number]: string } = {};
  preturi: any[] = []; 
  preturiPeProdus: Map<number, any[]> = new Map();


  constructor(private adminService: AdminComenziService, private productService: ProductService, private AuthService: AuthService) { }

  ngOnInit(): void {
    this.loadUsersAndComenzi();
    this.loadProduse();
    this.loadCategorii();
    this.productService.getAllProducts().subscribe({
      next: (data) => {
        this.produse = data.filter(p => p.IdUser !== null);

        this.incarcaPreturi();
      },
      error: (err) => console.error('Eroare la preluarea produselor:', err)
    });


  }
  incarcaPreturi(): void {
    this.preturiPeProdus.clear();

    const iduri = this.extrageIdProduse();
    iduri.forEach(id => {
      this.productService.getPreturiByProdus(id).subscribe({
        next: (data) => {
          this.preturiPeProdus.set(id, data);
        },
        error: (err) => console.error(`Eroare la produs ${id}:`, err)
      });
    });
  }


  extrageIdProduse(): number[] {
    return this.produse.map(p => p.idProdus);
  }


  salveazaPret(pret: any): void {
    this.productService.updatePret(pret.idPP, {
      pret: pret.pret,
      comision: pret.comision,
      dataExpirare: pret.dataExpirare
    }).subscribe({
      next: () => alert('Preț salvat.'),
      error: (err) => alert('Eroare la salvare.')
    });
  }
  stergePret(pret: any): void {
    this.productService.deletePret(pret.idPP).subscribe({
      next: () => {
        this.preturi = this.preturi.filter(p => p.idPP !== pret.idPP);
      },
      error: () => alert('Eroare la ștergere')
    });
  }
  poateFiSters(idProdus: number, pret: any): boolean {
    const toate = this.preturiPeProdus.get(idProdus) || [];
    const faraExp = toate.filter(p => !p.dataExpirare);
    return faraExp.length > 1 || pret.dataExpirare !== null;
  }


  loadUsersAndComenzi() {
    this.loading = true;
    this.adminService.getAllUsers().subscribe({
      next: users => {
        this.users = users;
        users.forEach(user => {
          const userId = user.idUser;
          this.adminService.getComenziScurtByUser(userId).subscribe({
            next: comenzi => this.userComenzi[userId] = comenzi,
            error: err => {
              if (err.status === 404) {
                this.userComenzi[userId] = [];
              } else {
                this.userComenzi[userId] = [];
                this.error = "Eroare la încărcarea comenzilor!";
              }
            }
          });
        });
        this.loading = false;
      },
      error: () => {
        this.error = "Eroare la încărcarea utilizatorilor!";
        this.loading = false;
      }
    });
  }
  onLivrareChange(comanda: any, value: string) {
    const livrata = value === 'Da';
    if (comanda.livrata === livrata) return;
    this.adminService.marcheazaCaLivrata(comanda.idComanda, livrata).subscribe({
      next: (resp) => {
        comanda.livrata = livrata;
      },
      error: (err) => {
        alert('Eroare la actualizarea statusului de livrare!');
      }
    });
  }
  loadProduse() {
    this.productService.getAllProducts().subscribe({
      next: produse => this.produse = produse,
      error: () => this.produse = []
    });
  }

  aplicaReducere() {
    if (!this.reducereProdusId || !this.pretNou) {
      this.mesajReducere = "Selectează produsul și prețul redus!";
      return;
    }
    this.productService.adaugaReducere(this.reducereProdusId, this.pretNou, this.dataExpirare || undefined)
      .subscribe({
        next: () => {
          this.mesajReducere = "Reducerea a fost aplicată!";
          this.reducereProdusId = null;
          this.pretNou = null;
          this.dataExpirare = null;
        },
        error: () => this.mesajReducere = "Eroare la aplicarea reducerii!"
      });
  }
  get pretVechiSelectat(): number | null {
    if (!this.reducereProdusId) return null;
    const produs = this.produse.find(p => p.idProdus === this.reducereProdusId);
    if (!produs) return null;

    // Daca produsul are array cu istoric preturi:
    if (produs.preturi && produs.preturi.length > 0) {
      // Pretul curent este cel cu dataExpirare == null
      const curent = produs.preturi.find((p: any) => p.dataExpirare == null);

      // Pretul vechi este cel mai recent cu dataExpirare != null
      const vechi = produs.preturi
        .filter((p: PretProdus) => p.dataExpirare != null)
        .sort((a: PretProdus, b: PretProdus) =>
          new Date(b.dataExpirare!).getTime() - new Date(a.dataExpirare!).getTime()
        )[0];
    }

    // Sau daca avem doar pret direct:
    return produs.pret || null;
  }
  onSelectProdusReducere(idProdus: number) {
    this.reducereProdusId = idProdus;
    this.productService.getReducereProdus(idProdus).subscribe({
      next: data => this.pretReducere = data,
      error: () => this.pretReducere = null
    });
  }
  loadCategorii() {
    this.productService.getAllCategories().subscribe({
      next: categorii => this.categorii = categorii.map((c: any) => c.denumireCategorie || c.DenumireCategorie),
      error: () => this.categorii = []
    });
  }

  // Selectezi un produs pentru editare
  onSelectProdusEdit(id: number) {
    if (!id) { this.produsEdit = null; return; }
    this.productService.getProductById(id).subscribe({
      next: produs => {
        this.produsEdit = { ...produs };
        this.imgPreview = produs.Imagine ? this.toImageSrc(produs.Imagine) : null;
        // (optional) Incarci si cantitatea subproduselor separat, daca ai endpoint
        this.productService.getSubprodusCountByProdusId(id).subscribe({
          next: c => this.cantitateSubprodus = c,
          error: () => this.cantitateSubprodus = 0
        });
      },
      error: () => { this.produsEdit = null; }
    });
  }

  onImagineChange(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imgPreview = e.target.result;
    };
    reader.readAsDataURL(file);
    this.produsEdit._file = file;
  }
  stergeImagine() {
    this.imgPreview = null;
    this.produsEdit.imagine = null;
    this.produsEdit._file = null;
  }



  //Il face doar indisponibil pe stoc
  stergeProdus() {
    if (!confirm('Sigur vrei să ștergi produsul?')) return;
    this.productService.deleteProduct(this.produsEdit.idProdus).subscribe({
      next: () => {
        this.mesajProdus = "Produs șters!";
        this.loadProduse();
        this.produsEdit = null;
        this.produsSelectatId = null;
      },
      error: () => this.mesajProdus = "Eroare la ștergere!"
    });
  }

  // Helper pt. imagine base64
  toImageSrc(img: any): string {
    if (!img) return 'assets/logoR.png';
    if (typeof img === 'string' && img.startsWith('data:image')) return img;
    return 'data:image/png;base64,' + img;
  }
  getProductImage(prod: any) {
    return this.toImageSrc(prod.imagine || prod.Imagine);
  }

  updateProdus() {
    const idProdus = this.produsEdit?.idProdus;
    if (!idProdus) {
      this.mesajProdus = "Selectează un produs!";
      return;
    }


    const dto: any = {};

    // Nume produs
    if (this.produsEdit.nume != null && this.produsEdit.nume !== '') {
      dto.nume = this.produsEdit.nume;
    }
    // Descriere
    if (this.produsEdit.descriere != null && this.produsEdit.descriere !== '') {
      dto.descriere = this.produsEdit.descriere;
    }
    // EsteSpray
    if (typeof this.produsEdit.esteSpray === 'boolean') {
      dto.esteSpray = this.produsEdit.esteSpray;
    }
    // Valabil
    if (typeof this.produsEdit.valabil === 'boolean') {
      dto.valabil = this.produsEdit.valabil;
    }
    // DenumireCategorie (fortat litere mici)
    if (this.produsEdit.denumireCategorie != null && this.produsEdit.denumireCategorie !== '') {
      dto.denumireCategorie = this.produsEdit.denumireCategorie.toLowerCase();
    }
    // IdUser
    if (this.produsEdit.idUser != null && this.produsEdit.idUser !== '') {
      dto.idUser = this.produsEdit.idUser;
    }
    // Imagine (Base64, dacă există preview nou)
    if (this.imgPreview) {
      // Extrage doar conținutul base64, fără prefix
      dto.imagineBase64 = this.imgPreview.replace(/^data:image\/(png|jpeg);base64,/, '');
    }
    // Cantitate subproduse
    if (typeof this.cantitateSubprodus === 'number' && !isNaN(this.cantitateSubprodus)) {
      dto.cantitate = this.cantitateSubprodus;
    }

    // Daca nu s-a modificat nimic, nu trimite request
    if (Object.keys(dto).length === 0) {
      this.mesajProdus = "Nu ai modificat niciun câmp!";
      return;
    }

    this.productService.adminUpdateProdus(idProdus, dto).subscribe({
      next: () => this.mesajProdus = "Produs actualizat cu succes!",
      error: () => this.mesajProdus = "Eroare la actualizare!"
    });
  }

  onRoleChange(user: any, newRole: string) {
    if (!newRole) return;

    this.AuthService.updateUserRole(user.idUser, newRole).subscribe({
      next: () => {
        this.roleUpdateStatus[user.idUser] = '✅ Rol actualizat!';
        setTimeout(() => this.roleUpdateStatus[user.idUser] = '', 3000);
      },
      error: () => {
        this.roleUpdateStatus[user.idUser] = '❌ Eroare la actualizare!';
        setTimeout(() => this.roleUpdateStatus[user.idUser] = '', 3000);
      }
    });
  }
  getRolUser(): string {
    const userInfo = this.AuthService.getUserInfo ? this.AuthService.getUserInfo() : this.AuthService.user;
    if (userInfo) {
      if (userInfo.persoana && userInfo.persoana.rol) return userInfo.persoana.rol;
      if (userInfo.Persoana && userInfo.Persoana.Rol) return userInfo.Persoana.Rol;
      if (userInfo.rol) return userInfo.rol;
      if (userInfo.Rol) return userInfo.Rol;
    }
    return '';
  }

  get usersFaraOwner(): any[] {
    return this.users.filter(u =>

      u?.username !== 'admin' // Exclude admin user
    );
  }

}
interface PretProdus {
  pret: number;
  dataExpirare: string | null;

}
