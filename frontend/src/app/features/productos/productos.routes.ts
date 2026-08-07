import { Routes } from '@angular/router';

import { ProductosApiService } from './services/productos-api.service';
import { ProductosStore } from './store/productos.store';

export const PRODUCTOS_ROUTES: Routes = [
  {
    path: '',
    providers: [ProductosApiService, ProductosStore],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./pages/productos-page/productos-page').then(
            ({ ProductosPage }) => ProductosPage,
          ),
      },
      {
        path: 'nuevo',
        loadComponent: () =>
          import('./pages/producto-form-page/producto-form-page').then(
            ({ ProductoFormPage }) => ProductoFormPage,
          ),
      },
      {
        path: ':id/editar',
        loadComponent: () =>
          import('./pages/producto-form-page/producto-form-page').then(
            ({ ProductoFormPage }) => ProductoFormPage,
          ),
      },
    ],
  },
];
