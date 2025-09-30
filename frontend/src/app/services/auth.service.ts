import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  roles: string[];
  token: string;
  expiresAtUtc: string;
}

export interface AuthState {
  userId: string;
  email: string;
  roles: string[];
  token: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'iam-auth-state';
  private readonly stateSubject = new BehaviorSubject<AuthState | null>(this.loadInitialState());

  readonly authState$ = this.stateSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  get token(): string | null {
    return this.stateSubject.value?.token ?? null;
  }

  get roles(): string[] {
    return this.stateSubject.value?.roles ?? [];
  }

  login(request: LoginRequest): Observable<AuthState> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrls.iam}/api/auth/login`, request)
      .pipe(
        tap(response => this.setState({
          userId: response.userId,
          email: response.email,
          roles: response.roles,
          token: response.token,
          expiresAtUtc: response.expiresAtUtc
        })),
        map(() => this.stateSubject.value as AuthState)
      );
  }

  logout(): void {
    this.setState(null);
  }

  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }

  private setState(state: AuthState | null): void {
    this.stateSubject.next(state);

    if (state) {
      localStorage.setItem(this.storageKey, JSON.stringify(state));
    } else {
      localStorage.removeItem(this.storageKey);
    }
  }

  private loadInitialState(): AuthState | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as AuthState;
      const expiration = new Date(parsed.expiresAtUtc);
      if (Number.isNaN(expiration.getTime()) || expiration <= new Date()) {
        localStorage.removeItem(this.storageKey);
        return null;
      }

      return parsed;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
