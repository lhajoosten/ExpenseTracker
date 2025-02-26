import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private darkTheme = new BehaviorSubject<boolean>(false);

  isDarkTheme = this.darkTheme.asObservable();

  constructor() {
    // Check for saved theme preference
    const savedTheme = localStorage.getItem('darkTheme');
    if (savedTheme) {
      this.darkTheme.next(savedTheme === 'true');
      this.setTheme(savedTheme === 'true');
    }
  }

  toggleDarkTheme(): void {
    const newValue = !this.darkTheme.value;
    this.darkTheme.next(newValue);
    this.setTheme(newValue);
    localStorage.setItem('darkTheme', String(newValue));
  }

  private setTheme(isDark: boolean): void {
    const body = document.body;
    if (isDark) {
      body.classList.add('dark-theme');
    } else {
      body.classList.remove('dark-theme');
    }
  }
}
