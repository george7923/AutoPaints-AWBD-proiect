import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { TermeniConditiiComponent } from './termeni-conditii.component';

describe('TermeniConditiiComponent', () => {
  let component: TermeniConditiiComponent;
  let fixture: ComponentFixture<TermeniConditiiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TermeniConditiiComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TermeniConditiiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
