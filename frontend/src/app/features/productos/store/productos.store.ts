import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, Observable } from 'rxjs';

import {
  ApiProblemError,
} from '../../../core/interceptors/problem-details.interceptor';
import { ProblemDetails } from '../../../core/models/problem-details.model';
import { CategoriaResponse } from '../models/categoria.models';
import {
  ActualizarProductoRequest,
  AjustarStockRequest,
  CrearProductoRequest,
  EstadoStock,
  ListarProductosQueryParams,
  ProductoResponse,
} from '../models/producto.models';
import { ProductosApiService } from '../services/productos-api.service';

type OperationResult<T> =
  | { readonly ok: true; readonly value: T }
  | { readonly ok: false };

@Injectable()
export class ProductosStore {
  private readonly api = inject(ProductosApiService);

  private readonly productosState = signal<readonly ProductoResponse[]>([]);
  private readonly categoriasState = signal<readonly CategoriaResponse[]>([]);
  private readonly productoSeleccionadoState =
    signal<ProductoResponse | null>(null);
  private readonly categoriaSeleccionadaState = signal<number | null>(null);
  private readonly estadoStockState = signal<EstadoStock | null>(null);
  private readonly lecturasActivasState = signal(0);
  private readonly escriturasActivasState = signal(0);
  private readonly errorState = signal<ProblemDetails | null>(null);
  private secuenciaListado = 0;

  readonly productos = this.productosState.asReadonly();
  readonly categorias = this.categoriasState.asReadonly();
  readonly productoSeleccionado = this.productoSeleccionadoState.asReadonly();
  readonly categoriaSeleccionada =
    this.categoriaSeleccionadaState.asReadonly();
  readonly estadoStock = this.estadoStockState.asReadonly();
  readonly error = this.errorState.asReadonly();

  readonly loading = computed(() => this.lecturasActivasState() > 0);
  readonly guardando = computed(() => this.escriturasActivasState() > 0);
  readonly filtros = computed<ListarProductosQueryParams>(() => {
    const categoriaId = this.categoriaSeleccionada();
    const estadoStock = this.estadoStock();

    return {
      ...(categoriaId === null ? {} : { categoriaId }),
      ...(estadoStock === null ? {} : { estadoStock }),
    };
  });
  readonly totalProductos = computed(() => this.productos().length);
  readonly totalStockBajo = computed(
    () => this.productos().filter((producto) => producto.esStockBajo).length,
  );
  readonly totalUnidades = computed(() =>
    this.productos().reduce((total, producto) => total + producto.stock, 0),
  );
  readonly hayFiltrosActivos = computed(
    () =>
      this.categoriaSeleccionada() !== null || this.estadoStock() !== null,
  );
  readonly sinResultados = computed(
    () => !this.loading() && this.productos().length === 0,
  );

  async inicializar(): Promise<void> {
    await Promise.all([this.cargarCategorias(), this.cargarProductos()]);
  }

  async cargarCategorias(): Promise<boolean> {
    const result = await this.ejecutarLectura(this.api.listarCategorias());

    if (!result.ok) {
      return false;
    }

    this.categoriasState.set(result.value);
    return true;
  }

  async cargarProductos(): Promise<boolean> {
    const solicitudActual = ++this.secuenciaListado;
    const result = await this.ejecutarLectura(
      this.api.listar(this.filtros()),
    );

    if (!result.ok) {
      return false;
    }

    if (solicitudActual === this.secuenciaListado) {
      this.productosState.set(result.value);
    }

    return true;
  }

  async actualizarFiltros(
    categoriaId: number | null,
    estadoStock: EstadoStock | null,
  ): Promise<void> {
    this.categoriaSeleccionadaState.set(categoriaId);
    this.estadoStockState.set(estadoStock);
    await this.cargarProductos();
  }

  async cargarProducto(id: number): Promise<boolean> {
    this.productoSeleccionadoState.set(null);
    const result = await this.ejecutarLectura(this.api.obtenerPorId(id));

    if (!result.ok) {
      return false;
    }

    this.productoSeleccionadoState.set(result.value);
    return true;
  }

  async crear(request: CrearProductoRequest): Promise<boolean> {
    const result = await this.ejecutarEscritura(this.api.crear(request));

    if (!result.ok) {
      return false;
    }

    await this.cargarProductos();
    return true;
  }

  async actualizar(
    id: number,
    request: ActualizarProductoRequest,
  ): Promise<boolean> {
    const result = await this.ejecutarEscritura(
      this.api.actualizar(id, request),
    );

    if (!result.ok) {
      return false;
    }

    this.productoSeleccionadoState.set(result.value);
    await this.cargarProductos();
    return true;
  }

  async eliminar(id: number): Promise<boolean> {
    const result = await this.ejecutarEscritura(this.api.eliminar(id));

    if (!result.ok) {
      return false;
    }

    this.productosState.update((productos) =>
      productos.filter((producto) => producto.id !== id),
    );
    return true;
  }

  async ajustarStock(
    id: number,
    request: AjustarStockRequest,
  ): Promise<boolean> {
    const result = await this.ejecutarEscritura(
      this.api.ajustarStock(id, request),
    );

    if (!result.ok) {
      const problem = this.error();

      if (problem?.status === 409) {
        await this.cargarProductos();
        this.errorState.set(problem);
      }

      return false;
    }

    await this.cargarProductos();
    return true;
  }

  limpiarProductoSeleccionado(): void {
    this.productoSeleccionadoState.set(null);
  }

  limpiarError(): void {
    this.errorState.set(null);
  }

  private async ejecutarLectura<T>(
    source: Observable<T>,
  ): Promise<OperationResult<T>> {
    this.errorState.set(null);
    this.lecturasActivasState.update((cantidad) => cantidad + 1);

    try {
      return {
        ok: true,
        value: await firstValueFrom(source),
      };
    } catch (error: unknown) {
      this.errorState.set(this.toProblemDetails(error));
      return { ok: false };
    } finally {
      this.lecturasActivasState.update((cantidad) =>
        Math.max(0, cantidad - 1),
      );
    }
  }

  private async ejecutarEscritura<T>(
    source: Observable<T>,
  ): Promise<OperationResult<T>> {
    this.errorState.set(null);
    this.escriturasActivasState.update((cantidad) => cantidad + 1);

    try {
      return {
        ok: true,
        value: await firstValueFrom(source),
      };
    } catch (error: unknown) {
      this.errorState.set(this.toProblemDetails(error));
      return { ok: false };
    } finally {
      this.escriturasActivasState.update((cantidad) =>
        Math.max(0, cantidad - 1),
      );
    }
  }

  private toProblemDetails(error: unknown): ProblemDetails {
    if (error instanceof ApiProblemError) {
      return error.problem;
    }

    return {
      type: 'urn:gestor-inventario:problem:unexpected-error',
      title: 'Ocurrió un error inesperado.',
      status: 500,
      detail: 'No fue posible completar la solicitud.',
      instance: '',
      code: 'unexpected_error',
      traceId: '',
    };
  }
}
