import { Component, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { BusinessDto } from '../../../core/models/business.dto';

@Component({
  selector: 'app-salon-card',
  standalone: true,
  imports: [DecimalPipe, RouterLink],
  templateUrl: './salon-card.html',
  styleUrl: './salon-card.scss',
})
export class SalonCardComponent {
  readonly salon = input.required<BusinessDto>();
}
