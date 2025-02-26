import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AngularMaterialModule } from './common/components/angular-material.module';
import { SidebarComponent } from './common/components/sidebar/sidebar.component';
import { HeaderComponent } from './common/components/header/header.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [RouterOutlet, AngularMaterialModule, SidebarComponent, HeaderComponent],
  standalone: true
})
export class AppComponent {
  sidebarCollapsed = false;
  title = 'expense-tracker';

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }
}
