import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ComandaUserDTO } from '../models/NewDTOs';
import { ComandaService } from '../services/comanda.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-istoric-comenzi',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './istoric-comenzi.component.html',
  styleUrls: ['./istoric-comenzi.component.css']
})
export class IstoricComenziComponent implements OnInit {
  comenzi: ComandaUserDTO[] = [];
  filteredComenzi: ComandaUserDTO[] = [];
  edFilter: 'all' | 'delivered' | 'onTheWay' = 'all';

  constructor(
    private comandaService: ComandaService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const userId = this.authService.user.IdUser;
    if (!userId || userId === 0) {
      console.error('Utilizator neautentificat în AuthService');
      return;
    }
    this.loadComenzi(userId);
  }

  private loadComenzi(userId: number): void {
    this.comandaService.getComenziByUserCuNouDTO(userId)
      .subscribe({
        next: data => {
          this.comenzi = data;
          this.applyFilter();
        },
        error: err => console.error('Eroare la preluarea comenzilor:', err)
      });
  }

  setFilter(filter: 'all' | 'delivered' | 'onTheWay') {
    this.edFilter = filter;
    this.applyFilter();
  }

  private applyFilter(): void {
    if (this.edFilter === 'delivered') {
      this.filteredComenzi = this.comenzi.filter(c => this.isDelivered(c));
    } else if (this.edFilter === 'onTheWay') {
      this.filteredComenzi = this.comenzi.filter(c => !this.isDelivered(c));
    } else {
      this.filteredComenzi = [...this.comenzi];
    }
  }

  // 0==Pe drum (false), 1==Livrat (true)
  isDelivered(comanda: ComandaUserDTO): boolean {
    return comanda.isPlaced;
  }

  descarcaBon(idComanda: number): void {
    this.comandaService.descarcaBonFiscal(idComanda).subscribe({
      next: (blob: Blob) => {
        const a = document.createElement('a');
        const url = window.URL.createObjectURL(blob);
        a.href = url;
        a.download = `Bon_Comanda_${idComanda}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: err => {
        console.error('Eroare la descărcarea bonului fiscal:', err);
        alert('A apărut o eroare la descărcarea bonului.');
      }
    });
  }


}
