import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VopseaAutoPreparataDupaCodulDeCuloareComponent } from './vopsea-auto-preparata-dupa-codul-de-culoare.component';

describe('VopseaAutoPreparataDupaCodulDeCuloareComponent', () => {
  let component: VopseaAutoPreparataDupaCodulDeCuloareComponent;
  let fixture: ComponentFixture<VopseaAutoPreparataDupaCodulDeCuloareComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VopseaAutoPreparataDupaCodulDeCuloareComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VopseaAutoPreparataDupaCodulDeCuloareComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
