import { computed, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom, Observable } from 'rxjs';

import {
  ApiProblemError,
} from '../interceptors/problem-details.interceptor';
import { ProblemDetails } from '../models/problem-details.model';
import { AuthApiService } from './auth-api.service';
import {
  LoginRequest,
  RegistrarUsuarioRequest,
  UsuarioResponse,
} from './auth.models';

type OperationResult<T> =
  | { readonly ok: true; readonly value: T }
  | { readonly ok: false };

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(AuthApiService);
  private readonly usuarioState = signal<UsuarioResponse | null>(null);
  private readonly loadingState = signal(false);
  private readonly initializedState = signal(false);
  private readonly errorState = signal<ProblemDetails | null>(null);
  private sessionPromise: Promise<boolean> | null = null;

  readonly usuario = this.usuarioState.asReadonly();
  readonly loading = this.loadingState.asReadonly();
  readonly initialized = this.initializedState.asReadonly();
  readonly error = this.errorState.asReadonly();
  readonly autenticado = computed(() => this.usuario() !== null);
  readonly nombreCompleto = computed(() => {
    const usuario = this.usuario();
    return usuario === null
      ? ''
      : `${usuario.nombre} ${usuario.apellidos}`.trim();
  });

  async asegurarSesion(): Promise<boolean> {
    if (this.initialized()) {
      return this.autenticado();
    }

    if (this.sessionPromise !== null) {
      return this.sessionPromise;
    }

    this.sessionPromise = this.cargarSesion();

    try {
      return await this.sessionPromise;
    } finally {
      this.sessionPromise = null;
    }
  }

  async login(request: LoginRequest): Promise<boolean> {
    const result = await this.execute(this.api.login(request));

    if (!result.ok) {
      return false;
    }

    this.usuarioState.set(result.value.usuario);
    this.initializedState.set(true);
    return true;
  }

  async registrar(
    request: RegistrarUsuarioRequest,
  ): Promise<UsuarioResponse | null> {
    const result = await this.execute(this.api.registrar(request));
    return result.ok ? result.value : null;
  }

  async sugerirUsuario(
    nombre: string,
    apellidos: string,
  ): Promise<string | null> {
    const result = await this.execute(
      this.api.sugerirUsuario(nombre, apellidos),
    );
    return result.ok ? result.value.usuario : null;
  }

  async logout(): Promise<void> {
    this.loadingState.set(true);

    try {
      await firstValueFrom(this.api.logout());
    } catch {
      // La sesión local se elimina aun si el servidor ya la invalidó.
    } finally {
      this.usuarioState.set(null);
      this.initializedState.set(true);
      this.errorState.set(null);
      this.loadingState.set(false);
    }
  }

  limpiarError(): void {
    this.errorState.set(null);
  }

  private async cargarSesion(): Promise<boolean> {
    this.loadingState.set(true);

    try {
      const response = await firstValueFrom(this.api.obtenerSesion());
      this.usuarioState.set(response.usuario);
      return true;
    } catch {
      this.usuarioState.set(null);
      return false;
    } finally {
      this.initializedState.set(true);
      this.loadingState.set(false);
    }
  }

  private async execute<T>(
    source: Observable<T>,
  ): Promise<OperationResult<T>> {
    this.errorState.set(null);
    this.loadingState.set(true);

    try {
      return { ok: true, value: await firstValueFrom(source) };
    } catch (error: unknown) {
      this.errorState.set(this.toProblemDetails(error));
      return { ok: false };
    } finally {
      this.loadingState.set(false);
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
