import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { IstoricComenziComponent } from './istoric-comenzi.component';

describe('IstoricComenziComponent', () => {
  let component: IstoricComenziComponent;
  let fixture: ComponentFixture<IstoricComenziComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IstoricComenziComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(IstoricComenziComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
