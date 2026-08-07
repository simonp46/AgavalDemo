export type EstadoStock = 'bajo' | 'normal';

export type TipoAjusteStock = 'ENTRADA' | 'SALIDA';

export interface CrearProductoRequest {
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly precio: number;
  readonly stock: number;
  readonly stockMinimo: number;
  readonly categoriaId: number;
}

export interface ActualizarProductoRequest {
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly precio: number;
  readonly stock: number;
  readonly stockMinimo: number;
  readonly categoriaId: number;
}

export interface AjustarStockRequest {
  readonly tipo: TipoAjusteStock;
  readonly cantidad: number;
}

export interface ProductoResponse {
  readonly id: number;
  readonly nombre: string;
  readonly descripcion: string | null;
  readonly precio: number;
  readonly stock: number;
  readonly stockMinimo: number;
  readonly categoriaId: number;
  readonly categoriaNombre: string;
  readonly fechaCreacion: string;
  readonly esStockBajo: boolean;
}

export interface AjusteStockResponse {
  readonly productoId: number;
  readonly stockAnterior: number;
  readonly tipo: TipoAjusteStock;
  readonly cantidad: number;
  readonly stockActual: number;
  readonly esStockBajo: boolean;
}

export interface ListarProductosQueryParams {
  readonly categoriaId?: number;
  readonly estadoStock?: EstadoStock;
}
