import { Component } from '@angular/core';
import { PredictionService } from '../services/prediction.service';

@Component({
  selector: 'app-zgarieturi-ai',
  templateUrl: './zgarieturi-ai.component.html',
  styleUrls: ['./zgarieturi-ai.component.css']
})
export class ZgarieturiAIComponent {
  selectedFile: File | null = null;
  result: any = null;
  imagePreviewUrl: string | null = null;

  defectTranslation: { [key: string]: string } = {
    "crazing": "Fisuri Fine",
    "inclusion": "Inclusiuni",
    "patches": "Pete",
    "pitted_surface": "Suprafață Pătata",
    "rolled_in_scale": "Scale Incorporate",
    "scratches": "Zgârieturi",
    "Fără defecțiuni": "Fără defecte"
  };

  defectColors: { [key: string]: string } = {
    "Fisuri Fine": "#FFCCCC",
    "Inclusiuni": "#CCCCFF",
    "Pete": "#CCFFCC",
    "Suprafață Pătata": "#FFFFCC",
    "Scale Incorporate": "#E6CCFF",
    "Zgârieturi": "#FFD9B3",
    "Fără defecte": "#CCFFCC"
  };

  readonly NO_DEFECT_THRESHOLD = 0.4;

  constructor(private predictionService: PredictionService) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.result = null;

    if (this.selectedFile) {
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
        console.log('[onFileSelected] URL imagine generat:', this.imagePreviewUrl);
      };
      reader.readAsDataURL(this.selectedFile);
    } else {
      this.imagePreviewUrl = null;
    }
  }


  upload() {
    if (this.selectedFile) {
      console.log('[upload] Trimit fișier:', this.selectedFile);

      this.predictionService.predictDefect(this.selectedFile).subscribe(
        data => {
          console.log('[upload] Răspuns primit:', data);

          let defectEng = data.defectClass;
          let probability = data.probability;

          if (probability < this.NO_DEFECT_THRESHOLD) {
            console.log('[upload] Probabilitate sub prag. Clasificat ca fără defecte.');
            defectEng = "Fără defecțiuni";
            probability = 0;
          }

          const defectRo = this.defectTranslation[defectEng] || defectEng;
          console.log(`[upload] Tradus în română: ${defectEng} -> ${defectRo}`);

          this.result = {
            DefectClass: defectRo,
            Probability: probability,
            RawOutput: data.RawOutput
          };

          console.log('[upload] Rezultat final setat:', this.result);
        },
        error => {
          console.error("[upload] Eroare la detectarea defectelor:");
          console.error("Status: ", error.status);
          console.error("StatusText: ", error.statusText);
          console.error("Error: ", error.error);
          console.error("URL: ", error.url);
        }
      );
    } else {
      console.warn('[upload] Nu a fost selectat niciun fișier!');
    }
  }

  getBackgroundColor(): string {
    if (!this.result) return '';
    const defect = this.result.DefectClass;
    return this.defectColors[defect] || '#ffffff';
  }
}
