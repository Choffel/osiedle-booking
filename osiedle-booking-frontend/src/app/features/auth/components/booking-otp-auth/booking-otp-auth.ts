import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiErrorResponse, AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-booking-otp-auth',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './booking-otp-auth.html',
  styleUrl: './booking-otp-auth.scss',
})
export class BookingOtpAuthComponent {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly submitCodeLabel = input<string>('Potwierdź i zarezerwuj wizytę');
  readonly verified = output<void>();

  readonly stage = signal<'details' | 'code'>('details');
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);
  readonly sentToEmail = signal<string>('');
  readonly resendCooldown = signal<number>(0);

  private cooldownTimerId: ReturnType<typeof setInterval> | null = null;

  readonly detailsForm = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2)],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(2)],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    acceptTerms: new FormControl(false, {
      nonNullable: true,
      validators: [Validators.requiredTrue],
    }),
  });

  readonly codeForm = new FormGroup({
    code: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^\d{6}$/)],
    }),
  });

  constructor() {
    this.destroyRef.onDestroy(() => this.clearCooldownTimer());
  }

  onRequestOtp(): void {
    if (this.detailsForm.invalid) {
      this.detailsForm.markAllAsTouched();
      return;
    }

    this.sendOtpRequest(true);
  }

  onResendOtp(): void {
    if (this.resendCooldown() > 0 || this.isLoading()) {
      return;
    }

    this.sendOtpRequest(false);
  }

  onEditDetails(): void {
    this.errorMessage.set(null);
    this.codeForm.reset();
    this.stage.set('details');
  }

  onCodeInput(event: Event): void {
    const inputEl = event.target as HTMLInputElement;
    const digitsOnly = inputEl.value.replace(/\D/g, '').slice(0, 6);
    if (inputEl.value !== digitsOnly) {
      inputEl.value = digitsOnly;
    }
    this.codeForm.controls.code.setValue(digitsOnly, { emitEvent: false });
  }

  onVerifyOtp(): void {
    if (this.codeForm.invalid) {
      this.codeForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .verifyBookingOtp({
        email: this.sentToEmail(),
        code: this.codeForm.controls.code.value.trim(),
      })
      .subscribe({
        next: () => {
          this.isLoading.set(false);
          this.verified.emit();
        },
        error: (err: HttpErrorResponse) => {
          this.isLoading.set(false);
          this.errorMessage.set(
            this.extractError(err, 'Nieprawidłowy lub wygasły kod. Spróbuj ponownie.'),
          );
        },
      });
  }

  private sendOtpRequest(switchStage: boolean): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    const raw = this.detailsForm.getRawValue();
    const payload = {
      firstName: raw.firstName.trim(),
      lastName: raw.lastName.trim(),
      email: raw.email.trim(),
    };

    this.authService.requestBookingOtp(payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.sentToEmail.set(payload.email);
        this.startCooldown(45);
        if (switchStage) {
          this.stage.set('code');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          this.extractError(err, 'Nie udało się wysłać kodu weryfikacyjnego. Spróbuj ponownie.'),
        );
      },
    });
  }

  private startCooldown(seconds: number): void {
    this.clearCooldownTimer();
    this.resendCooldown.set(seconds);

    this.cooldownTimerId = setInterval(() => {
      const next = this.resendCooldown() - 1;
      if (next <= 0) {
        this.resendCooldown.set(0);
        this.clearCooldownTimer();
      } else {
        this.resendCooldown.set(next);
      }
    }, 1000);
  }

  private clearCooldownTimer(): void {
    if (this.cooldownTimerId !== null) {
      clearInterval(this.cooldownTimerId);
      this.cooldownTimerId = null;
    }
  }

  private extractError(err: HttpErrorResponse, fallback: string): string {
    let apiError: ApiErrorResponse | undefined;
    try {
      apiError = typeof err.error === 'string' ? JSON.parse(err.error) : err.error;
    } catch {
      apiError = undefined;
    }
    return apiError?.detail || fallback;
  }
}
