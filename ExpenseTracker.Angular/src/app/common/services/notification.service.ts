import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationCountSource = new BehaviorSubject<number>(0);

  constructor() {
    // For demo purposes, simulate notifications
    setTimeout(() => {
      this.setNotificationCount(3);
    }, 2000);
  }

  getNotificationCount(): Observable<number> {
    return this.notificationCountSource.asObservable();
  }

  setNotificationCount(count: number): void {
    this.notificationCountSource.next(count);
  }

  clearNotifications(): void {
    this.notificationCountSource.next(0);
  }
}
