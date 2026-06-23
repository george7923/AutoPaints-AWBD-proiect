import { ComponentFixture, TestBed } from '@angular/core/testing';
import 'jest';
import { VopseaAutoReadyMixComponent } from './vopsea-auto-ready-mix.component';

describe('VopseaAutoReadyMixComponent', () => {
  let component: VopseaAutoReadyMixComponent;
  let fixture: ComponentFixture<VopseaAutoReadyMixComponent>;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VopseaAutoReadyMixComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(VopseaAutoReadyMixComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });
  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
