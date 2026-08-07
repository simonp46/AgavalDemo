import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';

export const appRoutes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    loadChildren: () =>
      import('./features/auth/auth.routes').then(
        ({ AUTH_ROUTES }) => AUTH_ROUTES,
      ),
  },
  {
    path: 'productos',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./features/productos/productos.routes').then(
        ({ PRODUCTOS_ROUTES }) => PRODUCTOS_ROUTES,
      ),
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
