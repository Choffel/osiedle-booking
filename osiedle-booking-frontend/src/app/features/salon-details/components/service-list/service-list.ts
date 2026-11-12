import { Component, input, output, Service } from '@angular/core';
import { Salon } from '../../../../shared/Interfaces/Services';

@Component({
  imports: [],
  selector: 'app-service-list',
  styleUrl: './service-list.scss',
  templateUrl: './service-list.html',
})
export class ServiceList {
  salon = input.required<Salon>();
  selectService = output<Salon>();
}
