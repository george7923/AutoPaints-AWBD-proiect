import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';  // Import RouterLink
import { CommonModule } from '@angular/common'; // Import CommonModule
import { AuthService } from '../services/auth.service';  // Import AuthService

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css'],
  standalone: true,  
  imports: [CommonModule, RouterLink]  
})
export class FooterComponent {
  constructor(public authService: AuthService) { }  
}
