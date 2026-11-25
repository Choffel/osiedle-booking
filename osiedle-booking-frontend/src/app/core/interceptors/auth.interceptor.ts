import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenRefreshService } from '../../features/auth/services/token-refresh.service';
import { TokenService } from '../../features/auth/services/token.service';

const ACCESS_TOKEN_ENDPOINT = '/api/tokens/newaccesstoken';
const AUTHENTICATION_ENDPOINT = '/api/authentication/';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const tokenRefreshService = inject(TokenRefreshService);
  const url = req.url.toLowerCase();

  // Не добавляем/не обновляем токены для самого логина и запроса обновления токена
  if (url.endsWith(ACCESS_TOKEN_ENDPOINT) || url.includes(AUTHENTICATION_ENDPOINT)) {
    return next(req);
  }

  const accessToken = tokenService.getAccessToken();
  const authorizedRequest = accessToken
    ? req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } })
    : req;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        !tokenService.getRefreshToken()
      ) {
        return throwError(() => error);
      }

      return tokenRefreshService.refreshTokens().pipe(
        catchError(() => throwError(() => error)),
        switchMap((tokens) =>
          next(
            req.clone({
              setHeaders: { Authorization: `Bearer ${tokens.accessToken}` },
            }),
          ),
        ),
      );
    }),
  );
};
