import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ZgarieturiAIComponent } from './zgarieturi-ai.component';

describe('ZgarieturiAIComponent', () => {
  let component: ZgarieturiAIComponent;
  let fixture: ComponentFixture<ZgarieturiAIComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ZgarieturiAIComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ZgarieturiAIComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
