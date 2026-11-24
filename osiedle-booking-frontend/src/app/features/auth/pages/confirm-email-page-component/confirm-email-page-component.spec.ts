import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmEmailPageComponent } from './confirm-email-page-component';

describe('ConfirmEmailPageComponent', () => {
  let component: ConfirmEmailPageComponent;
  let fixture: ComponentFixture<ConfirmEmailPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ConfirmEmailPageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ConfirmEmailPageComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
