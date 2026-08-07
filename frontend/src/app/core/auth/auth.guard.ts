import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthStore);
  const router = inject(Router);

  if (await auth.asegurarSesion()) {
    return true;
  }

  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url },
  });
};
