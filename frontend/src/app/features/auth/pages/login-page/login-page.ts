import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthStore } from '../../../../core/auth/auth.store';
import { UsuarioResponse } from '../../../../core/auth/auth.models';
import { RegistroDialog } from '../../components/registro-dialog/registro-dialog';

interface LoginFormControls {
  usuario: FormControl<string>;
  contrasena: FormControl<string>;
}

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    MatSnackBarModule,
    ReactiveFormsModule,
  ],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly auth = inject(AuthStore);
  protected readonly loginForm = new FormGroup<LoginFormControls>({
    usuario: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(60)],
    }),
    contrasena: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(128)],
    }),
  });

  ngOnInit(): void {
    void this.redirigirSiExisteSesion();
  }

  protected async ingresar(): Promise<void> {
    this.auth.limpiarError();

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const autenticado = await this.auth.login(this.loginForm.getRawValue());

    if (autenticado) {
      await this.router.navigateByUrl(this.returnUrl());
    }
  }

  protected async abrirRegistro(): Promise<void> {
    this.auth.limpiarError();
    const usuario = await firstValueFrom(
      this.dialog
        .open<RegistroDialog, undefined, UsuarioResponse>(RegistroDialog, {
          width: 'min(54rem, calc(100vw - 2rem))',
          maxWidth: '54rem',
          autoFocus: 'first-tabbable',
          restoreFocus: true,
        })
        .afterClosed(),
    );

    if (usuario === undefined) {
      return;
    }

    this.loginForm.controls.usuario.setValue(usuario.usuario);
    this.loginForm.controls.contrasena.setValue('');
    this.snackBar.open(
      'Cuenta creada. Ingresa con tu nueva contraseña.',
      'Cerrar',
      { duration: 5_000 },
    );
  }

  private async redirigirSiExisteSesion(): Promise<void> {
    if (await this.auth.asegurarSesion()) {
      await this.router.navigateByUrl(this.returnUrl());
    }
  }

  private returnUrl(): string {
    const requested = this.route.snapshot.queryParamMap.get('returnUrl');
    return requested?.startsWith('/productos') === true
      ? requested
      : '/productos';
  }
}
