import { Component, input } from '@angular/core';
import { Salon } from '../../../features/home/pages/home-page/home-page';

@Component({
  imports: [],
  selector: 'app-salon-card',
  styleUrl: './salon-card.scss',
  templateUrl: './salon-card.html',
})
export class SalonCardComponent {
  salon = input.required<Salon>();
}
