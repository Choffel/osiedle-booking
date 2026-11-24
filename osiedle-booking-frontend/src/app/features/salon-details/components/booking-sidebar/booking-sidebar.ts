import { Component, input } from '@angular/core';
import { BusinessDetailsDto } from '../../../../core/models/business.dto';

@Component({
  selector: 'app-booking-sidebar',
  standalone: true,
  imports: [],
  templateUrl: './booking-sidebar.html',
  styleUrl: './booking-sidebar.scss',
})
export class BookingSidebarComponent {
  readonly business = input.required<BusinessDetailsDto>();
}
