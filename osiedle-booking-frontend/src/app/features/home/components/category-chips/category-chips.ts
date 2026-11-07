import { Component } from '@angular/core';

@Component({
  selector: 'app-category-chips',
  standalone: true,
  imports: [],
  templateUrl: './category-chips.html',
  styleUrl: './category-chips.scss',
})
export class CategoryChipsComponent {
  categories: string[] = [
    'Wszystkie',
    'Strzyżenie męskie',
    'Trymowanie brody',
    'Combo (włosy + broda)',
    'Golenie brzytwą',
    'Zabiegi'
  ];
}