import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { InformatiiComponent } from './informatii.component';

describe('InformatiiComponent', () => {
  let component: InformatiiComponent;
  let fixture: ComponentFixture<InformatiiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InformatiiComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InformatiiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
