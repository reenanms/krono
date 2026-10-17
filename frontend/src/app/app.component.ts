import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header class="header-banner no-print">
      <div class="brand-title">
        <span>⏱️ Krono</span>
        <span class="brand-badge">PCP / APS</span>
      </div>

      <nav class="nav-links">
        <a routerLink="/hub" routerLinkActive="active">Navigation Hub</a>
        <a routerLink="/planning" routerLinkActive="active">Planning Calendar</a>
        <a routerLink="/reports" routerLinkActive="active">Schedule Report</a>
        <a routerLink="/products" routerLinkActive="active">Products</a>
        <a routerLink="/machines" routerLinkActive="active">Machines</a>
        <a routerLink="/skills" routerLinkActive="active">Skills</a>
        <a routerLink="/shifts" routerLinkActive="active">Factory Shifts</a>
      </nav>
    </header>

    <main class="container">
      <router-outlet></router-outlet>
    </main>
  `
})
export class AppComponent {}
