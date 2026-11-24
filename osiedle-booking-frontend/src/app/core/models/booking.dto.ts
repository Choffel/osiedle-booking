export interface AvailableSlotDto {
  readonly start: string;
  readonly end: string;
}

export interface CreateBookingRequest {
  readonly businessId: string;
  readonly staffId: string;
  readonly serviceId: string;
  readonly startTime: string;
  readonly endTime: string;
}
