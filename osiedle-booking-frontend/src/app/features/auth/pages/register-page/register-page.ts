import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiErrorResponse, AuthService } from '../../services/auth.service';
import { PasswordInputComponent } from '../../../../shared/components/password-input/password-input';

@Component({
  imports: [RouterLink, PasswordInputComponent, ReactiveFormsModule],
  selector: 'app-register-page',
  styleUrl: './register-page.scss',
  templateUrl: './register-page.html',
})
export class RegisterPageComponent {
  private readonly authService = inject(AuthService);

  readonly isLoading = signal(false);
  readonly serverError = signal<string | null>(null);
  readonly isRegistered = signal(false);
  readonly registeredEmail = signal('');

  readonly registerForm = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
      ],
    }),
    acceptTerms: new FormControl(false, {
      nonNullable: true,
      validators: [Validators.requiredTrue],
    }),
  });

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.serverError.set(null);

    const raw = this.registerForm.getRawValue();
    const payload = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      email: raw.email.trim(),
      password: raw.password,
    };

    this.authService.register(payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.registeredEmail.set(payload.email);
        this.isRegistered.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.serverError.set(this.mapError(err));
      },
    });
  }

  private mapError(err: HttpErrorResponse): string {
    let apiError: ApiErrorResponse | undefined;
    try {
      apiError = typeof err.error === 'string' ? JSON.parse(err.error) : err.error;
    } catch {
      apiError = undefined;
    }

    switch (apiError?.code) {
      case 'Identity.DuplicateEmail':
      case 'User.EmailAlreadyExists':
        return 'Konto z tym adresem e-mail już istnieje.';
      default:
        return apiError?.detail || 'Wystąpił błąd podczas rejestracji. Spróbuj ponownie.';
    }
  }
}
