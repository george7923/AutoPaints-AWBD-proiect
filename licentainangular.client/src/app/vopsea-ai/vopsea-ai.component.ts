// vopsea-ai.component.ts
import { Component } from '@angular/core';
import { PaintAnalysisService } from '../services/paint-analysis.service';

@Component({
  selector: 'app-vopsea-ai',
  templateUrl: './vopsea-ai.component.html',
  styleUrls: ['./vopsea-ai.component.css']
})
export class Vopsea_AIComponent {
  selectedFile: File | null = null;
  result: any = null;
  imagePreviewUrl: string | null = null;

  constructor(private paintAnalysisService: PaintAnalysisService) { }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.result = null;

    if (this.selectedFile) {
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
        console.log('[onFileSelected] Preview generat:', this.imagePreviewUrl);
      };
      reader.readAsDataURL(this.selectedFile);
    } else {
      this.imagePreviewUrl = null;
    }
  }

  upload() {
    if (this.selectedFile) {
      this.paintAnalysisService.analyzePaint(this.selectedFile).subscribe(
        data => {
          this.result = data;
        },
        error => {
          console.error("Eroare la analiza vopselei:", error);
        }
      );
    }
  }
}
