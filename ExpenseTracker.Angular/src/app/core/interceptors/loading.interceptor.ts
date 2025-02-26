import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { finalize } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';

// Create a loading service
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();
  private loadingCount = 0;

  setLoading(loading: boolean): void {
    if (loading) {
      this.loadingCount++;
      this.loadingSubject.next(true);
    } else {
      this.loadingCount--;
      if (this.loadingCount <= 0) {
        this.loadingCount = 0;
        this.loadingSubject.next(false);
      }
    }
  }
}

// Provide the loading service
export const loadingServiceProvider = {
  provide: LoadingService,
  useClass: LoadingService
};

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const loadingService = inject(LoadingService);

  // Don't show loading for specific endpoints if needed
  if (req.url.includes('/auth/status')) {
    return next(req);
  }

  loadingService.setLoading(true);

  return next(req).pipe(
    finalize(() => {
      loadingService.setLoading(false);
    })
  );
};
