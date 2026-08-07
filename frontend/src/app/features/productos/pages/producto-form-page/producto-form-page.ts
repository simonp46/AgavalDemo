import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';

import { ApiErrorPanel } from '../../../../shared/components/api-error-panel/api-error-panel';
import { ProductoForm } from '../../components/producto-form/producto-form';
import { CrearProductoRequest } from '../../models/producto.models';
import { ProductosStore } from '../../store/productos.store';

@Component({
  selector: 'app-producto-form-page',
  standalone: true,
  imports: [
    ApiErrorPanel,
    MatButtonModule,
    MatProgressBarModule,
    MatSnackBarModule,
    ProductoForm,
  ],
  templateUrl: './producto-form-page.html',
  styleUrl: './producto-form-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductoFormPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly routeId = this.route.snapshot.paramMap.get('id');
  private readonly productoId =
    this.routeId === null ? null : Number(this.routeId);

  protected readonly store = inject(ProductosStore);
  protected readonly esEdicion = this.routeId !== null;
  protected readonly inicializado = signal(false);
  protected readonly errorGeneral = computed(() => {
    const error = this.store.error();
    return error?.errors === undefined ? error : null;
  });

  ngOnInit(): void {
    void this.inicializar();
  }

  protected async guardar(request: CrearProductoRequest): Promise<void> {
    const guardado =
      this.productoId === null
        ? await this.store.crear(request)
        : await this.store.actualizar(this.productoId, request);

    if (!guardado) {
      return;
    }

    this.snackBar.open(
      this.esEdicion
        ? 'Producto actualizado correctamente.'
        : 'Producto creado correctamente.',
      'Cerrar',
      {
        duration: 4_000,
        horizontalPosition: 'end',
        verticalPosition: 'bottom',
      },
    );
    await this.router.navigate(['/productos']);
  }

  protected cancelar(): void {
    void this.router.navigate(['/productos']);
  }

  protected reintentar(): void {
    void this.inicializar();
  }

  private async inicializar(): Promise<void> {
    this.inicializado.set(false);
    this.store.limpiarError();
    this.store.limpiarProductoSeleccionado();

    if (
      this.esEdicion &&
      (this.productoId === null ||
        !Number.isInteger(this.productoId) ||
        this.productoId <= 0)
    ) {
      this.snackBar.open('El identificador del producto no es válido.', 'Cerrar', {
        duration: 4_000,
      });
      await this.router.navigate(['/productos']);
      return;
    }

    const categoriasCargadas = await this.store.cargarCategorias();

    if (!categoriasCargadas) {
      return;
    }

    if (this.productoId !== null) {
      const productoCargado = await this.store.cargarProducto(this.productoId);

      if (!productoCargado) {
        return;
      }
    }

    this.inicializado.set(true);
  }
}
