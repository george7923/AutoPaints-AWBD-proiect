import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { LacAutoComponent } from './lac-auto.component';

describe('LacAutoComponent', () => {
  let component: LacAutoComponent;
  let fixture: ComponentFixture<LacAutoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LacAutoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LacAutoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
