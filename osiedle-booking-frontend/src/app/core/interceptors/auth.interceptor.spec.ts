import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TokenService } from '../../features/auth/services/token.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let tokenService: TokenService;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('adds the access token to outgoing requests', () => {
    tokenService.saveTokens({ accessToken: 'old-access', refreshToken: 'refresh' });

    http.get('/api/Bookings').subscribe();

    const request = httpMock.expectOne('/api/Bookings');
    expect(request.request.headers.get('Authorization')).toBe('Bearer old-access');
    request.flush({});
  });

  it('refreshes tokens on 401 and retries the original request', () => {
    tokenService.saveTokens({ accessToken: 'expired', refreshToken: 'refresh' });

    http.get('/api/Bookings').subscribe();

    const original = httpMock.expectOne('/api/Bookings');
    expect(original.request.headers.get('Authorization')).toBe('Bearer expired');
    original.flush('unauthorized', { status: 401, statusText: 'Unauthorized' });

    const refresh = httpMock.expectOne('/api/Tokens/NewAccessToken');
    expect(refresh.request.method).toBe('POST');
    expect(refresh.request.headers.get('X-Refresh-Token')).toBe('refresh');
    refresh.flush({ accessToken: 'new-access', refreshToken: 'new-refresh' });

    const retry = httpMock.expectOne('/api/Bookings');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer new-access');
    retry.flush({});

    expect(tokenService.getAccessToken()).toBe('new-access');
    expect(tokenService.getRefreshToken()).toBe('new-refresh');
  });

  it('uses a single refresh request for parallel 401 responses', () => {
    tokenService.saveTokens({ accessToken: 'expired', refreshToken: 'refresh' });

    http.get('/api/A').subscribe();
    http.get('/api/B').subscribe();

    httpMock.expectOne('/api/A').flush('unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });
    httpMock.expectOne('/api/B').flush('unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });

    const refresh = httpMock.expectOne('/api/Tokens/NewAccessToken');
    refresh.flush({ accessToken: 'new-access', refreshToken: 'new-refresh' });

    const retryA = httpMock.expectOne('/api/A');
    const retryB = httpMock.expectOne('/api/B');
    expect(retryA.request.headers.get('Authorization')).toBe('Bearer new-access');
    expect(retryB.request.headers.get('Authorization')).toBe('Bearer new-access');
    retryA.flush({});
    retryB.flush({});
  });

  it('does not try to refresh a failed login request', () => {
    const errors: unknown[] = [];

    http.post('/api/Authentication/login', { email: 'a', password: 'b' }).subscribe({
      error: (error: unknown) => errors.push(error),
    });

    httpMock.expectOne('/api/Authentication/login').flush('unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });

    httpMock.expectNone('/api/Tokens/NewAccessToken');
    expect(errors.length).toBe(1);
  });

  it('clears tokens when refreshing fails', () => {
    const errors: number[] = [];
    tokenService.saveTokens({ accessToken: 'expired', refreshToken: 'refresh' });

    http.get('/api/Bookings').subscribe({
      error: (error: { status: number }) => errors.push(error.status),
    });

    httpMock.expectOne('/api/Bookings').flush('unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });
    httpMock.expectOne('/api/Tokens/NewAccessToken').flush('invalid refresh token', {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(errors).toEqual([401]);
    expect(tokenService.getAccessToken()).toBeNull();
    expect(tokenService.getRefreshToken()).toBeNull();
  });
});
