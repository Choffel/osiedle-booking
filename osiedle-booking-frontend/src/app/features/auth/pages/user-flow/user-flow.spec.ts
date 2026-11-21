import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserFlow } from './user-flow';

describe('UserFlow', () => {
  let component: UserFlow;
  let fixture: ComponentFixture<UserFlow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserFlow],
    }).compileComponents();

    fixture = TestBed.createComponent(UserFlow);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
