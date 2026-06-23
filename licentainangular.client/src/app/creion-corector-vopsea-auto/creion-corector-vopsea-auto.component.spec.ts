import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { CreionCorectorVopseaAutoComponent } from './creion-corector-vopsea-auto.component';

describe('CreionCorectorVopseaAutoComponent', () => {
  let component: CreionCorectorVopseaAutoComponent;
  let fixture: ComponentFixture<CreionCorectorVopseaAutoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreionCorectorVopseaAutoComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CreionCorectorVopseaAutoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
