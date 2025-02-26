import { TestBed } from '@angular/core/testing';

import { ReportsSkipTestsService } from './reports--skip-tests.service';

describe('ReportsSkipTestsService', () => {
  let service: ReportsSkipTestsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ReportsSkipTestsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
