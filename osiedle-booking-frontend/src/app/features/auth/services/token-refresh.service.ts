import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, finalize, Observable, shareReplay, tap, throwError } from 'rxjs';
import { LoginResponse } from '../models/auth.models';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root',
})
export class TokenRefreshService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);

  private refreshInFlight: Observable<LoginResponse> | null = null;

  refreshTokens(): Observable<LoginResponse> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    const refreshToken = this.tokenService.getRefreshToken();
    if (!refreshToken) {
      this.tokenService.clearTokens();
      return throwError(() => new Error('No refresh token available'));
    }

    this.refreshInFlight = this.http
      .post<LoginResponse>('/api/Tokens/NewAccessToken', null, {
        headers: new HttpHeaders({ 'X-Refresh-Token': refreshToken }),
      })
      .pipe(
        tap((tokens) => this.tokenService.saveTokens(tokens)),
        catchError((error: unknown) => {
          this.tokenService.clearTokens();
          return throwError(() => error);
        }),
        finalize(() => {
          this.refreshInFlight = null;
        }),
        shareReplay(1),
      );

    return this.refreshInFlight;
  }
}
