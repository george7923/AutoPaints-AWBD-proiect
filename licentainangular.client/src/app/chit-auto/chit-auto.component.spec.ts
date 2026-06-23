import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { ChitAutoComponent } from './chit-auto.component';

describe('ChitAutoComponent', () => {
  let component: ChitAutoComponent;
  let fixture: ComponentFixture<ChitAutoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChitAutoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ChitAutoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
