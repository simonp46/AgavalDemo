import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AuthApiService } from './auth-api.service';
import { UsuarioResponse } from './auth.models';
import { AuthStore } from './auth.store';

describe('AuthStore', () => {
  const usuario: UsuarioResponse = {
    id: 1,
    nombre: 'Ana',
    apellidos: 'Pérez',
    numeroDocumento: '123',
    area: 'Compras',
    usuario: 'anaperez00',
    fechaCreacion: '2026-08-07T00:00:00Z',
  };

  let api: jasmine.SpyObj<AuthApiService>;

  beforeEach(() => {
    api = jasmine.createSpyObj<AuthApiService>('AuthApiService', [
      'login',
      'registrar',
      'sugerirUsuario',
      'obtenerSesion',
      'logout',
    ]);
    TestBed.configureTestingModule({
      providers: [
        AuthStore,
        provideZonelessChangeDetection(),
        { provide: AuthApiService, useValue: api },
      ],
    });
  });

  it('establece la sesión después de un login correcto', async () => {
    api.login.and.returnValue(of({ usuario }));
    const store = TestBed.inject(AuthStore);

    const result = await store.login({
      usuario: 'anaperez00',
      contrasena: 'ClaveSegura',
    });

    expect(result).toBeTrue();
    expect(store.autenticado()).toBeTrue();
    expect(store.nombreCompleto()).toBe('Ana Pérez');
  });

  it('restaura una sesión existente una sola vez', async () => {
    api.obtenerSesion.and.returnValue(of({ usuario }));
    const store = TestBed.inject(AuthStore);

    expect(await store.asegurarSesion()).toBeTrue();
    expect(await store.asegurarSesion()).toBeTrue();
    expect(api.obtenerSesion).toHaveBeenCalledTimes(1);
  });

  it('limpia el usuario al cerrar sesión', async () => {
    api.login.and.returnValue(of({ usuario }));
    api.logout.and.returnValue(of(undefined));
    const store = TestBed.inject(AuthStore);

    await store.login({ usuario: 'anaperez00', contrasena: 'ClaveSegura' });
    await store.logout();

    expect(store.autenticado()).toBeFalse();
    expect(store.usuario()).toBeNull();
  });
});
