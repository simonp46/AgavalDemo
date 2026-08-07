export interface LoginRequest {
  readonly usuario: string;
  readonly contrasena: string;
}

export interface RegistrarUsuarioRequest {
  readonly nombre: string;
  readonly apellidos: string;
  readonly numeroDocumento: string;
  readonly area: string;
  readonly usuario: string;
  readonly contrasena: string;
}

export interface UsuarioResponse {
  readonly id: number;
  readonly nombre: string;
  readonly apellidos: string;
  readonly numeroDocumento: string;
  readonly area: string;
  readonly usuario: string;
  readonly fechaCreacion: string;
}

export interface SesionResponse {
  readonly usuario: UsuarioResponse;
}

export interface SugerenciaUsuarioResponse {
  readonly usuario: string;
}
