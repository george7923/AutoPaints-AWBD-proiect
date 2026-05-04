import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { SprayVopseaAutoDiverseAplicatiiComponent } from './spray-vopsea-auto-diverse-aplicatii.component';

describe('SprayVopseaAutoDiverseAplicatiiComponent', () => {
  let component: SprayVopseaAutoDiverseAplicatiiComponent;
  let fixture: ComponentFixture<SprayVopseaAutoDiverseAplicatiiComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SprayVopseaAutoDiverseAplicatiiComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(SprayVopseaAutoDiverseAplicatiiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
