import { TestBed } from '@angular/core/testing';

import { PersoanaService } from './persoana.service';

describe('PersoanaService', () => {
  let service: PersoanaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PersoanaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
