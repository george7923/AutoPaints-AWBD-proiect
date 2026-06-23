import { Component, OnInit, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

// Services
import { CosService } from '../services/cos.service';
import { AuthService } from '../services/auth.service';
import { ComandaService } from '../services/comanda.service';
import { AdresaService } from '../services/adresa.service';

// Models / DTOs
import { Cos } from '../models/Cos';
import { ProdusCosDTO } from '../models/ProdusCosDTO';
import { Comanda } from '../models/Comanda';
import { Adresa } from '../models/Adresa';
import { Persoana } from '../models/Persoana';
import { Card_Plati } from '../models/Card_Plati';
import { SubprodusUpdateDTO, UserAddressDTO, ComandaPaymentDTO, AdresaNestedDTO, SimularePlataDTO, CardulDTO } from '../models/NewDTOs';
import { Subcomanda } from '../models/Subcomanda';

// Stripe
import { loadStripe, Stripe, StripeCardCvcElement, StripeCardElement, StripeCardExpiryElement, StripeCardNumberElement } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';
import { CardService } from '../services/card.service';

@Component({
  selector: 'app-cos',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './cos.component.html',
  styleUrls: ['./cos.component.css'],
})
export class CosComponent implements OnInit, AfterViewInit {

  // Stripe Elements
  stripe: Stripe | null = null;
  cardNumber!: StripeCardNumberElement;
  cardExpiry!: StripeCardExpiryElement;
  cardCvc!: StripeCardCvcElement;
  cardErrors: string | null = null;
  cardBrand: string = '';
  cardLast4: string = '';
  livrareLaDomiciliu: boolean = false;


  // Cart data
  currentCart: Cos | null = null;
  products: ProdusCosDTO[] = [];
  totalPrice: number = 0;

  // Order data
  comanda: Comanda = {
    IdComanda: 0,
    IdUser: 0,
    IdAdresa: 0,
    ETA: new Date(),
    PretTotal: 0,
    IsPlaced: false
  };
  subcomanda: Subcomanda = {
    IdSubcomanda: 0,
    IdProdus: 0,
    IdComanda: 0,
    TotalSubproduse: 0
  };

  // User additional data
  user: any;
  persoana: Persoana | null = null;
  card: Card_Plati | null = null;
  editingAddress: boolean = false;

  userAddresses: Adresa[] = [];         
  edAddressId: number | 'new' | null = null;       
  edAddress: Adresa | null = null;   
  edCardId: number | 'new' | null = null;     


  manualCardDetails = {
    numarCard: '',
    dataExpirare: '',  // format MM/YY
    cvv: ''
  };


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


  cards: any[] = []; 
  addresses: any[] = [];
  private realCVV: string = ''; 

  userCards: CardulDTO[] = [];

  manualCard = {
    numarCard: '',
    expiry: '',
    cvv: ''
  };


  orderSuccess: boolean = false;
  confirmationMessage: string = "";
  confirmationColor: string = "";
  categoryImages: { [key: string]: string } = {};


  constructor(
    private cosService: CosService,
    private authService: AuthService,
    private comandaService: ComandaService,
    private adresaService: AdresaService,
    private cardService: CardService, 
    private router: Router
  ) { }


  ngOnInit(): void {
    const userId = this.authService.user?.IdUser;
    if (!userId) {
      console.error("[CosComponent] Userul nu este autentificat.");
      return;
    }
    if (this.totalPrice > 1000 && this.livrareLaDomiciliu) {
      this.livrareLaDomiciliu = false;
    }
    // Incarca continutul cosului
    this.cosService.getCartContentByUserId(userId).subscribe({
      next: (response) => {
        console.log("[CosComponent] cart-content response:", response);
        if (!response || !response.cart) {
          console.warn("[CosComponent] Nu există coș => se creează unul nou.");
          this.createCartForUser(userId);
          return;
        }
        this.currentCart = response.cart;
        this.authService.cos = response.cart;
        this.products = (response.products || []).map(item => {
          let base64Img = "assets/default.png";
          if (item.imagine && Array.isArray(item.imagine)) {
            base64Img = this.convertToBase64(item.imagine);
          }
          return { ...item, imagine: base64Img };
        });
        this.calculateTotalPrice();
      },
      error: (err) => {
        if (err.status === 404) {
          console.warn("[CosComponent] Nu există coș => se creează unul nou.");
          this.createCartForUser(userId);
        } else {
          console.error("[CosComponent] Eroare la preluarea coșului:", err);
        }
      }
    });

    // Incarca adresele asociate utilizatorului
    this.adresaService.getAddressesByUser(userId).subscribe({
      next: (response: any) => {
        this.userAddresses = response.addresses || response;
      },
      error: (err) => {
        console.error("Eroare la încărcarea adreselor:", err);
      }
    });

    this.persoana = this.authService.user?.Persoana || null;
    this.card = this.authService.card || null;
    //this.loadAdresele();, deja am incarcat cu getAddressesByUser
    this.loadCards();
  }

  ngAfterViewInit(): void {
    loadStripe('pk_test_YourStripePublicKey').then((stripeInstance) => {
      this.stripe = stripeInstance;
      if (!this.stripe) return;
      const elements = this.stripe.elements();

      this.cardNumber = elements.create('cardNumber', {
        style: { base: { fontSize: '16px' } },
      });
      this.cardNumber.mount('#card-number-element');

      this.cardExpiry = elements.create('cardExpiry', {
        style: { base: { fontSize: '16px' } },
      });
      this.cardExpiry.mount('#card-expiry-element');

      this.cardCvc = elements.create('cardCvc', {
        style: { base: { fontSize: '16px' } },
      });
      this.cardCvc.mount('#card-cvc-element');

      this.cardNumber.on('change', (event: any) => {
        this.cardErrors = event.error ? event.error.message : null;
        this.cardBrand = event.brand ? event.brand : '';
      });
      this.cardExpiry.on('change', (event: any) => {
        this.cardErrors = event.error ? event.error.message : null;
      });
      this.cardCvc.on('change', (event: any) => {
        this.cardErrors = event.error ? event.error.message : null;
      });
    });
  }

  // Functii pentru gestionarea cosului
  increaseCantitatea(product: ProdusCosDTO): void {
    if (!this.currentCart) return;
    const request: SubprodusUpdateDTO = { idCos: this.currentCart.idCos, idProdus: product.idProdus };
    this.cosService.addOneSubprodusToCart(request).subscribe({
      next: () => {
        product.cantitatea++;
        this.calculateTotalPrice();
      },
      error: (err) => console.error("Eroare la creșterea cantității:", err)
    });
  }

  decreaseCantitatea(product: ProdusCosDTO): void {
    if (!this.currentCart || product.cantitatea <= 1) return;
    const request: SubprodusUpdateDTO = { idCos: this.currentCart.idCos, idProdus: product.idProdus };
    this.cosService.removeOneSubprodusFromCart(request).subscribe({
      next: () => {
        product.cantitatea--;
        this.calculateTotalPrice();
      },
      error: (err) => console.error("Eroare la scăderea cantității:", err)
    });
  }


  StergeDinCos(product: ProdusCosDTO): void {
    if (!this.currentCart) return;
    const request: SubprodusUpdateDTO = { idCos: this.currentCart.idCos, idProdus: product.idProdus };
    this.cosService.removeAllSubproduseFromCart(request).subscribe({
      next: () => {
        this.products = this.products.filter(p => p.idProdus !== product.idProdus);
        this.calculateTotalPrice();
      },
      error: (err) => console.error("Eroare la ștergerea produsului din coș:", err)
    });
  }

  private createCartForUser(userId: number): void {
    const newCart: Cos = {
      idCos: 0,
      CodUnic: 'cart-' + Date.now(),
      IdUser: userId,
      User: null,
      DataCreare: new Date()
    };
    this.cosService.createCart(newCart).subscribe({
      next: (created: Cos) => {
        console.log("[CosComponent] Coș nou creat:", created);
        this.currentCart = created;
        this.authService.cos = created;
        this.products = [];
        this.totalPrice = 0;
      },
      error: (err) => console.error("[CosComponent] Eroare la crearea coșului:", err)
    });
  }

  private calculateTotalPrice(): void {
    this.totalPrice = this.products.reduce((acc, item) => acc + (item.pret * item.cantitatea), 0);
  }

  private convertToBase64(byteArray: number[]): string {
    const uint8Array = new Uint8Array(byteArray);
    let binary = '';
    for (let i = 0; i < uint8Array.length; i++) {
      binary += String.fromCharCode(uint8Array[i]);
    }
    return 'data:image/png;base64,' + window.btoa(binary);
  }

  loadAddresses(userId: number): void {
    this.adresaService.getAddressesByUser(userId).subscribe({
      next: (response: any) => {
        console.log("Adrese primite:", response);
        this.userAddresses = response.addresses || response;
      },
      error: (err) => {
        console.error("Eroare la încărcarea adreselor:", err);
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
        DenumireStrada: this.addressForm.NrStrada,
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

    // ➕ Dacă e adresă nouă
    if (!this.edAddressId || this.edAddressId === 'new') {
      this.adresaService.createAdresaForUser(this.user.IdUser, payload).subscribe({
        next: (created) => {
          if (created && created.idAdresa) {
            const link: UserAddressDTO = {
              idUser: this.user.IdUser,
              idAdresa: created.idAdresa
            };

            this.adresaService.assignAddressToUser(link).subscribe({
              next: () => {
                console.log("Adresă creată și asociată cu succes");
                this.loadAddresses(this.user.IdUser);
                this.editingAddress = false;
              },
              error: (err) => console.error("Eroare la asociere adresă:", err)
            });
          } else {
            console.warn("Adresă creată, dar fără idAdresa.");
          }
        },
        error: (err) => console.error("Eroare la crearea adresei:", err)
      });

      // Daca e adresa existenta
    } else if (typeof this.edAddressId === 'number') {
      this.adresaService.updateAdresaNestedById(this.edAddressId, payload).subscribe({
        next: (resp) => {
          console.log("Adresa actualizată:", resp);
          this.loadAddresses(this.user.IdUser);
          this.editingAddress = false;
        },
        error: (err) => console.error("Eroare la actualizare adresă:", err)
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


  onedAddressChange(): void {
    const ed = this.userAddresses.find(addr => addr.IdAdresa === this.edAddressId);

    if (ed) {
      this.addressForm.DenumireStrada = ed.Strada || '';
      this.addressForm.NrStrada = ed.Nr?.toString() || '';
      this.addressForm.Bloc = ed.Bloc || '';
      this.addressForm.Scara = ed.Scara || '';
      this.addressForm.Etaj = ed.Etaj || '';
      this.addressForm.Apartament = ed.Apartament || '';
      this.addressForm.DenumireLocalitate = ed.Localitate || '';
      this.addressForm.DenumireJudet = ed.Judet || '';
      this.addressForm.DenumireTara = ed.Tara || '';
    } else {
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
  }





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
    detectCardBrand(value: string) {
        throw new Error('Method not implemented.');
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




  isCashDisabled(): boolean {
    return this.totalPrice > 1000;
  }



  loadCards(): void {
    const IdUser = this.authService.user?.IdUser;
    if (!IdUser) {
      console.error("ID utilizator indisponibil pentru carduri.");
      return;
    }

    this.cardService.getCardsByUser(IdUser).subscribe({
      next: (cards: CardulDTO[]) => {
        console.log("Carduri primite de la backend:", cards);
        this.userCards = cards;
      },
      error: (err: any) => {
        console.error("Eroare la încărcarea cardurilor:", err);
      }
    });
  }
  //Fara plata la domiciliu
 

  submitOrder(): void {
    const idUser = this.authService.user?.IdUser;
    const idAdresa = this.edAddressId;
    const idCard = this.edCardId;

    // 1. Restrictie: Plata la domiciliu nu este permisa daca suma > 1000 lei
    if (this.livrareLaDomiciliu && this.totalPrice > 1000) {
      alert("Plata la domiciliu nu este disponibilă pentru comenzi peste 1000 lei.");
      this.livrareLaDomiciliu = false;
      return;
    }

    // 2. Livrare la domiciliu (fara card)
    if (this.livrareLaDomiciliu) {
      if (!idUser || !idAdresa) {
        this.cardErrors = "Selectează o adresă validă pentru livrare la domiciliu.";
        return;
      }
      this.comandaService.emitereComandaCash({
        idUser: idUser,
        idAdresa: Number(idAdresa),
        idCard: null
      });
      alert("Comandă cu plată la domiciliu trimisă cu succes!");
      this.cardErrors = null;
      this.navigareHome();
      return;
    }

    // 3. Plata cu cardul (simulare)
    if (!idUser || !idAdresa || !idCard) {
      this.cardErrors = "Selectează o adresă și un card valide.";
      return;
    }

    if (idAdresa === 'new' || idCard === 'new') {
      this.cardErrors = "Te rugăm să creezi și să salvezi întâi noul card și/sau adresă.";
      return;
    }

    const cardPayload: SimularePlataDTO = {
      idUser: idUser,
      idAdresa: Number(idAdresa),
      idCard: Number(idCard),
      numarCard: this.manualCard.numarCard.replace(/\s/g, ''),
      dataExpirare: this.manualCard.expiry,
      cvv: this.realCVV || this.manualCard.cvv
    };

    const FAKE_CARD_NUMBER = '4444123456789012';
    const FAKE_CARD_EXPIRY = '10/27';
    const FAKE_CARD_CVV = '555';
    console.warn("#### COMPARARE CU CARD FAKE:");
    console.log(" Numar introdus:", cardPayload.numarCard, "| FAKE:", FAKE_CARD_NUMBER);
    console.log("Expirare introdusa:", cardPayload.dataExpirare, "| FAKE:", FAKE_CARD_EXPIRY);
    console.log("CVV introdus:", cardPayload.cvv, "| FAKE:", FAKE_CARD_CVV);

    if (cardPayload.numarCard !== FAKE_CARD_NUMBER) {
      console.warn("Numărul cardului nu corespunde.");
    }
    if (cardPayload.dataExpirare !== FAKE_CARD_EXPIRY) {
      console.warn("Data expirării nu corespunde.");
    }
    if (cardPayload.cvv !== FAKE_CARD_CVV) {
      console.warn("CVV-ul nu corespunde.");
    }
    if (
      cardPayload.numarCard === FAKE_CARD_NUMBER &&
      cardPayload.dataExpirare === FAKE_CARD_EXPIRY &&
      cardPayload.cvv === FAKE_CARD_CVV
    ) {
      console.log("Card valid pentru plată simulată.");
      this.comandaService.emitereComanda({
        idUser: idUser,
        idAdresa: Number(idAdresa),
        idCard: Number(idCard)
      });
      console.log("Plată simulată cu succes!");
      alert("Plată simulată cu succes");
      this.cardErrors = null;
      this.navigareHome();
    } else {
      this.cardErrors = "Cardul introdus nu este valid pentru plată simulată.";
    }
  }







  loadAdresele(): void {
    const IdUser = this.authService.user?.IdUser;
    if (!IdUser) {
      console.error("User ID indisponibil pentru adrese.");
      return;
    }

    this.adresaService.getAddressesByUser(IdUser).subscribe({
      next: (adrese: Adresa[]) => {
        console.log("Adrese primite de la backend:", adrese);
        this.userAddresses = adrese;
      },
      error: (err) => {
        console.error("Eroare la încărcarea adreselor:", err);
      }
    });
  }



  onAddressionChange(): void {
    if (this.edAddressId === 'new') {
      console.log('Se va crea o adresă nouă.');
    } else {
      console.log(`Adresa ată (ID): ${this.edAddressId}`);
    }
  }


  onCardionChange(): void {
    const id = typeof this.edCardId === 'string' ? parseInt(this.edCardId, 10) : this.edCardId;
    const ed = this.userCards.find(card => card.idCard === id);

    if (ed) {
      this.manualCard.numarCard = ed.numarCard;
      this.manualCard.expiry = ed.dataExpirare
        ? new Date(ed.dataExpirare).toISOString().slice(5, 7) + '/' +
        new Date(ed.dataExpirare).toISOString().slice(2, 4)
        : '';

      this.realCVV = ed.cvv;
      this.manualCard.cvv = '***';
      console.log(`Card at (ID): ${this.edCardId}`);
    } else {
      this.manualCard = { numarCard: '', expiry: '', cvv: '' };
      this.realCVV = '';
    }
  }

  navigareHome(): void {
    this.router.navigate(['/home']);
  }
}





