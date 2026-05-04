import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ComandaService } from '../services/comanda.service';
import { SubcomandaService } from '../services/subcomanda.service';
import { AuthService } from '../services/auth.service';

import { Comanda } from '../models/Comanda';
import { Produs } from '../models/Produs'; 
import { Persoana } from '../models/Persoana';
//COMPONENTA NEFOLOSITA, AM PUS IN ISTORIC-COMENZI-COMPONENTA GENERARE PDF IN LOCUL ACESTEIA
@Component({
  selector: 'app-comanda',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comanda.component.html',
  styleUrls: ['./comanda.component.css'],
})
export class ComandaComponent implements OnInit {
  // Comanda curenta
  comanda: Comanda = {
    IdComanda: 0,
    IdUser: 0,
    User: null,
    IdAdresa: 0,
    Adresa: null,
    IdCard: 0,
    Card: null,
    ETA: new Date(),
    PretTotal: 0,  
    IsPlaced: false 
  };


  // Produsele asociate comenzii
  products: Produs[] = [];
  total: number = 0;

  // Info persoana (din AuthService)
  persoana: Persoana | null = null;

  // Hardcode / map categorii -> imagini
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
    private route: ActivatedRoute,
    private router: Router,
    private comandaService: ComandaService,
    private subcomandaService: SubcomandaService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // Luam param "IdComanda" din URL
    const comandaIdParam = this.route.snapshot.paramMap.get('IdComanda');
    const IdComanda = comandaIdParam ? Number(comandaIdParam) : 0;

    // incarcam datele comenzii daca ID-ul e valid
    if (IdComanda > 0) {
      // 1) Afisam detaliile comenzii
      this.comandaService.getComandaById(IdComanda).subscribe({
        next: (comandaFetched) => {
          this.comanda = comandaFetched;
          console.log('Detaliile comenzii selectate: ', this.comanda);

          // 2) Afisam produsele din subcomenzi (join)
          this.subcomandaService.getProdusByComandaId(IdComanda).subscribe({
            next: (produse) => {
              this.products = produse;
              console.log('Produsele din comanda selectata: ', this.products);
              this.calculateTotal();
            },
            error: (err) => {
              console.error('Eroare la afisarea produselor din comanda:', err);
            },
          });
        },
        error: (err) => {
          console.error('Eroare la afisarea comenzii:', err);
        },
      });
    } else {
      console.error('ID al comenzii invalid:', IdComanda);
    }

    // Preluam persoana logata
    this.persoana = this.authService.user?.Persoana || null;
  }

  /**
   * Calculeaza un total local, adunand preturile produselor
   */
  calculateTotal(): void {
    this.total = this.products.reduce((sum, product) => sum + (product.Pret || 0), 0);
  }


  anuleazaComanda(): void {

    console.log('Functie de Anulare Comanda - neimplementata.');
  }

  /**
   * Navigheaza la istoricul comenzilor userului
   */
  navigateToIstoricComenzi(): void {
    this.router.navigate(['/istoric-comenzi']);
  }
}
