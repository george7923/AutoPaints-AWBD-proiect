import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) { }

  canActivate(): boolean {
    const user = this.auth.getUserInfo();

    // Daca nu e logat sau e participant, il redirectionam
    if (!user || user.Persoana?.Rol === 'Participant' || user.persoana?.rol === 'Participant') {
      alert("Acces interzis pentru participanți.");
      this.router.navigate(['/home']); 
      return false;
    }

    return true; // are acces
  }
}
