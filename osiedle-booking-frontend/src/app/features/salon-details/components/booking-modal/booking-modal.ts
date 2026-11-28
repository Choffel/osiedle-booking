import { Component, computed, inject, input, output, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, combineLatest, of, startWith, switchMap, tap } from 'rxjs';
import { BusinessEmployeeDto, BusinessServiceItemDto } from '../../../../core/models/business.dto';
import { AvailableSlotDto } from '../../../../core/models/booking.dto';
import { BookingsService } from '../../../../core/services/bookings.service';
import { AuthService } from '../../../auth/services/auth.service';
import { BookingOtpAuthComponent } from '../../../auth/components/booking-otp-auth/booking-otp-auth';

interface DayOption {
  readonly isoDate: string;
  readonly isToday: boolean;
  readonly dayShort: string;
  readonly dayFull: string;
  readonly dayNumber: number;
  readonly monthShort: string;
  readonly monthGenitive: string;
}

export interface TimeSlotView {
  readonly label: string;
  readonly slot: AvailableSlotDto;
}

export interface BookingSelectionResult {
  readonly service: BusinessServiceItemDto;
  readonly slot: AvailableSlotDto;
}

const DAYS_SHORT = ['Nd', 'Pn', 'Wt', 'Śr', 'Cz', 'Pt', 'Sob'];
const DAYS_FULL = ['Niedziela', 'Poniedziałek', 'Wtorek', 'Środa', 'Czwartek', 'Piątek', 'Sobota'];
const MONTHS_SHORT = [
  'sty',
  'lut',
  'mar',
  'kwi',
  'maj',
  'cze',
  'lip',
  'sie',
  'wrz',
  'paź',
  'lis',
  'gru',
];
const MONTHS_GENITIVE = [
  'stycznia',
  'lutego',
  'marca',
  'kwietnia',
  'maja',
  'czerwca',
  'lipca',
  'sierpnia',
  'września',
  'października',
  'listopada',
  'grudnia',
];

@Component({
  selector: 'app-booking-modal',
  standalone: true,
  imports: [BookingOtpAuthComponent],
  templateUrl: './booking-modal.html',
  styleUrl: './booking-modal.scss',
})
export class BookingModalComponent {
  private readonly bookingsService = inject(BookingsService);
  private readonly authService = inject(AuthService);

  readonly businessId = input.required<string>();
  readonly service = input.required<BusinessServiceItemDto>();
  readonly employee = input<BusinessEmployeeDto | null>(null);

  readonly closeModal = output<void>();
  readonly confirmSelection = output<BookingSelectionResult>();

  readonly isAuthenticated = this.authService.isAuthenticated;

  readonly step = signal<1 | 2>(1);

  readonly days: readonly DayOption[] = this.buildNextDays(5);
  readonly selectedDate = signal<string>(this.days[0].isoDate);
  readonly selectedSlot = signal<AvailableSlotDto | null>(null);
  readonly isLoadingSlots = signal<boolean>(true);

  readonly isSubmitting = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  private readonly rawSlots = toSignal(
    combineLatest([toObservable(this.service), toObservable(this.selectedDate)]).pipe(
      tap(() => {
        this.isLoadingSlots.set(true);
        this.selectedSlot.set(null);
      }),
      switchMap(([service, date]) =>
        this.bookingsService
          .getAvailableSlots(service.employeeId, service.bookableServiceId, date)
          .pipe(
            catchError(() => of([] as readonly AvailableSlotDto[])),
            tap((slots) => {
              this.isLoadingSlots.set(false);
              if (slots.length > 0) {
                this.selectedSlot.set(slots[0]);
              }
            }),
          ),
      ),
      startWith([] as readonly AvailableSlotDto[]),
    ),
    { initialValue: [] as readonly AvailableSlotDto[] },
  );

  readonly groupedSlots = computed(() => {
    const morning: TimeSlotView[] = [];
    const afternoon: TimeSlotView[] = [];
    const evening: TimeSlotView[] = [];

    for (const slot of this.rawSlots()) {
      const timePart = slot.start.split('T')[1] ?? '';
      const label = timePart.slice(0, 5);
      const hour = Number(timePart.slice(0, 2));

      const item: TimeSlotView = { label, slot };

      if (hour < 12) {
        morning.push(item);
      } else if (hour < 17) {
        afternoon.push(item);
      } else {
        evening.push(item);
      }
    }

    return { morning, afternoon, evening };
  });

  readonly hasAnySlots = computed(() => this.rawSlots().length > 0);

  readonly selectedTimeLabel = computed(() => {
    const slot = this.selectedSlot();
    if (!slot) return null;
    return (slot.start.split('T')[1] ?? '').slice(0, 5);
  });

  readonly summaryDateText = computed(() => {
    const iso = this.selectedDate();
    const matched = this.days.find((d) => d.isoDate === iso);
    if (matched) {
      return `${matched.dayFull}, ${matched.dayNumber} ${matched.monthGenitive}`;
    }
    const parsed = this.parseIsoDate(iso);
    return `${DAYS_FULL[parsed.getDay()]}, ${parsed.getDate()} ${MONTHS_GENITIVE[parsed.getMonth()]}`;
  });

  formatDuration(duration: string): string {
    const [hours = 0, minutes = 0] = duration.split(':').map(Number);
    return `${hours * 60 + minutes} min`;
  }

  onDatePickerChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    if (value) {
      this.selectedDate.set(value);
    }
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeModal.emit();
    }
  }

  goToLoginStep(): void {
    const slot = this.selectedSlot();
    if (!slot) return;

    this.errorMessage.set(null);

    if (this.isAuthenticated()) {
      this.submitBooking(slot);
      return;
    }

    this.step.set(2);
  }

  goBackToSlotStep(): void {
    this.errorMessage.set(null);
    this.step.set(1);
  }

  onOtpVerified(): void {
    const slot = this.selectedSlot();
    if (!slot) return;

    this.submitBooking(slot);
  }

  private submitBooking(slot: AvailableSlotDto): void {
    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.bookingsService
      .createBooking({
        businessId: this.businessId(),
        staffId: this.service().employeeId,
        serviceId: this.service().bookableServiceId,
        startTime: slot.start,
        endTime: slot.end,
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.confirmSelection.emit({
            service: this.service(),
            slot,
          });
        },
        error: (err) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(
            err?.error?.detail ?? 'Nie udało się zarezerwować wizyty. Wybierz inny termin.',
          );
        },
      });
  }

  private buildNextDays(count: number): DayOption[] {
    const result: DayOption[] = [];
    const today = new Date();

    for (let i = 0; i < count; i++) {
      const date = new Date(today.getFullYear(), today.getMonth(), today.getDate() + i);
      result.push({
        isoDate: this.toIsoDate(date),
        isToday: i === 0,
        dayShort: DAYS_SHORT[date.getDay()],
        dayFull: DAYS_FULL[date.getDay()],
        dayNumber: date.getDate(),
        monthShort: MONTHS_SHORT[date.getMonth()],
        monthGenitive: MONTHS_GENITIVE[date.getMonth()],
      });
    }

    return result;
  }

  private toIsoDate(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

  private parseIsoDate(iso: string): Date {
    const [y, m, d] = iso.split('-').map(Number);
    return new Date(y, m - 1, d);
  }
}
