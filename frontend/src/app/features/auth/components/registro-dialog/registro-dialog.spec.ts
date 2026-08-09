import { provideZonelessChangeDetection, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';

import { AuthStore } from '../../../../core/auth/auth.store';
import { ProblemDetails } from '../../../../core/models/problem-details.model';
import { RegistroDialog } from './registro-dialog';

describe('RegistroDialog', () => {
  let fixture: ComponentFixture<RegistroDialog>;

  beforeEach(async () => {
    const auth = {
      error: signal<ProblemDetails | null>(null).asReadonly(),
      loading: signal(false).asReadonly(),
      limpiarError: jasmine.createSpy('limpiarError'),
      sugerirUsuario: jasmine
        .createSpy('sugerirUsuario')
        .and.returnValue(Promise.resolve(null)),
      registrar: jasmine
        .createSpy('registrar')
        .and.returnValue(Promise.resolve(null)),
    };
    const dialogRef = jasmine.createSpyObj<MatDialogRef<RegistroDialog>>(
      'MatDialogRef',
      ['close'],
    );

    await TestBed.configureTestingModule({
      imports: [RegistroDialog],
      providers: [
        provideZonelessChangeDetection(),
        { provide: AuthStore, useValue: auth },
        { provide: MatDialogRef, useValue: dialogRef },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegistroDialog);
    fixture.detectChanges();
  });

  it('crea el componente con un formulario de registro semantico', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('section[aria-labelledby]')).not.toBeNull();
    expect(element.querySelectorAll('form input').length).toBe(6);
  });
});
