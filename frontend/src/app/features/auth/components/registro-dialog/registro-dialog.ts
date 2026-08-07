import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { AuthStore } from '../../../../core/auth/auth.store';
import { UsuarioResponse } from '../../../../core/auth/auth.models';

interface RegistroFormControls {
  nombre: FormControl<string>;
  apellidos: FormControl<string>;
  numeroDocumento: FormControl<string>;
  area: FormControl<string>;
  usuario: FormControl<string>;
  contrasena: FormControl<string>;
}

@Component({
  selector: 'app-registro-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogActions,
    MatDialogContent,
    MatDialogTitle,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule,
    ReactiveFormsModule,
  ],
  templateUrl: './registro-dialog.html',
  styleUrl: './registro-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegistroDialog {
  private readonly dialogRef = inject(
    MatDialogRef<RegistroDialog, UsuarioResponse>,
  );

  protected readonly auth = inject(AuthStore);
  protected readonly registroForm = new FormGroup<RegistroFormControls>({
    nombre: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    apellidos: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    numeroDocumento: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(30)],
    }),
    area: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    usuario: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(60),
        Validators.pattern(/^[a-zA-Z0-9._-]+$/),
      ],
    }),
    contrasena: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(128),
      ],
    }),
  });

  protected async sugerirUsuario(): Promise<void> {
    const nombre = this.registroForm.controls.nombre;
    const apellidos = this.registroForm.controls.apellidos;
    const usuario = this.registroForm.controls.usuario;

    if (nombre.invalid || apellidos.invalid || usuario.dirty) {
      return;
    }

    const sugerencia = await this.auth.sugerirUsuario(
      nombre.value,
      apellidos.value,
    );

    if (sugerencia !== null && !usuario.dirty) {
      usuario.setValue(sugerencia);
    }
  }

  protected async crear(): Promise<void> {
    this.auth.limpiarError();

    if (this.registroForm.invalid) {
      this.registroForm.markAllAsTouched();
      return;
    }

    const usuario = await this.auth.registrar(this.registroForm.getRawValue());

    if (usuario !== null) {
      this.dialogRef.close(usuario);
    }
  }

  protected cancelar(): void {
    this.auth.limpiarError();
    this.dialogRef.close();
  }
}
