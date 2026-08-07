import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { API_BASE_URL } from '../services/api-base-url.token';
import {
  LoginRequest,
  RegistrarUsuarioRequest,
  SesionResponse,
  SugerenciaUsuarioResponse,
  UsuarioResponse,
} from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${inject(API_BASE_URL)}/auth`;

  login(request: LoginRequest): Observable<SesionResponse> {
    return this.http.post<SesionResponse>(`${this.authUrl}/login`, request);
  }

  registrar(request: RegistrarUsuarioRequest): Observable<UsuarioResponse> {
    return this.http.post<UsuarioResponse>(`${this.authUrl}/registro`, request);
  }

  sugerirUsuario(
    nombre: string,
    apellidos: string,
  ): Observable<SugerenciaUsuarioResponse> {
    const params = new HttpParams()
      .set('nombre', nombre)
      .set('apellidos', apellidos);

    return this.http.get<SugerenciaUsuarioResponse>(
      `${this.authUrl}/sugerencia-usuario`,
      { params },
    );
  }

  obtenerSesion(): Observable<SesionResponse> {
    return this.http.get<SesionResponse>(`${this.authUrl}/sesion`);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.authUrl}/logout`, null);
  }
}
