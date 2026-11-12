import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BookingSidebar } from './booking-sidebar';

describe('BookingSidebar', () => {
  let component: BookingSidebar;
  let fixture: ComponentFixture<BookingSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookingSidebar],
    }).compileComponents();

    fixture = TestBed.createComponent(BookingSidebar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
