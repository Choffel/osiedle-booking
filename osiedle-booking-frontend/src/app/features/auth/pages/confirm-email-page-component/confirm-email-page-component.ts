import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

type ConfirmStatus = 'loading' | 'success' | 'error';

@Component({
  imports: [RouterLink],
  selector: 'app-confirm-email-page-component',
  styleUrl: './confirm-email-page-component.scss',
  templateUrl: './confirm-email-page-component.html',
})
export class ConfirmEmailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly status = signal<ConfirmStatus>('loading');
  readonly errorMessage = signal('Link aktywacyjny jest nieprawidłowy lub wygasł.');

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email')?.trim();
    const token = this.route.snapshot.queryParamMap.get('token')?.trim();

    if (!email || !token) {
      this.status.set('error');
      this.errorMessage.set('Brak wymaganych parametrów w adresie URL.');
      return;
    }

    this.authService.confirmEmail(email, token).subscribe({
      next: () => {
        this.status.set('success');

        const timerId = setTimeout(() => {
          this.router.navigateByUrl('/login');
        }, 2500);

        this.destroyRef.onDestroy(() => clearTimeout(timerId));
      },
      error: () => {
        this.status.set('error');
        this.errorMessage.set('Link aktywacyjny jest nieprawidłowy lub wygasł.');
      },
    });
  }
}
