import { Component, computed, input, output, signal } from '@angular/core';
import { PastVisitView } from '../../models/profile.models';

@Component({
  selector: 'app-visit-history-list',
  standalone: true,
  templateUrl: './visit-history-list.html',
  styleUrl: './visit-history-list.scss',
})
export class VisitHistoryListComponent {
  readonly visits = input.required<readonly PastVisitView[]>();
  readonly totalCount = input.required<number>();

  readonly bookAgain = output<PastVisitView>();

  readonly isExpanded = signal(false);

  readonly visibleVisits = computed(() =>
    this.isExpanded() ? this.visits() : this.visits().slice(0, 2),
  );

  readonly remainingCount = computed(() =>
    Math.max(0, this.totalCount() - this.visibleVisits().length),
  );

  toggleExpand(): void {
    this.isExpanded.update((v) => !v);
  }
}
