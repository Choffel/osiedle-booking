import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PasswordInputComponent } from '../../../../shared/components/password-input/password-input';
import { AuthService, ApiErrorResponse } from '../../services/auth.service';

@Component({
  imports: [RouterLink, PasswordInputComponent, ReactiveFormsModule],
  selector: 'app-login-page',
  styleUrl: './login-page.scss',
  templateUrl: './login-page.html',
})
export class LoginPageComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly serverError = signal<string | null>(null);

  readonly loginForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(6)],
    }),
  });

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.serverError.set(null);

    this.authService.login(this.loginForm.getRawValue()).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);

        const apiError = err.error as ApiErrorResponse | undefined;

        if (apiError?.code === 'Identity.InvalidCredentials' || err.status === 401) {
          this.serverError.set('Nieprawidłowy adres e-mail lub hasło.');
          return;
        }

        this.serverError.set('Wystąpił błąd podczas logowania. Spróbuj ponownie.');
      },
    });
  }
}
