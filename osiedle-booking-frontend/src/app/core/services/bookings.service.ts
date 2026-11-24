import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AvailableSlotDto } from '../models/booking.dto';

@Injectable({ providedIn: 'root' })
export class BookingsService {
  private readonly http = inject(HttpClient);

  getAvailableSlots(
    staffId: string,
    serviceId: string,
    date: string,
  ): Observable<readonly AvailableSlotDto[]> {
    const params = new HttpParams()
      .set('StaffId', staffId)
      .set('ServiceId', serviceId)
      .set('Date', date);

    return this.http.get<readonly AvailableSlotDto[]>('/api/Bookings/available-slots', { params });
  }
}
