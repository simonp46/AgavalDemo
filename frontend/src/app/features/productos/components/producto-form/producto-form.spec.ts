import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoriaResponse } from '../../models/categoria.models';
import {
  CrearProductoRequest,
  ProductoResponse,
} from '../../models/producto.models';
import { ProductoForm } from './producto-form';

const CATEGORIAS: readonly CategoriaResponse[] = [
  { id: 1, nombre: 'Electrónica', activo: true },
  { id: 2, nombre: 'Oficina', activo: true },
];

describe('ProductoForm', () => {
  let component: ProductoForm;
  let fixture: ComponentFixture<ProductoForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductoForm],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductoForm);
    fixture.componentRef.setInput('categorias', CATEGORIAS);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('rechaza nombre vacío, precio no positivo y stock negativo', () => {
    component.form.setValue({
      nombre: '   ',
      descripcion: null,
      precio: 0,
      stock: -1,
      stockMinimo: 5,
      categoriaId: 1,
    });

    expect(component.form.invalid).toBeTrue();
    expect(component.form.controls.nombre.hasError('nonBlank')).toBeTrue();
    expect(component.form.controls.precio.hasError('min')).toBeTrue();
    expect(component.form.controls.stock.hasError('min')).toBeTrue();
  });

  it('emite un request tipado y normalizado cuando el formulario es válido', () => {
    let emitted: CrearProductoRequest | undefined;
    component.guardar.subscribe((request) => {
      emitted = request;
    });
    component.form.setValue({
      nombre: '  Teclado  ',
      descripcion: '  Mecánico  ',
      precio: 249_900,
      stock: 8,
      stockMinimo: 5,
      categoriaId: 1,
    });

    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));

    expect(emitted).toEqual({
      nombre: 'Teclado',
      descripcion: 'Mecánico',
      precio: 249_900,
      stock: 8,
      stockMinimo: 5,
      categoriaId: 1,
    });
  });

  it('rechaza precios con más de dos decimales', () => {
    component.form.controls.precio.setValue(10.123);

    expect(
      component.form.controls.precio.hasError('decimalPlaces'),
    ).toBeTrue();
  });

  it('precarga en el DOM el producto recibido para edición', async () => {
    const producto: ProductoResponse = {
      id: 10,
      nombre: 'Producto existente',
      descripcion: 'Descripción existente',
      precio: 1250.5,
      stock: 8,
      stockMinimo: 3,
      categoriaId: 1,
      categoriaNombre: 'Electrónica',
      fechaCreacion: '2026-08-07T09:00:00',
      esStockBajo: false,
    };

    fixture.componentRef.setInput('producto', producto);
    fixture.detectChanges();
    await fixture.whenStable();

    const nombre = fixture.nativeElement.querySelector(
      'input[formcontrolname="nombre"]',
    ) as HTMLInputElement;

    expect(component.form.getRawValue().nombre).toBe(producto.nombre);
    expect(nombre.value).toBe(producto.nombre);
  });
});
