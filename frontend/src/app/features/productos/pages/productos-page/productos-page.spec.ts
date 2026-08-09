import { provideZonelessChangeDetection, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { ProductosStore } from '../../store/productos.store';
import { ProductosPage } from './productos-page';

describe('ProductosPage', () => {
  let fixture: ComponentFixture<ProductosPage>;

  beforeEach(async () => {
    const store = {
      categoriaSeleccionada: signal(null).asReadonly(),
      estadoStock: signal(null).asReadonly(),
      categorias: signal([]).asReadonly(),
      productos: signal([]).asReadonly(),
      loading: signal(false).asReadonly(),
      guardando: signal(false).asReadonly(),
      error: signal(null).asReadonly(),
      totalProductos: signal(0).asReadonly(),
      totalStockBajo: signal(0).asReadonly(),
      totalUnidades: signal(0).asReadonly(),
      hayFiltrosActivos: signal(false).asReadonly(),
      sinResultados: signal(true).asReadonly(),
      inicializar: jasmine
        .createSpy('inicializar')
        .and.returnValue(Promise.resolve()),
      actualizarFiltros: jasmine
        .createSpy('actualizarFiltros')
        .and.returnValue(Promise.resolve()),
      eliminar: jasmine
        .createSpy('eliminar')
        .and.returnValue(Promise.resolve(false)),
      ajustarStock: jasmine
        .createSpy('ajustarStock')
        .and.returnValue(Promise.resolve(false)),
    };

    await TestBed.configureTestingModule({
      imports: [ProductosPage],
      providers: [
        provideRouter([]),
        provideZonelessChangeDetection(),
        { provide: ProductosStore, useValue: store },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductosPage);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('crea la pagina con header, resumen y formulario de filtros', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('.productos-page > header')).not.toBeNull();
    expect(element.querySelector('.productos-page__metrics')).not.toBeNull();
    expect(element.querySelector('form')).not.toBeNull();
  });
});
