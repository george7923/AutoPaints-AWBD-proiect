import { ComponentFixture, TestBed } from '@angular/core/testing';
import 'jest';
import { VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent } from './vopsea-auto-preparata-dupa-codul-de-culoare-al-masinii.component';

describe('VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent', () => {
  let component: VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent;
  let fixture: ComponentFixture<VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(
      VopseaAutoPreparataDupaCodulDeCuloareAlMasiniiComponent
    );
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
