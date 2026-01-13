import { Routes } from '@angular/router';

export const routes: Routes = [
  // Rota padrão agora aponta para o login
  {
    path: '',
    redirectTo: 'auth/login',
    pathMatch: 'full'
  },

  {
    path: 'auth',
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/login/login.component').then(m => m.LoginComponent),
        data: { title: 'Login - TechsysLog' }
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./features/auth/register/register.component').then(m => m.RegisterComponent),
        data: { title: 'Registro - TechsysLog' }
      },
      // Redirecionamento interno de /auth para /auth/login
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      }
    ]
  },

  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    // canActivate: [authGuard],
    data: { title: 'Dashboard - TechsysLog' }
  },

  // Fallback para login se a rota não existir
  {
    path: '**',
    redirectTo: 'auth/login'
  }
];
