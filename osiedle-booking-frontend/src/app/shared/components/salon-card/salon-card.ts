import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import {Salon, Services} from '../../../shared/Interfaces/Services';

@Component({
  imports: [RouterLink],
  selector: 'app-salon-card',
  styleUrl: './salon-card.scss',
  templateUrl: './salon-card.html',
})
export class SalonCardComponent {
  salon = input.required<Salon>();
}
  