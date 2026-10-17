import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { OperatorSkill } from '../../models';

@Component({
  selector: 'app-operator-skills',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
        <div>
          <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Operator Roles & Specialized Skills</h2>
          <p style="color: var(--text-muted); font-size: 0.875rem;">
            Define job roles for labor dimensioning. The system tracks peak concurrencies and man-hours per role.
          </p>
        </div>
      </div>

      <!-- Add Skill Form -->
      <div class="card" style="background-color: #f8fafc;">
        <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary); margin-bottom: 1rem;">
          Register New Specialty / Skill
        </h3>

        <div class="grid grid-cols-2" style="margin-bottom: 1rem;">
          <div>
            <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Role / Skill Name</label>
            <input type="text" [(ngModel)]="newSkill.name" placeholder="e.g. Master Baker" class="input" />
          </div>
          <div>
            <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Role Description</label>
            <input type="text" [(ngModel)]="newSkill.description" placeholder="e.g. Yeast fermentation and dough hydration" class="input" />
          </div>
        </div>

        <button class="btn btn-primary" (click)="saveSkill()">➕ Add Operator Skill</button>
      </div>

      <!-- Skills Table -->
      <div class="card">
        <h3 style="font-size: 1.15rem; font-weight: 700; margin-bottom: 1rem;">Active Factory Roles ({{ skills().length }} defined)</h3>
        <div class="table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Skill / Role Name</th>
                <th>Description</th>
                <th style="text-align: right;">Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (s of skills(); track s.id) {
                <tr>
                  <td><strong>{{ s.name }}</strong></td>
                  <td style="color: var(--text-muted);">{{ s.description }}</td>
                  <td style="text-align: right;">
                    <button class="btn btn-danger" style="padding: 0.3rem 0.6rem; font-size: 0.8rem;" (click)="deleteSkill(s.id)">
                      🗑️ Remove
                    </button>
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="3" style="text-align: center; color: var(--text-muted); padding: 2rem;">No operator skills defined.</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `
})
export class OperatorSkillsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly skills = signal<OperatorSkill[]>([]);

  newSkill = {
    name: '',
    description: ''
  };

  ngOnInit(): void {
    this.loadSkills();
  }

  loadSkills(): void {
    this.api.getSkills().subscribe(s => this.skills.set(s));
  }

  saveSkill(): void {
    if (!this.newSkill.name) return;
    this.api.createSkill(this.newSkill).subscribe(() => {
      this.newSkill = { name: '', description: '' };
      this.loadSkills();
    });
  }

  deleteSkill(id: string): void {
    if (confirm('Delete this operator skill?')) {
      this.api.deleteSkill(id).subscribe(() => this.loadSkills());
    }
  }
}
