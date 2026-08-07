import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { ApiProblemError } from '../../../core/interceptors/problem-details.interceptor';
import { ProblemDetails } from '../../../core/models/problem-details.model';
import { CategoriaResponse } from '../models/categoria.models';
import { ProductoResponse } from '../models/producto.models';
import { ProductosApiService } from '../services/productos-api.service';
import { ProductosStore } from './productos.store';

const CATEGORIAS: readonly CategoriaResponse[] = [
  { id: 1, nombre: 'Electrónica', activo: true },
];

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

const PROBLEM: ProblemDetails = {
  type: 'urn:gestor-inventario:problem:unexpected-error',
  title: 'Error',
  status: 500,
  detail: 'No fue posible completar la solicitud.',
  instance: '/api/productos',
  code: 'unexpected_error',
  traceId: 'trace-1',
};

describe('ProductosStore', () => {
  let store: ProductosStore;
  let api: jasmine.SpyObj<ProductosApiService>;

  beforeEach(() => {
    api = jasmine.createSpyObj<ProductosApiService>('ProductosApiService', [
      'listar',
      'obtenerPorId',
      'crear',
      'actualizar',
      'eliminar',
      'listarStockBajo',
      'ajustarStock',
      'listarCategorias',
    ]);
    api.listar.and.returnValue(of(PRODUCTOS));
    api.listarCategorias.and.returnValue(of(CATEGORIAS));
    api.eliminar.and.returnValue(of(undefined));

    TestBed.configureTestingModule({
      providers: [
        ProductosStore,
        provideZonelessChangeDetection(),
        {
          provide: ProductosApiService,
          useValue: api,
        },
      ],
    });

    store = TestBed.inject(ProductosStore);
  });

  it('carga productos, categorías y métricas derivadas', async () => {
    await store.inicializar();

    expect(store.productos()).toEqual(PRODUCTOS);
    expect(store.categorias()).toEqual(CATEGORIAS);
    expect(store.totalProductos()).toBe(2);
    expect(store.totalStockBajo()).toBe(1);
    expect(store.totalUnidades()).toBe(12);
    expect(store.loading()).toBeFalse();
  });

  it('envía al backend la categoría y el estado seleccionados', async () => {
    await store.actualizarFiltros(1, 'bajo');

    expect(store.categoriaSeleccionada()).toBe(1);
    expect(store.estadoStock()).toBe('bajo');
    expect(api.listar).toHaveBeenCalledWith({
      categoriaId: 1,
      estadoStock: 'bajo',
    });
  });

  it('elimina el producto del estado después de confirmar la API', async () => {
    await store.cargarProductos();
    const eliminado = await store.eliminar(1);

    expect(eliminado).toBeTrue();
    expect(store.productos().map((producto) => producto.id)).toEqual([2]);
  });

  it('conserva ProblemDetails cuando falla una lectura', async () => {
    api.listar.and.returnValue(
      throwError(() => new ApiProblemError(PROBLEM)),
    );

    const cargado = await store.cargarProductos();

    expect(cargado).toBeFalse();
    expect(store.error()).toEqual(PROBLEM);
    expect(store.loading()).toBeFalse();
  });
});
