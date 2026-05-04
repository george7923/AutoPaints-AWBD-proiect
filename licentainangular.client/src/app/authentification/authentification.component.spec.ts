import { ComponentFixture, TestBed } from '@angular/core/testing';
import 'jest';
import { AuthentificationComponent } from './authentification.component';

describe('LoginComponent', () => {
  let component: AuthentificationComponent;
  let fixture: ComponentFixture<AuthentificationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AuthentificationComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(AuthentificationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
