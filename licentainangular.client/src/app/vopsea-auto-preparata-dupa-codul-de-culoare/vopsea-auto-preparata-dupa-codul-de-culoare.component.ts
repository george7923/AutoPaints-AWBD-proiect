import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { VopseaService } from '../services/vopsea.service';

import {
  CreateVopseaSiAdaugaInCosDto,
  VopseaCreareCosResponseDto,

} from '../services/vopsea.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-vopsea-auto-preparata-dupa-codul-de-culoare',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './vopsea-auto-preparata-dupa-codul-de-culoare.component.html',

  styleUrls: ['./vopsea-auto-preparata-dupa-codul-de-culoare.component.css']
})
export class VopseaAutoPreparataDupaCodulDeCuloareComponent implements OnInit {

  tipVopsea: string = 'spray';
  marcaMasinii: string = '';
  codCuloare: string = '';
  modelMasina: string = '';
  anFabricatie: number = 0;

  serieCaroserie: string = '';
  cantitateSubproduse: number = 1;
  detaliiSuplimentare: string = '';

  // Pentru mesaje
  msg: string = '';
  msgColor: string = 'black';

  // Debug logs
  debugLogs: string[] = [];

  // ID-ul userului, preluat din AuthService
  IdUser: number = 0;

  constructor(
    private vopseaService: VopseaService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // Luăm user-ul logat
    this.IdUser = this.authService.user.IdUser;
    this.debugLogs.push(`INIT: Preluat IdUser = ${this.IdUser}`);
  }

  adaugaInCos(): void {
    if (!this.IdUser || this.IdUser <= 0) {
      this.msg = "Ești neautentificat! Nu putem adăuga în coș.";
      this.msgColor = "red";
      this.debugLogs.push("EROARE: Nu există IdUser valid.");
      return;
    }

    // Construim payload conform noii structuri
    const payload: CreateVopseaSiAdaugaInCosDto = {
      IdUser: this.IdUser,
      tipVopsea: this.tipVopsea,
      marcaMasinii: this.marcaMasinii,
      codCuloare: this.codCuloare,
      model: this.modelMasina,
      an: this.anFabricatie,
      serieCaroserie: this.serieCaroserie,
      detaliiSuplimentare: this.detaliiSuplimentare,
      cantitate: this.cantitateSubproduse
    };

    this.debugLogs.push("Trimit date la server...");

    // Apelez endpoint-ul creerii vopselei + adaugarii subproduselor in cos
    this.vopseaService.creareVopseaSiAdaugaInCos(payload).subscribe({
      next: (response: VopseaCreareCosResponseDto) => {
        this.debugLogs.push("Răspuns primit de la server:");
        this.debugLogs.push(JSON.stringify(response));

        this.msg = `Succes! vopselele adăugate în coș!`;
        this.msgColor = 'green';

        this.debugLogs.push("Vopsea creată + subproduse adăugate în cos = " + response.idCos);
      },
      error: (err) => {
        this.debugLogs.push("Eroare la adaugaInCos:");
        this.debugLogs.push(JSON.stringify(err));
        this.msg = 'Eroare la crearea produsului/vopselei.';
        this.msgColor = 'red';
      }
    });
  }
}
