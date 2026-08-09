import { provideZonelessChangeDetection, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  ActivatedRoute,
  convertToParamMap,
  provideRouter,
} from '@angular/router';

import { ProductosStore } from '../../store/productos.store';
import { ProductoFormPage } from './producto-form-page';

describe('ProductoFormPage', () => {
  let fixture: ComponentFixture<ProductoFormPage>;

  beforeEach(async () => {
    const store = {
      error: signal(null).asReadonly(),
      loading: signal(false).asReadonly(),
      guardando: signal(false).asReadonly(),
      categorias: signal([]).asReadonly(),
      productoSeleccionado: signal(null).asReadonly(),
      limpiarError: jasmine.createSpy('limpiarError'),
      limpiarProductoSeleccionado: jasmine.createSpy(
        'limpiarProductoSeleccionado',
      ),
      cargarCategorias: jasmine
        .createSpy('cargarCategorias')
        .and.returnValue(Promise.resolve(true)),
      cargarProducto: jasmine
        .createSpy('cargarProducto')
        .and.returnValue(Promise.resolve(true)),
      crear: jasmine
        .createSpy('crear')
        .and.returnValue(Promise.resolve(true)),
      actualizar: jasmine
        .createSpy('actualizar')
        .and.returnValue(Promise.resolve(true)),
    };

    await TestBed.configureTestingModule({
      imports: [ProductoFormPage],
      providers: [
        provideRouter([]),
        provideZonelessChangeDetection(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: convertToParamMap({}) },
          },
        },
        { provide: ProductosStore, useValue: store },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductoFormPage);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('crea la pagina y muestra el formulario para un producto nuevo', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('section[aria-labelledby]')).not.toBeNull();
    expect(element.textContent).toContain('Crear producto');
  });
});
