import { Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-profile-hero',
  standalone: true,
  templateUrl: './profile-hero.html',
  styleUrl: './profile-hero.scss',
})
export class ProfileHeroComponent {
  readonly fullName = input.required<string>();
  readonly email = input.required<string>();
  readonly memberSince = input.required<string>();
  readonly avatarUrl = input<string | null>(null);
  readonly isVerified = input<boolean>(false);

  readonly editProfile = output<void>();

  readonly initials = computed(() =>
    this.fullName()
      .split(' ')
      .map((part) => part[0])
      .filter(Boolean)
      .slice(0, 2)
      .join('')
      .toUpperCase(),
  );
}
