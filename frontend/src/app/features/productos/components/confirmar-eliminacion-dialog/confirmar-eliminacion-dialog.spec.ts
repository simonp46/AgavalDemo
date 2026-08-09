import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

import { ProductoResponse } from '../../models/producto.models';
import { ConfirmarEliminacionDialog } from './confirmar-eliminacion-dialog';

const PRODUCTO: ProductoResponse = {
  id: 1,
  nombre: 'Teclado',
  descripcion: null,
  precio: 200_000,
  stock: 8,
  stockMinimo: 5,
  categoriaId: 1,
  categoriaNombre: 'Electronica',
  fechaCreacion: '2026-08-08T00:00:00Z',
  esStockBajo: false,
};

describe('ConfirmarEliminacionDialog', () => {
  let fixture: ComponentFixture<ConfirmarEliminacionDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmarEliminacionDialog],
      providers: [
        provideZonelessChangeDetection(),
        { provide: MAT_DIALOG_DATA, useValue: PRODUCTO },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmarEliminacionDialog);
    fixture.detectChanges();
  });

  it('crea una seccion de confirmacion con el nombre del producto', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('section[aria-labelledby]')).not.toBeNull();
    expect(element.textContent).toContain(PRODUCTO.nombre);
  });
});
