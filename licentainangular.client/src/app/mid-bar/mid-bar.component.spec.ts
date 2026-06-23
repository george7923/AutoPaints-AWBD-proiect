import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MidBarComponent } from './mid-bar.component';

describe('MidBarComponent', () => {
  let component: MidBarComponent;
  let fixture: ComponentFixture<MidBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MidBarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MidBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
