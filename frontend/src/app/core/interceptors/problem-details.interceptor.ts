import {
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { ProblemDetails } from '../models/problem-details.model';

export class ApiProblemError extends Error {
  constructor(readonly problem: ProblemDetails) {
    super(problem.detail);
    this.name = 'ApiProblemError';
  }
}

export const problemDetailsInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof ApiProblemError) {
        return throwError(() => error);
      }

      const problem =
        error instanceof HttpErrorResponse
          ? normalizeHttpError(error)
          : createFallbackProblem();

      return throwError(() => new ApiProblemError(problem));
    }),
  );

function normalizeHttpError(error: HttpErrorResponse): ProblemDetails {
  if (isProblemDetails(error.error)) {
    return error.error;
  }

  const serviceUnavailable = [0, 502, 503, 504].includes(error.status);

  return {
    type: 'urn:gestor-inventario:problem:unexpected-error',
    title: serviceUnavailable
      ? 'No fue posible conectar con el servicio.'
      : 'Ocurrió un error inesperado.',
    status: error.status || 503,
    detail: serviceUnavailable
      ? 'Verifique que la API esté disponible e intente nuevamente.'
      : 'No fue posible completar la solicitud.',
    instance: error.url ?? '',
    code: serviceUnavailable ? 'service_unavailable' : 'unexpected_error',
    traceId: '',
  };
}

function createFallbackProblem(): ProblemDetails {
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

function isProblemDetails(value: unknown): value is ProblemDetails {
  if (!isRecord(value)) {
    return false;
  }

  return (
    typeof value['type'] === 'string' &&
    typeof value['title'] === 'string' &&
    typeof value['status'] === 'number' &&
    typeof value['detail'] === 'string' &&
    typeof value['instance'] === 'string' &&
    typeof value['code'] === 'string' &&
    typeof value['traceId'] === 'string' &&
    (value['errors'] === undefined || isValidationErrors(value['errors']))
  );
}

function isValidationErrors(
  value: unknown,
): value is Readonly<Record<string, readonly string[]>> {
  if (!isRecord(value)) {
    return false;
  }

  return Object.values(value).every(
    (messages) =>
      Array.isArray(messages) &&
      messages.every((message) => typeof message === 'string'),
  );
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}
