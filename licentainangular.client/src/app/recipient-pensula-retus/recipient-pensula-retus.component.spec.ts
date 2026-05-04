import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { RecipientPensulaRetusComponent } from './recipient-pensula-retus.component';

describe('RecipientPensulaRetusComponent', () => {
  let component: RecipientPensulaRetusComponent;
  let fixture: ComponentFixture<RecipientPensulaRetusComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecipientPensulaRetusComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(RecipientPensulaRetusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
