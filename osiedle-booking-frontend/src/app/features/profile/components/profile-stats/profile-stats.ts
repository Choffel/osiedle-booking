import { Component, input } from '@angular/core';

@Component({
  selector: 'app-profile-stats',
  standalone: true,
  templateUrl: './profile-stats.html',
  styleUrl: './profile-stats.scss',
})
export class ProfileStatsComponent {
  readonly completedCount = input.required<number>();
  readonly upcomingCount = input.required<number>();
  readonly favoritesCount = input.required<number>();
}
