import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FavoriteSalonView } from '../../models/profile.models';

@Component({
  selector: 'app-favorite-salons-widget',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './favorite-salons-widget.html',
  styleUrl: './favorite-salons-widget.scss',
})
export class FavoriteSalonsWidgetComponent {
  readonly salons = input.required<readonly FavoriteSalonView[]>();
}
