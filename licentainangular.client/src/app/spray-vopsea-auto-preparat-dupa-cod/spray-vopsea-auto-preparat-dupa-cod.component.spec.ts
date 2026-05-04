import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { SprayVopseaAutoPreparatDupaCodComponent } from './spray-vopsea-auto-preparat-dupa-cod.component';

describe('SprayVopseaAutoPreparatDupaCodComponent', () => {
  let component: SprayVopseaAutoPreparatDupaCodComponent;
  let fixture: ComponentFixture<SprayVopseaAutoPreparatDupaCodComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SprayVopseaAutoPreparatDupaCodComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(SprayVopseaAutoPreparatDupaCodComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
