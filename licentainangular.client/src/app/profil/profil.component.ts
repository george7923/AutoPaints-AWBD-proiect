import { Component, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ProductService } from '../services/product.service'; 

import {
  Stripe,
  StripeCardElement,
  StripeCardCvcElement,
  StripeCardExpiryElement,
  StripeCardNumberElement,
  loadStripe
} from '@stripe/stripe-js';
// Servicii
import { AuthService } from '../services/auth.service';
import { PersoanaService } from '../services/persoana.service';
import { CardService } from '../services/card.service';
import { AdresaService } from '../services/adresa.service';

// DTO-uri
import { AdresaNestedDTO, UserAddressDTO } from '../models/NewDTOs';
import { AdminUpdateProdusDTO } from '../models/DTO_Admin';

@Component({
  selector: 'app-profil',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './profil.component.html',
  styleUrls: ['./profil.component.css'],
})
export class ProfilComponent implements OnInit {
  // -- date user & card
  user: any;
  card: any;
  rolUser: string = '';

  userEdit: any;
  cardEdit: any;
  activeTab: string = 'user';
  editingUser: boolean = false;
  editingCard: boolean = false;

  // -- adrese
  addresses: any[] = [];

  edAddressId: number | null = null;
  stripe: Stripe | null = null;
  cardNumber!: StripeCardNumberElement;
  cardExpiry!: StripeCardExpiryElement;
  cardCvc!: StripeCardCvcElement;
  cardErrors: string | null = null;
  cardBrand: string | null = null;
  cardLast4: string = '';
  cards: any[] = [];
  addingCard: boolean = false;
  produse: any[] = [];
  produsSelectatId: number | null = null;
  produsEdit: any = null;
  imgPreview: string | null = null;
  cantitateSubprodus: number | null = null;
  mesajProdus: string = '';
  categorii: string[] = [];

  // Formular (Nested)
  addressForm = {
    Bloc: '',
    Scara: '',
    Etaj: '',
    Apartament: '',
    DenumireStrada: '',
    NrStrada: '',
    DenumireLocalitate: '',
    DenumireJudet: '',
    DenumireTara: ''
  };
  editingAddress: boolean = false;
  manualCard = {
    numarCard: '',
    expiry: '',
    cvv: ''
  };
  produseJuridice: any[] = [];
  parolaError: string = '';
  reducereProdusId: number | null = null;
  pretNou: number | null = null;
  dataExpirare: string | null = null;
  mesajReducere: string = "";
  pretReducere: any = null;
  preturi: any[] = []; 
  preturiPeProdus: Map<number, any[]> = new Map();
  constructor(
    public router: Router,
    private authService: AuthService,
    private persService: PersoanaService,
    private cardService: CardService,
    private adresaService: AdresaService,
    private productService: ProductService
  ) { }

  ngOnInit(): void {
    // user & card from Auth
    const userInfo = this.authService.getUserInfo
      ? this.authService.getUserInfo()
      : this.authService.user;

    if (userInfo) {
      // Extrage Persoana din user, indiferent de denumire (Persoana/persoana)
      let persoana = userInfo.persoana || userInfo.Persoana || userInfo;
      this.rolUser = persoana.rol || persoana.Rol || '';
      // Acum ai rolul într-un string accesibil oricand!
    } else {
      this.rolUser = '';
    }
    this.user = this.authService.user;
    this.card = this.authService.card;
    console.log(this.user.Persoana);
    console.log(this.rolUser);


    this.userEdit = this.cloneObject(this.user);
    this.cardEdit = this.cloneObject(this.card);

    // daca e logat
    if (this.user?.IdUser) {
      this.loadAddresses(this.user.IdUser);
      this.loadProduseForUser(this.user.IdUser);
    }

    //this.loadProduse();
    this.refreshTabData();
    this.incarcaPreturi();
  }

  private cloneObject(obj: any): any {
    return JSON.parse(JSON.stringify(obj));
  }
  refreshTabData() {
    // Adrese si carduri
    if (this.user?.IdUser) {
      this.loadAddresses(this.user.IdUser);
      this.loadCards(this.user.IdUser);
    }

    // Produse doar daca e juridic SAU administrator
    if (this.user?.Persoana?.TipPersoana === 'juridica' || this.user?.Persoana?.Rol === 'Administrator') {
      let filterId = this.user.Persoana.Rol === 'Administrator' ? 1 : this.user.IdUser;
      this.loadProduseForUser(filterId);
    }
  }


  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
  addProdus() {
    this.router.navigate(['/add-product']);
  }

  // -- user & card
  toggleEditUser(): void {
    if (!this.editingUser) {
      this.userEdit = this.cloneObject(this.user);
      this.userEdit.Password = '';  // Parola mereu goala la edit!
    }
    this.editingUser = !this.editingUser;
    this.parolaError = ''; // Goleste eroarea la fiecare deschidere edit
  }

  applyUserChanges(): void {
    const idUser = this.user?.IdUser;
    if (!idUser) return;

    this.parolaError = ''; // Goleste vechiul mesaj de eroare

    const dto: any = {};

    if (this.userEdit.Username && this.userEdit.Username !== this.user.Username)
      dto.username = this.userEdit.Username;

    if (this.userEdit.Persoana?.Nume && this.userEdit.Persoana.Nume !== this.user.Persoana.Nume)
      dto.nume = this.userEdit.Persoana.Nume;

    if (this.userEdit.Persoana?.Prenume && this.userEdit.Persoana.Prenume !== this.user.Persoana.Prenume)
      dto.prenume = this.userEdit.Persoana.Prenume;

    if (this.userEdit.Persoana?.Email && this.userEdit.Persoana.Email !== this.user.Persoana.Email)
      dto.email = this.userEdit.Persoana.Email;

    if (this.userEdit.Persoana?.Telefon && this.userEdit.Persoana.Telefon !== this.user.Persoana.Telefon)
      dto.telefon = this.userEdit.Persoana.Telefon;

    // Validare și update parola daca a fost completata
    if (this.userEdit.Password && this.userEdit.Password.trim() !== '') {
      // Regex: min 8, litere mari, mici, cifre
      const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
      if (!regex.test(this.userEdit.Password)) {
        this.parolaError = 'Parola trebuie să aibă minim 8 caractere, o literă mare, una mică și o cifră.';
        return; // Nu trimite nimic la backend
      }
      dto.password = this.userEdit.Password;
    }

    if (Object.keys(dto).length === 0) {
      this.editingUser = false;
      return;
    }

    this.authService.updateDetaliiUser(idUser, dto).subscribe({
      next: () => {
        this.user = { ...this.user, ...this.userEdit };
        this.editingUser = false;
        this.userEdit.Password = ''; // Goleste parola si dupa submit
      },
      error: (err) => {
        console.error('Eroare la update profil:', err);
      }
    });
  }



  updatePersoana(): void {
    this.persService.updatePersoanaById(this.user.IdPersoana, this.user.Persoana).subscribe({
      next: (resp) => console.log('Persoana actualizată cu succes', resp),
      error: (err) => console.error('Eroare la update Persoana', err),
    });
  }

  toggleEditCard(): void {
    if (!this.editingCard) {
      this.cardEdit = this.cloneObject(this.card);
    }
    this.editingCard = !this.editingCard;
  }

  applyCardChanges(): void {
    this.card = this.cloneObject(this.cardEdit);
    this.editingCard = false;
    this.updateCard();
  }

  updateCard(): void {
    this.cardService.updateCardsByUserId(this.user.IdUser, this.card).subscribe({
      next: (resp) => console.log('Card actualizat', resp),
      error: (err) => console.error('Eroare la update card', err),
    });
  }

  // -- adrese
  loadAddresses(userId: number): void {
    this.adresaService.getSimplifiedAddressesByUser(userId).subscribe({
      next: (resp: any[]) => {
        this.addresses = resp || [];
        console.log('Adrese:', this.addresses);
      },
      error: (err) => {
        this.addresses = [];
        console.error('Eroare la încărcarea adreselor:', err);
      }
    });
  }


  loadCards(idUser: number) {
    this.cardService.getCardsByUser(idUser).subscribe({
      next: (resp) => this.cards = resp || [],
      error: (err) => { this.cards = []; }
    });
  }

  loadProduse() {
    const idUserLogat = this.user?.IdUser;
    const rolUser = this.rolUser;
    this.productService.getAllProducts().subscribe({
      next: produse => {
        let produseFiltrate: any[] = [];

        if (rolUser === 'Administrator' || rolUser === 'Owner') {
          produseFiltrate = produse.filter((p: any) => p.idUser === 1);
        } else if (idUserLogat) {
          produseFiltrate = produse.filter((p: any) => p.idUser === idUserLogat);
        }

        this.produseJuridice = produseFiltrate;

        // DEBUG: Printează produsele filtrate pentru user
        console.log('ProduseJuridice:', this.produseJuridice);
        console.log('Toate produsele:', produse);
        console.log('User curent:', this.user);
        console.log('Rol user:', this.rolUser);
      },
      error: () => {
        this.produseJuridice = [];
        console.log('Eroare la încărcarea produselor!');
      }
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
          this.loadProduse();
        },
        error: () => this.mesajReducere = "Eroare la aplicarea reducerii!"
      });
  }
  get pretVechiSelectat(): number | null {
    if (!this.reducereProdusId) return null;
    const produs = this.produse.find(p => p.idProdus === this.reducereProdusId);
    if (!produs) return null;
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
  onSelectProdusEdit(id: number) {
    if (!id) { this.produsEdit = null; return; }
    this.productService.getProductById(id).subscribe({
      next: produs => {
        this.produsEdit = { ...produs };
        this.imgPreview = produs.Imagine ? this.toImageSrc(produs.Imagine) : null;
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
  toImageSrc(img: any): string {
    if (!img) return 'assets/logoR.png';
    if (typeof img === 'string' && img.startsWith('data:image')) return img;
    return 'data:image/png;base64,' + img;
  }
  getProductImage(prod: any): string {
    // Accepta img ca string, base64, Uint8Array sau null
    let img = prod.imagine || prod.Imagine;
    if (!img) return 'assets/logoR.png';
    if (typeof img === 'string' && img.startsWith('data:image')) return img;
    if (img instanceof Uint8Array || (Array.isArray(img) && typeof img[0] === 'number')) {
      return 'data:image/png;base64,' + this.arrayBufferToBase64(new Uint8Array(img));
    }
    return 'data:image/png;base64,' + img; // fallback
  }

  private arrayBufferToBase64(buffer: Uint8Array): string {
    let binary = '';
    for (let i = 0; i < buffer.byteLength; i++) {
      binary += String.fromCharCode(buffer[i]);
    }
    return window.btoa(binary);
  }
  produsEditId: number | null = null; // Id-ul produsului editat

  editProdus(prod: any) {
    this.produsEditId = prod.idProdus;

    // Toate campurile la null/gol
    this.produsEdit = {
      idProdus: prod.idProdus,
      nume: '',
      descriere: '',
      denumireCategorie: '',
      esteSpray: false,
      valabil: false,
      idUser: prod.idUser || null
    };
    this.cantitateSubprodus = null;
    this.imgPreview = null;
  }


  updateProdus() {
    const idProdus = this.produsEdit?.idProdus;
    if (!idProdus) {
      this.mesajProdus = "Selectează un produs!";
      return;
    }

    // Construieste DTO doar cu campuri nenule/definite
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
      // Extrage doar continutul base64, fara prefix
      dto.imagineBase64 = this.imgPreview.replace(/^data:image\/(png|jpeg);base64,/, '');
    }
    // Cantitate subproduse
    if (typeof this.cantitateSubprodus === 'number' && !isNaN(this.cantitateSubprodus)) {
      dto.cantitate = this.cantitateSubprodus;
    }

    // Dacq nu s-a modificat nimic, nu trimite request
    if (Object.keys(dto).length === 0) {
      this.mesajProdus = "Nu ai modificat niciun câmp!";
      return;
    }

    this.productService.adminUpdateProdus(idProdus, dto).subscribe({
      next: () => this.mesajProdus = "Produs actualizat cu succes!",
      error: () => this.mesajProdus = "Eroare la actualizare!"
    });
  }


  loadProduseForUser(idUser: number) {
    this.productService.getAllProducts().subscribe({
      next: (prods: any[]) => {
        this.produseJuridice = prods.filter(p => p.idUser === idUser);
        this.incarcaPreturi();
      }
    });
  }

  resetAddressForm(): void {
    this.addressForm = {
      Bloc: '',
      Scara: '',
      Etaj: '',
      Apartament: '',
      DenumireStrada: '',
      NrStrada: '',
      DenumireLocalitate: '',
      DenumireJudet: '',
      DenumireTara: ''
    };
  }

  addAddress(): void {
    this.editingAddress = true;
    this.edAddressId = null;
    this.resetAddressForm();
  }

  editAddress(addr: any): void {
    this.editingAddress = true;
    this.edAddressId = addr.idAdresa;

    this.addressForm = {
      Bloc: addr.Bloc || '',
      Scara: addr.Scara || '',
      Etaj: addr.Etaj || '',
      Apartament: addr.Apartament || '',
      DenumireStrada: addr.Strazi?.DenumireStrada || '',
      NrStrada: addr.Strazi?.Nr?.toString() || '',
      DenumireLocalitate: addr.Strazi?.Localitati?.DenumireLocalitate || '',
      DenumireJudet: addr.Strazi?.Localitati?.Judete?.DenumireJudet || '',
      DenumireTara: addr.Strazi?.Localitati?.Judete?.Tari?.DenumireTara || ''
    };
  }

  saveAddress(): void {
    const payload: AdresaNestedDTO = {
      Bloc: this.addressForm.Bloc,
      Scara: this.addressForm.Scara,
      Etaj: this.addressForm.Etaj,
      Apartament: this.addressForm.Apartament,
      Strazi: {
        DenumireStrada: this.addressForm.DenumireStrada,
        Nr: parseInt(this.addressForm.NrStrada, 10) || 0,
        Localitati: {
          DenumireLocalitate: this.addressForm.DenumireLocalitate,
          Judete: {
            DenumireJudet: this.addressForm.DenumireJudet,
            Tari: {
              DenumireTara: this.addressForm.DenumireTara
            }
          }
        }
      }
    };

    if (!this.edAddressId) {
      this.adresaService.createAdresaForUser(this.user.IdUser, payload).subscribe({
        next: (resp) => {
          if (resp && resp.idAdresa) {
            const associationPayload: UserAddressDTO = {
              idUser: this.user.IdUser,
              idAdresa: resp.idAdresa
            };
            // aici apelam assignAddressToUser (actualizat in serviciu cu / la final)
            this.adresaService.assignAddressToUser(associationPayload).subscribe({
              next: (assignResp) => {
                console.log("Asociere user-adresă reușită:", assignResp);
                this.loadAddresses(this.user.IdUser);
                this.editingAddress = false;
              },
              error: (errAssoc) => {
                console.error("Eroare la assignAddressToUser:", errAssoc);
              }
            });
          } else {
            console.warn("Nu am primit idAdresa => nu pot face assign");
          }
        },
        error: (err) => console.error("Eroare la createAdresaForUser:", err),
      });
    } else {
      this.adresaService.updateAdresaNestedById(this.edAddressId, payload).subscribe({
        next: (resp) => {
          console.log("Adresa actualizată:", resp);
          this.loadAddresses(this.user.IdUser);
          this.editingAddress = false;
        },
        error: (err) => console.error("Eroare la update adrese:", err)
      });
    }
  }


  deleteAddress(idAdr: number): void {
    if (!idAdr) return;
    if (confirm("Sigur doriți să ștergeți această adresă?")) {
      this.adresaService.deleteAdresa(idAdr).subscribe({
        next: (resp) => {
          console.log("Adresă ștearsă:", resp);
          this.loadAddresses(this.user.IdUser);
        },
        error: (err) => console.error("Eroare la ștergerea adresei:", err)
      });
    }
  }

  setDefaultAddress(addr: any): void {
    this.user.IdAdresa = addr.idAdresa;
    console.log("Adresă setată ca implicită:", addr.idAdresa);

  }
  startAddingCard(): void {
    this.addingCard = true;
  }
  /*
  onCardNumberInput(event: Event): void {
    const input = (event.target as HTMLInputElement);
    let value = input.value.replace(/\D/g, '').substring(0, 16);

    const parts = [];
    for (let i = 0; i < value.length; i += 4) {
      parts.push(value.substring(i, i + 4));
    }

    input.value = parts.join(' ');
    this.manualCard.numarCard = input.value;

    this.detectCardBrand(value);
  }
  */
  onCardNumberInput(event: any): void {
    let value = event.target.value.replace(/\D/g, ''); // doar cifre
    value = value.slice(0, 16); // max 16 cifre
    // Formateaza cu spatiu la fiecare 4 cifre (ex: 1234 5678 9012 3456)
    const groups = [];
    for (let i = 0; i < value.length; i += 4) {
      groups.push(value.slice(i, i + 4));
    }
    event.target.value = groups.join(' ');
    this.manualCard.numarCard = event.target.value;
  }

  onExpiryInput(event: Event): void {
    const input = (event.target as HTMLInputElement);
    let value = input.value.replace(/\D/g, '').substring(0, 4);

    if (value.length >= 3) {
      input.value = value.substring(0, 2) + '/' + value.substring(2);
    } else {
      input.value = value;
    }

    this.manualCard.expiry = input.value;
  }

  /*
  onExpiryInput(event: any): void {
  let value = event.target.value.replace(/\D/g, '').slice(0, 4);
  if (value.length >= 3) {
    value = value.slice(0, 2) + '/' + value.slice(2);
  }
  event.target.value = value;
  this.manualCard.expiry = value;
}

  */
 /*
  onCardNumberInput(event: any): void {
    let value = event.target.value.replace(/\D/g, ''); // doar cifre
    value = value.slice(0, 16); // max 16 cifre
    // Formatează cu spațiu la fiecare 4 cifre (ex: 1234 5678 9012 3456)
    const groups = [];
    for (let i = 0; i < value.length; i += 4) {
      groups.push(value.slice(i, i + 4));
    }
    event.target.value = groups.join(' ');
    this.manualCard.numarCard = event.target.value;
  }*/

  detectCardBrand(number: string): void {
    const firstDigit = number.charAt(0);
    const firstTwo = number.substring(0, 2);
    const firstFour = number.substring(0, 4);

    if (/^4/.test(number)) {
      this.cardBrand = 'Visa';
    } else if (/^5[1-5]/.test(number)) {
      this.cardBrand = 'MasterCard';
    } else if (/^3[47]/.test(number)) {
      this.cardBrand = 'American Express';
    } else if (/^6(?:011|5)/.test(number)) {
      this.cardBrand = 'Discover';
    } else {
      this.cardBrand = null;
    }
  }

  


  cancelCardForm(): void {
    this.addingCard = false;
    this.cardErrors = null;
    this.cardBrand = '';
    this.cardLast4 = '';
  }
  ngAfterViewInit(): void {
    loadStripe('pk_test_YOUR_PUBLIC_KEY').then((stripeInstance) => {
      this.stripe = stripeInstance;
      if (!this.stripe) return;

      const elements = this.stripe.elements();

      this.cardNumber = elements.create('cardNumber');
      this.cardNumber.mount('#card-number-element');

      this.cardExpiry = elements.create('cardExpiry');
      this.cardExpiry.mount('#card-expiry-element');

      this.cardCvc = elements.create('cardCvc');
      this.cardCvc.mount('#card-cvc-element');

      // doar pentru mesaje de eroare
      this.cardNumber.on('change', (event) => {
        this.cardErrors = event.error?.message || null;
        this.cardBrand = event.brand || '';
      });
    });
  }



  createAndAssignCard(): void {
    

    // 1. Validare numar card: 16 cifre, doar cifre
    const numarCardCurat = this.manualCard.numarCard.replace(/\D/g, '');
    if (!/^\d{16}$/.test(numarCardCurat)) {
      this.cardErrors = 'Numărul cardului trebuie să aibă exact 16 cifre.';
      return;
    }

    // 2. Validare data expirare: MM/YY
    const expiry = this.manualCard.expiry.replace(/\s/g, '');
    const match = /^(\d{2})\/(\d{2})$/.exec(expiry);
    if (!match) {
      this.cardErrors = 'Data expirării trebuie să fie în format MM/YY.';
      return;
    }
    const mm = parseInt(match[1], 10);
    const yy = parseInt(match[2], 10) + 2000; // ex: '27' -> 2027
    if (mm < 1 || mm > 12) {
      this.cardErrors = 'Luna expirării trebuie să fie între 01 și 12.';
      return;
    }
    // Sa nu fie in trecut:
    const today = new Date();
    const cardDate = new Date(yy, mm - 1, 1);
    if (cardDate < new Date(today.getFullYear(), today.getMonth(), 1)) {
      this.cardErrors = 'Cardul este expirat.';
      return;
    }

    // 3. Validare CVV: exact 3 cifre, doar cifre (Stripe-style)
    if (!/^\d{3}$/.test(this.manualCard.cvv)) {
      this.cardErrors = 'CVV-ul trebuie să conțină exact 3 cifre.';
      return;
    }

    //Daca am trecut toate validarile
    const dataExpirare = new Date(yy, mm - 1, 1).toISOString();

    const newCard = {
      numarCard: numarCardCurat,
      cvv: this.manualCard.cvv,
      dataExpirare: dataExpirare
    };

    this.cardService.createCard(newCard).subscribe({
      next: (created) => {
        const link = {
          idUser: this.user.IdUser,
          idCard: created.idCard
        };

        this.cardService.assignCardToUser(link).subscribe({
          next: () => {
            this.loadCards(this.user.IdUser);
            this.cancelCardForm();
            this.manualCard = { numarCard: '', expiry: '', cvv: '' };
            this.cardBrand = null;
            this.cardErrors = null;
          },
          error: (err) => {
            console.error('Eroare la asignare card:', err);
            this.cardErrors = 'Eroare la asignarea cardului.';
          }
        });
      },
      error: (err) => {
        console.error('Eroare la creare card:', err);
        this.cardErrors = 'Eroare la crearea cardului.';
      }
    });
  }

  loadProduseJuridice() {
  this.productService.getAllProducts().subscribe({
    next: (prods: any[]) => {
      this.produseJuridice = prods.filter(p => p.idUser === this.user.IdUser);
    }
  });
}








  deleteCard(idCard: number): void {
    this.cardService.deleteCard(idCard).subscribe({
      next: () => this.loadCards(this.user.IdUser),
      error: (err) => console.error("Eroare la ștergere card:", err)
    });
  }
  onCVVInput(event: any): void {
    // Permite doar cifre si doar 3 caractere
    const input = event.target as HTMLInputElement;
    input.value = input.value.replace(/\D/g, '').slice(0, 3);
    this.manualCard.cvv = input.value;
  }


  incarcaPreturi(): void {
    this.preturiPeProdus.clear();

    const iduri = this.produseJuridice.map(p => p.idProdus);
 // sau produseJuridice, daca ai redenumit

    iduri.forEach(id => {
      this.productService.getPreturiByProdus(id).subscribe({
        next: (preturi) => {
          this.preturiPeProdus.set(id, preturi);
        },
        error: () => console.error(`Eroare la preturi pentru produsul ${id}`)
      });
    });
  }


  incarcaProduse(): void {
    this.productService.getAllProducts().subscribe({
      next: (produse) => {
        this.produse = produse.filter(p => p.IdUser == this.authService.user?.IdUser); // exact ca in administrator.component.ts
        this.incarcaPreturi(); // doar dupa ce ai produse valide
      },
      error: () => this.produse = []
    });

  }


  extrageIdProduse(): number[] {
    return this.produse.map(p => p.idProdus);
  }


  salveazaPret(pret: any): void {
    this.productService.updatePret(pret.idPP, {
      pret: pret.pret,
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








  
}
