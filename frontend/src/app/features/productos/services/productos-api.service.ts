import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../../../core/services/api-base-url.token';
import { CategoriaResponse } from '../models/categoria.models';
import {
  ActualizarProductoRequest,
  AjustarStockRequest,
  AjusteStockResponse,
  CrearProductoRequest,
  ListarProductosQueryParams,
  ProductoResponse,
} from '../models/producto.models';

@Injectable()
export class ProductosApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = inject(API_BASE_URL);
  private readonly productosUrl = `${this.apiBaseUrl}/productos`;

  listar(
    filtros: ListarProductosQueryParams = {},
  ): Observable<readonly ProductoResponse[]> {
    let params = new HttpParams();

    if (filtros.categoriaId !== undefined) {
      params = params.set('categoriaId', filtros.categoriaId);
    }

    if (filtros.estadoStock !== undefined) {
      params = params.set('estadoStock', filtros.estadoStock);
    }

    return this.http.get<readonly ProductoResponse[]>(this.productosUrl, {
      params,
    });
  }

  obtenerPorId(id: number): Observable<ProductoResponse> {
    return this.http.get<ProductoResponse>(`${this.productosUrl}/${id}`);
  }

  crear(request: CrearProductoRequest): Observable<ProductoResponse> {
    return this.http.post<ProductoResponse>(this.productosUrl, request);
  }

  actualizar(
    id: number,
    request: ActualizarProductoRequest,
  ): Observable<ProductoResponse> {
    return this.http.put<ProductoResponse>(
      `${this.productosUrl}/${id}`,
      request,
    );
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.productosUrl}/${id}`);
  }

  listarStockBajo(): Observable<readonly ProductoResponse[]> {
    return this.http.get<readonly ProductoResponse[]>(
      `${this.productosUrl}/stock-bajo`,
    );
  }

  ajustarStock(
    id: number,
    request: AjustarStockRequest,
  ): Observable<AjusteStockResponse> {
    return this.http.patch<AjusteStockResponse>(
      `${this.productosUrl}/${id}/stock`,
      request,
    );
  }

  listarCategorias(): Observable<readonly CategoriaResponse[]> {
    return this.http.get<readonly CategoriaResponse[]>(
      `${this.apiBaseUrl}/categorias`,
    );
  }
}
