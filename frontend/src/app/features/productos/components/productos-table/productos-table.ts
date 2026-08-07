import { CurrencyPipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';

import { ProductoResponse } from '../../models/producto.models';

@Component({
  selector: 'app-productos-table',
  standalone: true,
  imports: [CurrencyPipe, MatButtonModule, MatTableModule],
  templateUrl: './productos-table.html',
  styleUrl: './productos-table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductosTable {
  readonly productos = input.required<readonly ProductoResponse[]>();
  readonly deshabilitada = input(false);
  readonly editar = output<number>();
  readonly eliminar = output<ProductoResponse>();
  readonly ajustar = output<ProductoResponse>();

  protected readonly columnas = [
    'nombre',
    'categoria',
    'precio',
    'stock',
    'stockMinimo',
    'estado',
    'acciones',
  ];
}
