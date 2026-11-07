import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/pages/home-page/home-page';
import { SalonDetailsPageComponent } from './features/salon-details/pages/salon-details-page/salon-details-page';

export const routes: Routes = [
  { path: '', component: HomePageComponent }, // Главный адрес http://localhost:4200/
  { path: 'salons/:id', component: SalonDetailsPageComponent }, // Страница салона http://localhost:4200/salons/1
];