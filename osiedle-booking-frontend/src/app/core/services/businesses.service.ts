import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BusinessDetailsDto, PagedBusinessesResponse } from '../models/business.dto';

@Injectable({ providedIn: 'root' })
export class BusinessesService {
  private readonly http = inject(HttpClient);

  getBusinesses(page = 1, pageSize = 3): Observable<PagedBusinessesResponse> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);

    return this.http.get<PagedBusinessesResponse>('/api/Businesses', { params });
  }

  getBusinessById(id: string): Observable<BusinessDetailsDto> {
    return this.http.get<BusinessDetailsDto>(`/api/Businesses/${id}`);
  }
}
