import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { materialModules } from '../../shared.config';

@Component({
  selector: 'app-left-sidebar',
  standalone: true,
  imports: [RouterModule, materialModules],
  templateUrl: './left-sidebar.component.html',
  styleUrls: ['./left-sidebar.component.scss']
})
export class LeftSidebarComponent { }
