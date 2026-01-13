import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/UserRole';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    // Verificar se a rota requer roles específicos
    const requiredRoles = route.data['roles'] as UserRole[];
    if (requiredRoles && requiredRoles.length > 0) {
      const userRole = this.authService.getUserRole();

      if (!userRole || !requiredRoles.includes(userRole)) {
        this.router.navigate(['/dashboard']);
        return false;
      }
    }

    return true;
  }
}
