import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VopseaAIComponent } from './vopsea-ai.component';

describe('VopseaAIComponent', () => {
  let component: VopseaAIComponent;
  let fixture: ComponentFixture<VopseaAIComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VopseaAIComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VopseaAIComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
