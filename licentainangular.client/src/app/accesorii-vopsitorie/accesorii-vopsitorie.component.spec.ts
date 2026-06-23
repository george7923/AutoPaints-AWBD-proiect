import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { AccesoriiVopsitorieComponent } from './accesorii-vopsitorie.component';

describe('AccesoriiVopsitorieComponent', () => {
  let component: AccesoriiVopsitorieComponent;
  let fixture: ComponentFixture<AccesoriiVopsitorieComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccesoriiVopsitorieComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AccesoriiVopsitorieComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
