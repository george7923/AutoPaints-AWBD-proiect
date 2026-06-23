import { TestBed } from '@angular/core/testing';

import { SubcomandaService } from './subcomanda.service';

describe('SubcomandaService', () => {
  let service: SubcomandaService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SubcomandaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
