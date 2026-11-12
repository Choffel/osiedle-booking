import { Component } from '@angular/core';
import { HeroSearchComponent } from '../../components/hero-search/hero-search';
import { CategoryChipsComponent } from '../../components/category-chips/category-chips';
import {SalonCardComponent} from '../../../../shared/components/salon-card/salon-card';
import {FeaturesListComponent} from '../../../../features/home/components/features-list/features-list';
import {FooterComponent} from '../../../../shared/components/footer/footer';
import {Services} from '../../../../shared/Interfaces/Services';

@Component({
  imports: [HeroSearchComponent, CategoryChipsComponent, SalonCardComponent, FeaturesListComponent, FooterComponent],
  selector: 'app-home-page',
  styleUrl: './home-page.scss',
  templateUrl: './home-page.html',
})
export class HomePageComponent {
  salons =  new Services().salons;
}
