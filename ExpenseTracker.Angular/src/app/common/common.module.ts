import { NgModule, Optional, SkipSelf } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthService } from './services/auth.service';
import { ApiService } from './services/api.service';
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  imports: [
    HttpClientModule
  ],
  providers: [
    AuthService,
    ApiService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ]
})
export class CommonModule {
  constructor(@Optional() @SkipSelf() parentModule: CommonModule) {
    if (parentModule) {
      throw new Error('CommonModule is already loaded. Import it in the AppModule only');
    }
  }
}
