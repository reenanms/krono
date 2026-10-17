import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { MachineType } from '../../models';

@Component({
  selector: 'app-machine-types',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
        <div>
          <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Machine & Equipment Fleet</h2>
          <p style="color: var(--text-muted); font-size: 0.875rem;">
            Control the physical fleet inventory. The system detects over-allocations and alerts planners to bottlenecks.
          </p>
        </div>
      </div>

      <!-- Add Machine Form -->
      <div class="card" style="background-color: #f8fafc;">
        <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary); margin-bottom: 1rem;">
          Register Equipment / Workstation
        </h3>

        <div class="grid grid-cols-3" style="margin-bottom: 1rem;">
          <div>
            <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Machine Name</label>
            <input type="text" [(ngModel)]="newMachine.name" placeholder="e.g. Tunnel Oven 03" class="input" />
          </div>
          <div>
            <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Description</label>
            <input type="text" [(ngModel)]="newMachine.description" placeholder="e.g. Continuous gas-fired oven" class="input" />
          </div>
          <div>
            <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Total Available Fleet Count</label>
            <input type="number" [(ngModel)]="newMachine.totalAvailable" min="1" class="input" />
          </div>
        </div>

        <button class="btn btn-primary" (click)="saveMachine()">➕ Add Machine Type</button>
      </div>

      <!-- Machines Table -->
      <div class="card">
        <h3 style="font-size: 1.15rem; font-weight: 700; margin-bottom: 1rem;">Available Factory Machinery ({{ machines().length }} registered)</h3>
        <div class="table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Machine Name</th>
                <th>Description</th>
                <th style="text-align: center;">Total Units Available</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (m of machines(); track m.id) {
                <tr>
                  <td><strong>{{ m.name }}</strong></td>
                  <td style="color: var(--text-muted);">{{ m.description }}</td>
                  <td style="text-align: center;">
                    <span class="badge badge-fixed" style="font-size: 0.85rem;">
                      {{ m.totalAvailable }} units
                    </span>
                  </td>
                  <td style="text-align: right;">
                    <button class="btn btn-danger" style="padding: 0.3rem 0.6rem; font-size: 0.8rem;" (click)="deleteMachine(m.id)">
                      🗑️ Remove
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="4" style="text-align: center; color: var(--text-muted); padding: 2rem;">No machines registered.</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class MachineTypesComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly machines = signal<MachineType[]>([]);

  newMachine = {
    name: '',
    description: '',
    totalAvailable: 1
  };

  ngOnInit(): void {
    this.loadMachines();
  }

  loadMachines(): void {
    this.api.getMachines().subscribe(ms => this.machines.set(ms));
  }

  saveMachine(): void {
    if (!this.newMachine.name) return;
    this.api.createMachine(this.newMachine).subscribe(() => {
      this.newMachine = { name: '', description: '', totalAvailable: 1 };
      this.loadMachines();
    });
  }

  deleteMachine(id: string): void {
    if (confirm('Delete this machine type?')) {
      this.api.deleteMachine(id).subscribe(() => this.loadMachines());
    }
  }
}
