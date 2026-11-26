export interface UserProfileView {
  readonly fullName: string;
  readonly email: string;
  readonly memberSince: string;
  readonly avatarUrl: string | null;
  readonly isVerified: boolean;
  readonly completedVisitsCount: number;
  readonly upcomingVisitsCount: number;
  readonly favoriteSalonsCount: number;
}

export interface UpcomingVisitView {
  readonly bookingId: string;
  readonly businessId: string;
  readonly categoryLabel: string;
  readonly salonName: string;
  readonly address: string;
  readonly coverUrl: string | null;
  readonly statusLabel: string;
  readonly price: number;
  readonly paymentNote: string;
  readonly serviceName: string;
  readonly durationMinutes: number;
  readonly dateLabel: string;
  readonly timeRangeLabel: string;
  readonly staffName: string;
  readonly staffInitial: string;
  readonly staffRole: string;
}

export interface PastVisitView {
  readonly bookingId: string;
  readonly businessId: string;
  readonly serviceId: string;
  readonly dayNumber: string;
  readonly monthShort: string;
  readonly salonName: string;
  readonly statusLabel: string;
  readonly serviceName: string;
  readonly durationMinutes: number;
  readonly price: number;
  readonly staffName: string;
  readonly fullDateTimeLabel: string;
  readonly rating: number | null;
}

export interface FavoriteSalonView {
  readonly businessId: string;
  readonly name: string;
  readonly locationLabel: string;
  readonly distanceLabel: string;
  readonly rating: number;
}

export interface ReminderOption {
  readonly hours: number;
  readonly label: string;
  readonly shortHint: string;
}
