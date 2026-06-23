import { TestBed } from '@angular/core/testing';

import { SubprodusService } from './subprodus.service';

describe('SubprodusService', () => {
  let service: SubprodusService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SubprodusService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
