import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterOutlet } from '@angular/router';

import { AuthStore } from './core/auth/auth.store';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [MatButtonModule, MatToolbarModule, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  private readonly router = inject(Router);

  protected readonly auth = inject(AuthStore);
  protected readonly title = signal('Gestor de Inventario');
  protected readonly logoUrl =
    'https://agaval.vtexassets.com/assets/vtex.file-manager-graphql/images/a021301a-febe-4f1b-9f76-353573079695___8affcee296d5f391ebf804e4f95963a6.svg';

  protected async cerrarSesion(): Promise<void> {
    await this.auth.logout();
    await this.router.navigate(['/login']);
  }
}
