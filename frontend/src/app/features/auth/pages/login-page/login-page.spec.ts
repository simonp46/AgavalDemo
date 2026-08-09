import { provideZonelessChangeDetection, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthStore } from '../../../../core/auth/auth.store';
import { ProblemDetails } from '../../../../core/models/problem-details.model';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;

  beforeEach(async () => {
    const auth = {
      error: signal<ProblemDetails | null>(null).asReadonly(),
      loading: signal(false).asReadonly(),
      limpiarError: jasmine.createSpy('limpiarError'),
      asegurarSesion: jasmine
        .createSpy('asegurarSesion')
        .and.returnValue(Promise.resolve(false)),
      login: jasmine
        .createSpy('login')
        .and.returnValue(Promise.resolve(false)),
    };

    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        provideRouter([]),
        provideZonelessChangeDetection(),
        { provide: AuthStore, useValue: auth },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('crea la pagina con encabezado y formulario de acceso', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('.login-page > header')).not.toBeNull();
    expect(element.querySelector('form')).not.toBeNull();
  });
});
