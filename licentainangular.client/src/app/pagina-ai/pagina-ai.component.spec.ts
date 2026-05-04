import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaginaAiComponent } from './pagina-ai.component';

describe('PaginaAiComponent', () => {
  let component: PaginaAiComponent;
  let fixture: ComponentFixture<PaginaAiComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PaginaAiComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PaginaAiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
