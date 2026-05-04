import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { AntifonInsonorizantiComponent } from './antifon-insonorizanti.component';

describe('AntifonInsonorizantiComponent', () => {
  let component: AntifonInsonorizantiComponent;
  let fixture: ComponentFixture<AntifonInsonorizantiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AntifonInsonorizantiComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AntifonInsonorizantiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
