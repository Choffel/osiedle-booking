import { Component, input, output } from '@angular/core';
import { BusinessServiceItemDto } from '../../../../core/models/business.dto';

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [],
  templateUrl: './service-list.html',
  styleUrl: './service-list.scss',
})
export class ServiceListComponent {
  readonly services = input.required<readonly BusinessServiceItemDto[]>();
  readonly selectService = output<BusinessServiceItemDto>();

  formatDuration(duration: string): string {
    const [hours = 0, minutes = 0] = duration.split(':').map(Number);
    return `${hours * 60 + minutes} min`;
  }
}
