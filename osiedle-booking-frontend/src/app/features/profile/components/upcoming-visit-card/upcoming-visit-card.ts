import { Component, input, output } from '@angular/core';
import { UpcomingVisitView } from '../../models/profile.models';

@Component({
  selector: 'app-upcoming-visit-card',
  standalone: true,
  templateUrl: './upcoming-visit-card.html',
  styleUrl: './upcoming-visit-card.scss',
})
export class UpcomingVisitCardComponent {
  readonly visits = input.required<readonly UpcomingVisitView[]>();

  readonly reschedule = output<UpcomingVisitView>();
  readonly cancel = output<UpcomingVisitView>();
}
