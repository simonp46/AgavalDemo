import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../../../core/services/api-base-url.token';
import {
  AjustarStockRequest,
  CrearProductoRequest,
  ProductoResponse,
} from '../models/producto.models';
import { ProductosApiService } from './productos-api.service';

const PRODUCTO: ProductoResponse = {
  id: 12,
  nombre: 'Teclado',
  descripcion: null,
  precio: 249_900,
  stock: 3,
  stockMinimo: 5,
  categoriaId: 1,
  categoriaNombre: 'Electrónica',
  fechaCreacion: '2026-08-06T14:30:00',
  esStockBajo: true,
};

describe('ProductosApiService', () => {
  let service: ProductosApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ProductosApiService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideZonelessChangeDetection(),
        {
          provide: API_BASE_URL,
          useValue: '/api',
        },
      ],
    });

    service = TestBed.inject(ProductosApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('envía los filtros de listado como query params', () => {
    let response: readonly ProductoResponse[] = [];

    service
      .listar({ categoriaId: 1, estadoStock: 'bajo' })
      .subscribe((productos) => {
        response = productos;
      });

    const request = httpTesting.expectOne(
      (candidate) =>
        candidate.url === '/api/productos' &&
        candidate.params.get('categoriaId') === '1' &&
        candidate.params.get('estadoStock') === 'bajo',
    );

    expect(request.request.method).toBe('GET');
    request.flush([PRODUCTO]);
    expect(response).toEqual([PRODUCTO]);
  });

  it('crea un producto con el payload contractual', () => {
    const payload: CrearProductoRequest = {
      nombre: 'Teclado',
      descripcion: null,
      precio: 249_900,
      stock: 3,
      stockMinimo: 5,
      categoriaId: 1,
    };

    service.crear(payload).subscribe((producto) => {
      expect(producto).toEqual(PRODUCTO);
    });

    const request = httpTesting.expectOne('/api/productos');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(PRODUCTO);
  });

  it('usa PATCH para ajustar el stock', () => {
    const payload: AjustarStockRequest = {
      tipo: 'SALIDA',
      cantidad: 2,
    };

    service.ajustarStock(12, payload).subscribe();

    const request = httpTesting.expectOne('/api/productos/12/stock');
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual(payload);
    request.flush({
      productoId: 12,
      stockAnterior: 3,
      tipo: 'SALIDA',
      cantidad: 2,
      stockActual: 1,
      esStockBajo: true,
    });
  });

  it('consulta el catálogo de categorías', () => {
    service.listarCategorias().subscribe();

    const request = httpTesting.expectOne('/api/categorias');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });
});
