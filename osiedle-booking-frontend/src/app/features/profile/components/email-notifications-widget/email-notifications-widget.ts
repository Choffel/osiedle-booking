import { Component, computed, input, output } from '@angular/core';
import { ReminderOption } from '../../models/profile.models';

@Component({
  selector: 'app-email-notifications-widget',
  standalone: true,
  templateUrl: './email-notifications-widget.html',
  styleUrl: './email-notifications-widget.scss',
})
export class EmailNotificationsWidgetComponent {
  readonly enabled = input.required<boolean>();
  readonly selectedHours = input.required<number>();
  readonly options = input.required<readonly ReminderOption[]>();

  readonly enabledChange = output<boolean>();
  readonly hoursChange = output<number>();

  readonly activeHint = computed(() => {
    const hours = this.selectedHours();
    return this.options().find((o) => o.hours === hours)?.shortHint ?? `${hours}h`;
  });

  onSelectHours(hours: number): void {
    if (!this.enabled()) return;
    this.hoursChange.emit(hours);
  }
}
