import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductoResponse } from '../../models/producto.models';
import { ProductosTable } from './productos-table';

const PRODUCTOS: readonly ProductoResponse[] = [
  {
    id: 1,
    nombre: 'Teclado',
    descripcion: null,
    precio: 200_000,
    stock: 2,
    stockMinimo: 5,
    categoriaId: 1,
    categoriaNombre: 'Electrónica',
    fechaCreacion: '2026-08-06T10:00:00',
    esStockBajo: true,
  },
  {
    id: 2,
    nombre: 'Mouse',
    descripcion: null,
    precio: 80_000,
    stock: 10,
    stockMinimo: 5,
    categoriaId: 1,
    categoriaNombre: 'Electrónica',
    fechaCreacion: '2026-08-06T11:00:00',
    esStockBajo: false,
  },
];

describe('ProductosTable', () => {
  let component: ProductosTable;
  let fixture: ComponentFixture<ProductosTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductosTable],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductosTable);
    fixture.componentRef.setInput('productos', PRODUCTOS);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('muestra estados de stock textuales además del color', () => {
    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Stock bajo');
    expect(text).toContain('Normal');
  });

  it('emite el id cuando se solicita editar', () => {
    let productoId: number | undefined;
    component.editar.subscribe((id) => {
      productoId = id;
    });

    const editarButton = fixture.nativeElement.querySelector(
      'button[aria-label="Editar Teclado"]',
    ) as HTMLButtonElement;
    editarButton.click();

    expect(productoId).toBe(1);
  });
});
