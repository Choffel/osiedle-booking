import { Component, input } from '@angular/core';
import { Salon } from '../../../../shared/Interfaces/Services';

@Component({
  imports: [],
  selector: 'app-salon-header',
  styleUrl: './salon-header.scss',
  templateUrl: './salon-header.html',
})
export class SalonHeaderComponent {
  salon = input.required<Salon>();
}
