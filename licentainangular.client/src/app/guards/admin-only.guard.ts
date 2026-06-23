import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminOnlyGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) { }

  canActivate(): boolean {
    const user = this.auth.getUserInfo();
    const rol = user?.Persoana?.Rol || user?.persoana?.rol || '';

    if (rol === 'Owner' || rol === 'Administrator') return true;

    alert('Acces permis doar administratorilor și ownerilor.');
    this.router.navigate(['/home']);
    return false;
  }
}
