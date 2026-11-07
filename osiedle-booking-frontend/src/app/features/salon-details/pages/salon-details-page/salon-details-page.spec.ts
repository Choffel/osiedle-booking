import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SalonDetailsPage } from './salon-details-page';

describe('SalonDetailsPage', () => {
  let component: SalonDetailsPage;
  let fixture: ComponentFixture<SalonDetailsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalonDetailsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(SalonDetailsPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
