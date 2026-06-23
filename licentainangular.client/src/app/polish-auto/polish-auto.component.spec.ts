import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { PolishAutoComponent } from './polish-auto.component';

describe('PolishAutoComponent', () => {
  let component: PolishAutoComponent;
  let fixture: ComponentFixture<PolishAutoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PolishAutoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PolishAutoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
