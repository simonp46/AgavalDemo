import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';

import { ProductoResponse } from '../../models/producto.models';
import { AjustarStockDialog } from './ajustar-stock-dialog';

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

describe('AjustarStockDialog', () => {
  let fixture: ComponentFixture<AjustarStockDialog>;

  beforeEach(async () => {
    const dialogRef = jasmine.createSpyObj<MatDialogRef<AjustarStockDialog>>(
      'MatDialogRef',
      ['close'],
    );

    await TestBed.configureTestingModule({
      imports: [AjustarStockDialog],
      providers: [
        provideZonelessChangeDetection(),
        { provide: MAT_DIALOG_DATA, useValue: PRODUCTO },
        { provide: MatDialogRef, useValue: dialogRef },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AjustarStockDialog);
    fixture.detectChanges();
  });

  it('crea el formulario y muestra el producto que se ajustara', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('form')).not.toBeNull();
    expect(element.textContent).toContain(PRODUCTO.nombre);
  });
});
