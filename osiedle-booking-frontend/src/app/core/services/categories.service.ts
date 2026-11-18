import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CategoryDto } from '../models/category.dto';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private readonly http = inject(HttpClient);
  getCategories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/Category');
  }
}
