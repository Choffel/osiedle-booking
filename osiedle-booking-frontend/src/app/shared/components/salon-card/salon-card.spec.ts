import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SalonCard } from './salon-card';

describe('SalonCard', () => {
  let component: SalonCard;
  let fixture: ComponentFixture<SalonCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalonCard],
    }).compileComponents();

    fixture = TestBed.createComponent(SalonCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
