import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProblemDetails } from '../../../core/models/problem-details.model';
import { ApiErrorPanel } from './api-error-panel';

const PROBLEM: ProblemDetails = {
  type: 'urn:gestor-inventario:problem:test',
  title: 'No fue posible cargar los datos',
  status: 500,
  detail: 'Intenta nuevamente.',
  instance: '/api/productos',
  code: 'test_error',
  traceId: 'trace-test',
};

describe('ApiErrorPanel', () => {
  let fixture: ComponentFixture<ApiErrorPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApiErrorPanel],
      providers: [provideZonelessChangeDetection()],
    }).compileComponents();

    fixture = TestBed.createComponent(ApiErrorPanel);
    fixture.componentRef.setInput('problem', PROBLEM);
    fixture.detectChanges();
  });

  it('crea un panel semantico con el detalle del problema', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('section[role="alert"]')).not.toBeNull();
    expect(element.textContent).toContain(PROBLEM.title);
    expect(element.textContent).toContain(PROBLEM.detail);
  });
});
