import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TokenService } from '../../../auth/services/token.service';
import {
  FavoriteSalonView,
  PastVisitView,
  ReminderOption,
  UpcomingVisitView,
  UserProfileView,
} from '../../models/profile.models';
import { ProfileHeroComponent } from '../../components/profile-hero/profile-hero';
import { ProfileStatsComponent } from '../../components/profile-stats/profile-stats';
import { UpcomingVisitCardComponent } from '../../components/upcoming-visit-card/upcoming-visit-card';
import { VisitHistoryListComponent } from '../../components/visit-history-list/visit-history-list';
import { FavoriteSalonsWidgetComponent } from '../../components/favorite-salons-widget/favorite-salons-widget';
import { EmailNotificationsWidgetComponent } from '../../components/email-notifications-widget/email-notifications-widget';

@Component({
  selector: 'app-profile-page',
  standalone: true,
  imports: [
    RouterLink,
    ProfileHeroComponent,
    ProfileStatsComponent,
    UpcomingVisitCardComponent,
    VisitHistoryListComponent,
    FavoriteSalonsWidgetComponent,
    EmailNotificationsWidgetComponent,
  ],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.scss',
})
export class ProfilePageComponent {
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  readonly profile = signal<UserProfileView>({
    fullName: 'Jan Kowalski',
    email: 'jan.kowalski@example.com',
    memberSince: 'Kwiecień 2023',
    avatarUrl: null,
    isVerified: true,
    completedVisitsCount: 8,
    upcomingVisitsCount: 1,
    favoriteSalonsCount: 2,
  });

  readonly upcomingVisits = signal<readonly UpcomingVisitView[]>([
    {
      bookingId: '1',
      businessId: '20000000-0000-0000-0000-000000000001',
      categoryLabel: 'BARBERSHOP WARSZAWA',
      salonName: 'Kosa & Brzytwa Barber Studio',
      address: 'ul. Marszałkowska 84, Warszawa (Śródmieście)',
      coverUrl: null,
      statusLabel: 'Potwierdzona natychmiast',
      price: 140,
      paymentNote: 'Płatność na miejscu',
      serviceName: 'Combo Klasyk (Włosy + Broda)',
      durationMinutes: 75,
      dateLabel: 'Sobota, 24 maja 2025',
      timeRangeLabel: 'godz. 14:30 - 15:45',
      staffName: 'Barber Marek',
      staffInitial: 'M',
      staffRole: 'Mistrz brzytwy',
    },
  ]);

  readonly pastVisits = signal<readonly PastVisitView[]>([
    {
      bookingId: '101',
      businessId: '20000000-0000-0000-0000-000000000002',
      serviceId: 's-1',
      dayNumber: '18',
      monthShort: 'KWI',
      salonName: 'Warsaw Gentlemen Club',
      statusLabel: 'Zrealizowana',
      serviceName: 'Strzyżenie Skin Fade',
      durationMinutes: 45,
      price: 100,
      staffName: 'Kamil',
      fullDateTimeLabel: '18 kwietnia 2025, 11:00',
      rating: 5.0,
    },
    {
      bookingId: '102',
      businessId: '20000000-0000-0000-0000-000000000001',
      serviceId: 's-2',
      dayNumber: '12',
      monthShort: 'MAR',
      salonName: 'Kosa & Brzytwa Barber Studio',
      statusLabel: 'Zrealizowana',
      serviceName: 'Combo Klasyk',
      durationMinutes: 75,
      price: 140,
      staffName: 'Marek',
      fullDateTimeLabel: '12 marca 2025, 17:30',
      rating: null,
    },
  ]);

  readonly favoriteSalons = signal<readonly FavoriteSalonView[]>([
    {
      businessId: '20000000-0000-0000-0000-000000000001',
      name: 'Kosa & Brzytwa Barber Studio',
      locationLabel: 'Warszawa, Śródmieście',
      distanceLabel: '1.2 km',
      rating: 4.9,
    },
    {
      businessId: '20000000-0000-0000-0000-000000000003',
      name: 'Rzemieślnicy Włosów & Brody',
      locationLabel: 'Warszawa, Powiśle',
      distanceLabel: '2.4 km',
      rating: 4.95,
    },
  ]);

  readonly reminderOptions: readonly ReminderOption[] = [
    { hours: 1, label: '1 godzina', shortHint: '1h' },
    { hours: 2, label: '2 godz. (domyślnie)', shortHint: '2h' },
    { hours: 4, label: '4 godziny', shortHint: '4h' },
    { hours: 24, label: '24 godziny', shortHint: '24h' },
  ];

  readonly emailRemindersEnabled = signal<boolean>(true);
  readonly selectedReminderHours = signal<number>(2);

  onEditProfile(): void {
    console.log('Edytuj profil');
  }

  onRescheduleVisit(visit: UpcomingVisitView): void {
    this.router.navigate(['/salon', visit.businessId]);
  }

  onCancelVisit(visit: UpcomingVisitView): void {
    console.log('Odwołaj wizytę:', visit.bookingId);
  }

  onBookAgain(visit: PastVisitView): void {
    this.router.navigate(['/salon', visit.businessId]);
  }

  onLogout(): void {
    this.tokenService.clearTokens();
    this.router.navigate(['/']);
  }
}
