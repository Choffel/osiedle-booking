import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {CategoriesService} from '../../../../core/services/categories.service'


@Component({
  selector: 'app-category-chips',
  standalone: true,
  imports: [],
  templateUrl: './category-chips.html',
  styleUrl: './category-chips.scss',
})
export class CategoryChipsComponent {
  private readonly categoriesService = inject(CategoriesService);
  readonly categories = toSignal(this.categoriesService.getCategories(), { initialValue: [] });
}
