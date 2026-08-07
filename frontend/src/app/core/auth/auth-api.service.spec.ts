import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { API_BASE_URL } from '../services/api-base-url.token';
import { AuthApiService } from './auth-api.service';

describe('AuthApiService', () => {
  let service: AuthApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthApiService,
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: '/api' },
      ],
    });

    service = TestBed.inject(AuthApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('envía las credenciales al endpoint de login', () => {
    service.login({ usuario: 'anaperez00', contrasena: 'ClaveSegura' })
      .subscribe();

    const request = http.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body.usuario).toBe('anaperez00');
    request.flush({
      usuario: {
        id: 1,
        nombre: 'Ana',
        apellidos: 'Pérez',
        numeroDocumento: '123',
        area: 'Compras',
        usuario: 'anaperez00',
        fechaCreacion: '2026-08-07T00:00:00Z',
      },
    });
  });

  it('solicita una sugerencia con nombre y apellidos', () => {
    service.sugerirUsuario('Ana', 'Pérez').subscribe();

    const request = http.expectOne(
      (candidate) =>
        candidate.url === '/api/auth/sugerencia-usuario' &&
        candidate.params.get('nombre') === 'Ana' &&
        candidate.params.get('apellidos') === 'Pérez',
    );
    expect(request.request.method).toBe('GET');
    request.flush({ usuario: 'anaperez00' });
  });
});
