import { HttpInterceptorFn } from '@angular/common/http';

export const authCredentialsInterceptor: HttpInterceptorFn = (request, next) =>
  next(request.clone({ withCredentials: true }));
