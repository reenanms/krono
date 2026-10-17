import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface ScreenItem {
  name: string;
  description: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-screen-hub',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="card">
      <div style="margin-bottom: 1.5rem;">
        <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">System Navigation Hub</h2>
        <p style="color: var(--text-muted); font-size: 0.9rem;">
          Quickly search and jump to any module using instant in-memory filtering.
        </p>
      </div>

      <div class="search-bar" style="margin-bottom: 2rem;">
        <input 
          type="text" 
          placeholder="Search screen by name or description..."
          [ngModel]="searchTerm()"
          (ngModelChange)="searchTerm.set($event)"
          class="input"
          style="font-size: 1rem; padding: 0.8rem 1rem;"
        />
      </div>

      <div class="grid grid-cols-3">
        @for (screen of filteredScreens(); track screen.route) {
          <div class="card" style="margin-bottom: 0; display: flex; flex-direction: column; justify-content: space-between; border-top: 4px solid var(--primary-light);">
            <div>
              <div style="font-size: 2rem; margin-bottom: 0.5rem;">{{ screen.icon }}</div>
              <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--text-main); margin-bottom: 0.5rem;">
                {{ screen.name }}
              </h3>
              <p style="color: var(--text-muted); font-size: 0.875rem;">
                {{ screen.description }}
              </p>
            </div>
            <a [routerLink]="screen.route" class="btn btn-primary" style="margin-top: 1.25rem;">
              Open Module →
            </a>
          </div>
        } @empty {
          <div style="grid-column: 1 / -1; text-align: center; padding: 3rem; color: var(--text-muted);">
            <p style="font-size: 1.1rem;">No screens found matching "{{ searchTerm() }}".</p>
          </div>
        }
      </div>
    </div>
  `
})
export class ScreenHubComponent {
  readonly searchTerm = signal('');

  readonly screens = signal<ScreenItem[]>([
    {
      name: 'Planning Calendar',
      description: 'Agenda and timeline view for production scheduling, order generation, stage adjustments, and conflict alerts.',
      route: '/planning',
      icon: '📅'
    },
    {
      name: 'Schedule Report',
      description: 'Consolidated daily and weekly schedules, labor sizing summary table, print-ready layout, and CSV exporter.',
      route: '/reports',
      icon: '📊'
    },
    {
      name: 'Products & Stage Routing',
      description: 'Manage recipes, multi-stage DAG routings, setup/cycle duration formulas, and predecessor dependencies.',
      route: '/products',
      icon: '🧁'
    },
    {
      name: 'Machine Types',
      description: 'Control physical fleet availability for planetary mixers, ovens, depositors, and workstations.',
      route: '/machines',
      icon: '⚙️'
    },
    {
      name: 'Operator Skills',
      description: 'Configure specialized factory roles (Dough Mixer, Depositor Operator, Oven Tender, Packager).',
      route: '/skills',
      icon: '👥'
    },
    {
      name: 'Factory Operating Shifts',
      description: 'Configure operating hours, lunch breaks, and active business days across the weekly calendar.',
      route: '/shifts',
      icon: '⏰'
    }
  ]);

  readonly filteredScreens = computed(() => {
    const query = this.searchTerm().toLowerCase().trim();
    if (!query) return this.screens();
    return this.screens().filter(s =>
      s.name.toLowerCase().includes(query) ||
      s.description.toLowerCase().includes(query)
    );
  });
}
