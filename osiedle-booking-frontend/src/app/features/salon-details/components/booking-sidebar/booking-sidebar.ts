import { Component, input } from '@angular/core';
import { Service } from '../../../../shared/Interfaces/Services'; // Путь к твоему файлу моделей

@Component({
  imports: [],
  selector: 'app-booking-sidebar',
  styleUrl: './booking-sidebar.scss',
  templateUrl: './booking-sidebar.html',
})
export class BookingSidebarComponent { // Не забудь поправить имя класса на BookingSidebarComponent
  selectedService = input<Service | null>(null);
}