import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.token;
    const isAuthRequest = req.url.includes('/api/auth/login');
    if (!token) {
      return next.handle(req).pipe(catchError(error => this.handleAuthError(error, isAuthRequest)));
    }

    const authenticatedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    return next.handle(authenticatedRequest).pipe(catchError(error => this.handleAuthError(error, isAuthRequest)));
  }

  private handleAuthError(error: unknown, isAuthRequest: boolean): Observable<never> {
    if (error instanceof HttpErrorResponse && (error.status === 401 || error.status === 403) && !isAuthRequest) {
      this.authService.logout();
      this.snackBar.open('Tu sesión ha expirado o no tienes permisos. Inicia sesión nuevamente.', 'Cerrar', {
        duration: 3500
      });
      this.router.navigate(['/login']);
    }

    return throwError(() => error);
  }
}
