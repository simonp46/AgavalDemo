import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { ApiErrorPanel } from '../../../../shared/components/api-error-panel/api-error-panel';
import { AjustarStockDialog } from '../../components/ajustar-stock-dialog/ajustar-stock-dialog';
import { ConfirmarEliminacionDialog } from '../../components/confirmar-eliminacion-dialog/confirmar-eliminacion-dialog';
import { ProductosTable } from '../../components/productos-table/productos-table';
import {
  AjustarStockRequest,
  EstadoStock,
  ProductoResponse,
} from '../../models/producto.models';
import { ProductosStore } from '../../store/productos.store';

interface FiltrosFormControls {
  categoriaId: FormControl<number | null>;
  estadoStock: FormControl<EstadoStock | null>;
}

@Component({
  selector: 'app-productos-page',
  standalone: true,
  imports: [
    ApiErrorPanel,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatFormFieldModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSnackBarModule,
    ProductosTable,
    ReactiveFormsModule,
  ],
  templateUrl: './productos-page.html',
  styleUrl: './productos-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductosPage implements OnInit {
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly store = inject(ProductosStore);
  protected readonly filtrosForm = new FormGroup<FiltrosFormControls>({
    categoriaId: new FormControl<number | null>(
      this.store.categoriaSeleccionada(),
    ),
    estadoStock: new FormControl<EstadoStock | null>(this.store.estadoStock()),
  });

  ngOnInit(): void {
    void this.store.inicializar();
  }

  protected aplicarFiltros(): void {
    const filtros = this.filtrosForm.getRawValue();
    void this.store.actualizarFiltros(
      filtros.categoriaId,
      filtros.estadoStock,
    );
  }

  protected limpiarFiltros(): void {
    this.filtrosForm.reset(
      {
        categoriaId: null,
        estadoStock: null,
      },
      { emitEvent: false },
    );
    void this.store.actualizarFiltros(null, null);
  }

  protected crearProducto(): void {
    void this.router.navigate(['/productos/nuevo']);
  }

  protected editarProducto(id: number): void {
    void this.router.navigate(['/productos', id, 'editar']);
  }

  protected async confirmarEliminacion(
    producto: ProductoResponse,
  ): Promise<void> {
    const confirmado = await firstValueFrom(
      this.dialog
        .open<ConfirmarEliminacionDialog, ProductoResponse, boolean>(
          ConfirmarEliminacionDialog,
          {
            data: producto,
            autoFocus: 'dialog',
            restoreFocus: true,
          },
        )
        .afterClosed(),
    );

    if (confirmado !== true) {
      return;
    }

    const eliminado = await this.store.eliminar(producto.id);

    if (eliminado) {
      this.mostrarExito('Producto eliminado correctamente.');
    }
  }

  protected async abrirAjusteStock(
    producto: ProductoResponse,
  ): Promise<void> {
    const request = await firstValueFrom(
      this.dialog
        .open<AjustarStockDialog, ProductoResponse, AjustarStockRequest>(
          AjustarStockDialog,
          {
            data: producto,
            autoFocus: 'first-tabbable',
            restoreFocus: true,
          },
        )
        .afterClosed(),
    );

    if (request === undefined) {
      return;
    }

    const ajustado = await this.store.ajustarStock(producto.id, request);

    if (ajustado) {
      this.mostrarExito('Stock actualizado correctamente.');
    }
  }

  protected reintentar(): void {
    void this.store.inicializar();
  }

  private mostrarExito(message: string): void {
    this.snackBar.open(message, 'Cerrar', {
      duration: 4_000,
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
    });
  }
}
