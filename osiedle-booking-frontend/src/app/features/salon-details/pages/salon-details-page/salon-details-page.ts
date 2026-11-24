import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, switchMap } from 'rxjs';
import { BusinessesService } from '../../../../core/services/businesses.service';
import { BusinessEmployeeDto, BusinessServiceItemDto } from '../../../../core/models/business.dto';
import { SalonHeaderComponent } from '../../components/salon-header/salon-header';
import { ServiceListComponent } from '../../components/service-list/service-list';
import { BookingSidebarComponent } from '../../components/booking-sidebar/booking-sidebar';

@Component({
  selector: 'app-salon-details-page',
  standalone: true,
  imports: [SalonHeaderComponent, ServiceListComponent, BookingSidebarComponent],
  templateUrl: './salon-details-page.html',
  styleUrl: './salon-details-page.scss',
})
export class SalonDetailsPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly businessesService = inject(BusinessesService);

  readonly coverPhoto =
    'https://d375139ucebi94.cloudfront.net/region2/pl/150097/biz_photo/9ce3b7f3d76445d4a52769cba5d256-tommy-barbershop-biz-photo-6582403c3ded446184d893e5a2777a-booksy.jpeg?size=640x427';

  readonly business = toSignal(
    this.route.paramMap.pipe(
      map((params) => params.get('id')),
      filter((id): id is string => !!id),
      switchMap((id) => this.businessesService.getBusinessById(id)),
    ),
    { initialValue: null },
  );

  // Задел под модальное окно бронирования: когда не null — открываем попап
  readonly bookingModalService = signal<BusinessServiceItemDto | null>(null);

  onOpenBookingModal(service: BusinessServiceItemDto): void {
    this.bookingModalService.set(service);
    console.log('Открываем попап бронирования для услуги:', service);
  }

  getInitials(emp: BusinessEmployeeDto): string {
    return `${emp.firstName[0] ?? ''}${emp.lastName[0] ?? ''}`.toUpperCase();
  }

  isValidUrl(url: string): boolean {
    return url.startsWith('http://') || url.startsWith('https://');
  }
}
