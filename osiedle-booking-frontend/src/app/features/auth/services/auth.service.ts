import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/auth.models';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);

  readonly isAuthenticated = this.tokenService.isAuthenticated;

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/Authentication/login', payload).pipe(
      tap((response) => {
        this.tokenService.saveTokens(response);
      }),
    );
  }

  register(payload: RegisterRequest): Observable<string> {
    return this.http.post('/api/Authentication/register', payload, {
      responseType: 'text',
    });
  }

  logout(): void {
    this.tokenService.clearTokens();
  }

  confirmEmail(email: string, token: string): Observable<string> {
    return this.http.post(
      '/api/Authentication/confirm-email',
      { email, token },
      { responseType: 'text' },
    );
  }
}

export interface ApiErrorResponse {
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
}
