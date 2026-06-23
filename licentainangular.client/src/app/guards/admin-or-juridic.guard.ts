import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AdminOrJuridicGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) { }

  canActivate(): boolean {
    const user = this.auth.getUserInfo();

    const rol = user?.Persoana?.Rol || user?.persoana?.rol || '';
    const tip = user?.Persoana?.tipPersoana || user?.persoana?.tipPersoana || '';

    if (rol === 'Owner' || rol === 'Administrator' || (rol === 'Participant' && tip === 'Juridica')) {
      return true;
    }

    alert('Acces permis doar administratorilor, ownerilor sau participanților juridici.');
    this.router.navigate(['/home']);
    return false;
  }
}
