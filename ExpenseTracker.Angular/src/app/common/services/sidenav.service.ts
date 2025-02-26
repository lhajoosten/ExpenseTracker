import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidenavService {
  private sidenavToggleSource = new Subject<void>();

  sidenavToggle$ = this.sidenavToggleSource.asObservable();

  toggleSidenav(): void {
    this.sidenavToggleSource.next();
  }
}
