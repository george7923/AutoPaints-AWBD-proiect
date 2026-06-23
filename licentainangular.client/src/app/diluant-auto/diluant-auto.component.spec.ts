import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { DiluantAutoComponent } from './diluant-auto.component';

describe('DiluantAutoComponent', () => {
  let component: DiluantAutoComponent;
  let fixture: ComponentFixture<DiluantAutoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DiluantAutoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DiluantAutoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
