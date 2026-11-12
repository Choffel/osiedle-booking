import { Component, inject, computed, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { SalonHeaderComponent } from '../../components/salon-header/salon-header';
import { FooterComponent } from '../../../../shared/components/footer/footer';
import { MOCK_SALONS, Service } from '../../../../shared/Interfaces/Services';
import {ServiceList} from '../../components/service-list/service-list';
import { BookingSidebarComponent } from '../../components/booking-sidebar/booking-sidebar';


@Component({
  imports: [SalonHeaderComponent, FooterComponent, ServiceList, BookingSidebarComponent],
  selector: 'app-salon-details-page',
  styleUrl: './salon-details-page.scss',
  templateUrl: './salon-details-page.html',
})
export class SalonDetailsPageComponent {
  private route = inject(ActivatedRoute);

  // 1. Реактивно получаем id из параметров маршрута в виде сигнала
  salonId = toSignal(
    this.route.paramMap.pipe(map(params => Number(params.get('id'))))
  );

  // 2. Вычисляем салон автоматически при изменении salonId
  salon = computed(() => {
    const id = this.salonId();
    return MOCK_SALONS.find(s => s.id === id);
  });

  selectedService = signal<Service | null>(null);

 onServiceSelected(service: any) {
    console.log('Выбрана услуга:', service);
    // Здесь будет логика открытия бронирования
  }
  
}