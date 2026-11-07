import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SalonHeader } from './salon-header';

describe('SalonHeader', () => {
  let component: SalonHeader;
  let fixture: ComponentFixture<SalonHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalonHeader],
    }).compileComponents();

    fixture = TestBed.createComponent(SalonHeader);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
