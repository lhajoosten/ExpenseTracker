import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError } from 'rxjs/operators';
import { HttpErrorService } from '../services/http-error.service';
import { LoggerService } from '../services/logger.service';

export const HttpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorHandler = inject(HttpErrorService);
  const logger = inject(LoggerService);

  return next(req).pipe(
    catchError(error => {
      // Log the error
      logger.error(`Request failed for ${req.url}`, error);

      // Get the operation name from the URL
      const urlParts = req.url.split('/');
      const operation = urlParts[urlParts.length - 1] || 'HTTP request';

      // Let the error handler service process the error
      return errorHandler.handleError(error, operation);
    })
  );
};
