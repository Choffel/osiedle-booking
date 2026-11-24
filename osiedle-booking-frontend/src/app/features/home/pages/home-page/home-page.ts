import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { BusinessesService } from '../../../../core/services/businesses.service';
import { HeroSearchComponent } from '../../components/hero-search/hero-search';
import { CategoryChipsComponent } from '../../components/category-chips/category-chips';
import { FeaturesListComponent } from '../../components/features-list/features-list';
import { SalonCardComponent } from '../../../../shared/components/salon-card/salon-card';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [HeroSearchComponent, CategoryChipsComponent, SalonCardComponent, FeaturesListComponent],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss',
})
export class HomePageComponent {
  private readonly businessesService = inject(BusinessesService);

  readonly salons = toSignal(
    this.businessesService.getBusinesses(1, 3).pipe(map((res) => res.items)),
    { initialValue: [] },
  );
}
