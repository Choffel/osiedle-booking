import { Component, signal } from '@angular/core';
import { generateUpcomingDays } from '../../../../utils/date.utils';
import { DateOption } from '../../../../../models/DateOption';


@Component({
  imports: [],
  selector: 'app-hero-search',
  styleUrl: './hero-search.scss',
  templateUrl: './hero-search.html',
})
export class HeroSearchComponent {
  upcomingDays = generateUpcomingDays();
  selectedDate = signal<DateOption | null>(this.upcomingDays[0] ?? null);

  selectDate(day: DateOption): void {
    this.selectedDate.set(day);
  }
}
