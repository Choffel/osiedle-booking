import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BusinessDetailsDto } from '../../../../core/models/business.dto';

@Component({
  selector: 'app-salon-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './salon-header.html',
  styleUrl: './salon-header.scss',
})
export class SalonHeaderComponent {
  readonly business = input.required<BusinessDetailsDto>();
  readonly coverPhoto = input.required<string>();
}
