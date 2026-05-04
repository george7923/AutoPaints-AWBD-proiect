import { ComponentFixture, TestBed } from '@angular/core/testing';
import '@types/jest';
import 'jest';
import { PoliticaConfidentialitateComponent } from './politica-confidentialitate.component';

describe('PoliticaConfidentialitateComponent', () => {
  let component: PoliticaConfidentialitateComponent;
  let fixture: ComponentFixture<PoliticaConfidentialitateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PoliticaConfidentialitateComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PoliticaConfidentialitateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
