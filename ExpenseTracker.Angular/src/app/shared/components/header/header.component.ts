import { Component, OnInit } from '@angular/core';
import { materialModules } from '../../shared.config';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  imports: [materialModules],
  standalone: true
})
export class HeaderComponent implements OnInit {
  title: string = 'Dashboard';

  constructor() { }

  ngOnInit(): void {
    // You could fetch the current user info or other dynamic content here
  }

  // You could add methods for search functionality here
  onSearch(searchTerm: string): void {
    console.log(`Searching for: ${searchTerm}`);
    // Implement search functionality
  }
}