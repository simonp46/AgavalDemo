import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { ProblemDetails } from '../../../../core/models/problem-details.model';
import { CategoriaResponse } from '../../models/categoria.models';
import {
  CrearProductoRequest,
  ProductoResponse,
} from '../../models/producto.models';
import {
  int32Validator,
  integerValidator,
  nonBlankValidator,
  twoDecimalPlacesValidator,
} from './producto.validators';

interface ProductoFormControls {
  nombre: FormControl<string>;
  descripcion: FormControl<string | null>;
  precio: FormControl<number>;
  stock: FormControl<number>;
  stockMinimo: FormControl<number>;
  categoriaId: FormControl<number | null>;
}

@Component({
  selector: 'app-producto-form',
  standalone: true,
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
  ],
  templateUrl: './producto-form.html',
  styleUrl: './producto-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductoForm {
  private readonly changeDetectorRef = inject(ChangeDetectorRef);
  private readonly destroyRef = inject(DestroyRef);

  readonly producto = input<ProductoResponse | null>(null);
  readonly categorias = input.required<readonly CategoriaResponse[]>();
  readonly guardando = input(false);
  readonly erroresServidor = input<ProblemDetails['errors'] | null>(undefined);
  readonly guardar = output<CrearProductoRequest>();
  readonly cancelar = output<void>();

  readonly form = new FormGroup<ProductoFormControls>({
    nombre: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(150),
        nonBlankValidator,
      ],
    }),
    descripcion: new FormControl<string | null>(null, {
      validators: [Validators.maxLength(500)],
    }),
    precio: new FormControl(0, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(0.01),
        Validators.max(99_999_999.99),
        twoDecimalPlacesValidator,
      ],
    }),
    stock: new FormControl(0, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(0),
        integerValidator,
        int32Validator,
      ],
    }),
    stockMinimo: new FormControl(5, {
      nonNullable: true,
      validators: [Validators.required, integerValidator, int32Validator],
    }),
    categoriaId: new FormControl<number | null>(null, {
      validators: [Validators.required, Validators.min(1), int32Validator],
    }),
  });

  protected readonly tituloAccion = computed(() =>
    this.producto() === null ? 'Crear producto' : 'Guardar cambios',
  );

  constructor() {
    effect(() => {
      const producto = this.producto();

      this.form.reset(
        producto === null
          ? {
              nombre: '',
              descripcion: null,
              precio: 0,
              stock: 0,
              stockMinimo: 5,
              categoriaId: null,
            }
          : {
              nombre: producto.nombre,
              descripcion: producto.descripcion,
              precio: producto.precio,
              stock: producto.stock,
              stockMinimo: producto.stockMinimo,
              categoriaId: producto.categoriaId,
            },
        { emitEvent: false },
      );
      queueMicrotask(() => {
        if (!this.destroyRef.destroyed) {
          this.changeDetectorRef.markForCheck();
        }
      });
    });

    effect(() => {
      this.aplicarErroresServidor(this.erroresServidor());
    });
  }

  protected enviar(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();

    if (value.categoriaId === null) {
      return;
    }

    this.guardar.emit({
      nombre: value.nombre.trim(),
      descripcion: this.normalizarDescripcion(value.descripcion),
      precio: value.precio,
      stock: value.stock,
      stockMinimo: value.stockMinimo,
      categoriaId: value.categoriaId,
    });
  }

  private normalizarDescripcion(descripcion: string | null): string | null {
    const value = descripcion?.trim();
    return value ? value : null;
  }

  private aplicarErroresServidor(
    errors: ProblemDetails['errors'] | null,
  ): void {
    this.limpiarErroresServidor();

    if (errors === undefined || errors === null) {
      return;
    }

    for (const [campo, mensajes] of Object.entries(errors)) {
      const control = this.controlPorCampo(campo);

      if (control !== null) {
        control.setErrors({
          ...control.errors,
          servidor: mensajes.join(' '),
        });
      }
    }
  }

  private limpiarErroresServidor(): void {
    for (const control of Object.values(this.form.controls)) {
      if (control.hasError('servidor')) {
        control.updateValueAndValidity({ emitEvent: false });
      }
    }
  }

  private controlPorCampo(campo: string): AbstractControl | null {
    switch (campo) {
      case 'nombre':
        return this.form.controls.nombre;
      case 'descripcion':
        return this.form.controls.descripcion;
      case 'precio':
        return this.form.controls.precio;
      case 'stock':
        return this.form.controls.stock;
      case 'stockMinimo':
        return this.form.controls.stockMinimo;
      case 'categoriaId':
        return this.form.controls.categoriaId;
      default:
        return null;
    }
  }
}
