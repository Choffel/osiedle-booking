import { Component } from '@angular/core';
import { HeroSearchComponent } from '../../components/hero-search/hero-search';
import { CategoryChipsComponent } from '../../components/category-chips/category-chips';
import {SalonCardComponent} from '../../../../shared/components/salon-card/salon-card';
import {FeaturesListComponent} from '../../../../features/home/components/features-list/features-list';
import {FooterComponent} from '../../../../shared/components/footer/footer';

@Component({
  imports: [HeroSearchComponent, CategoryChipsComponent, SalonCardComponent, FeaturesListComponent, FooterComponent],
  selector: 'app-home-page',
  styleUrl: './home-page.scss',
  templateUrl: './home-page.html',
})
export class HomePageComponent {
  salons = MOCK_SALONS;
}

export interface Salon {
  id: number;
  photo: string;
  name: string;
  address: string;
  rating: number;
  services: Service[];
  timeSlots: TimeSlot[];
}

export interface TimeSlot {
  time: string;
  staffMember: string;
}

export interface Service {
  name: string;
  price: number;
}

export const MOCK_SALONS: Salon[] = [
  {
    id: 1,
    photo: 'https://d375139ucebi94.cloudfront.net/region2/pl/150097/biz_photo/9ce3b7f3d76445d4a52769cba5d256-tommy-barbershop-biz-photo-6582403c3ded446184d893e5a2777a-booksy.jpeg?size=640x427', // подставь URL картинки
    name: 'Kosa & Brzytwa Barber Studio',
    address: 'Śródmieście, ul. Marszałkowska 84 • 0.8 km',
    rating: 4.9,
    services: [
      { name: 'Strzyżenie męskie klasyczne', price: 90 },
      { name: 'Combo (włosy + pielęgnacja brody)', price: 140 }
    ],
    timeSlots: [
      { time: '14:30', staffMember: 'Marek' },
      { time: '16:00', staffMember: 'Tomek' },
      { time: '17:15', staffMember: 'Sasha' }
    ]
  },
  {
    id: 2,
    photo: 'https://d375139ucebi94.cloudfront.net/region2/pl/150097/biz_photo/9ce3b7f3d76445d4a52769cba5d256-tommy-barbershop-biz-photo-6582403c3ded446184d893e5a2777a-booksy.jpeg?size=640x427',
    name: 'Warsaw Gentlemen Club',
    address: 'Mokotów, ul. Puławska 31 • 1.9 km',
    rating: 5.0,
    services: [
      { name: 'Strzyżenie Skin Fade', price: 100 },
      { name: 'Modelowanie brody brzytwą', price: 80 }
    ],
    timeSlots: [
      { time: '15:00', staffMember: 'Kamil' },
      { time: '16:30', staffMember: 'Adam' },
      { time: '18:45', staffMember: 'Kamil' }
    ]
  },
  {
    id: 3,
    photo: 'https://d375139ucebi94.cloudfront.net/region2/pl/150097/biz_photo/9ce3b7f3d76445d4a52769cba5d256-tommy-barbershop-biz-photo-6582403c3ded446184d893e5a2777a-booksy.jpeg?size=640x427',
    name: 'Pracownia Fryzur & Beard Atelier',
    address: 'Wola, ul. Grzybowska 61 • 2.4 km',
    rating: 4.8,
    services: [
      { name: 'Strzyżenie nożyczkami + mycie', price: 85 },
      { name: 'Full Service Combo', price: 135 }
    ],
    timeSlots: [
      { time: '14:15', staffMember: 'Piotr' },
      { time: '15:45', staffMember: 'Jan' },
      { time: '18:00', staffMember: 'Jan' }
    ]
  }
];