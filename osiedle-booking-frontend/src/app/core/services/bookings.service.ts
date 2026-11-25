import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AvailableSlotDto, CreateBookingRequest } from '../models/booking.dto';

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

  createBooking(payload: CreateBookingRequest): Observable<unknown> {
    const params = new HttpParams()
      .set('BusinessId', payload.businessId)
      .set('StaffId', payload.staffId)
      .set('ServiceId', payload.serviceId)
      .set('StartTime', this.ensureUtcString(payload.startTime))
      .set('EndTime', this.ensureUtcString(payload.endTime));

    return this.http.post('/api/Bookings/booking', null, { params });
  }

  private ensureUtcString(isoDateTime: string): string {
    return /([zZ]|[+-]\d{2}:\d{2})$/.test(isoDateTime) ? isoDateTime : `${isoDateTime}Z`;
  }
}
