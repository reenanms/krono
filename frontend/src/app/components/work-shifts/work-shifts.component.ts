import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { WorkShift } from '../../models';

@Component({
  selector: 'app-work-shifts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="card">
        <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Factory Operating Shifts & Calendar</h2>
        <p style="color: var(--text-muted); font-size: 0.875rem;">
          Configure working hours, lunch pauses, and business days. The scheduling engine uses this to accurately cascade stage completion times without overbooking non-working hours.
        </p>
      </div>

      @if (shift()) {
        <div class="card" style="max-width: 800px;">
          <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary-dark); margin-bottom: 1.25rem;">
            Shift Parameters: {{ shift()!.name }}
          </h3>

          <div class="grid grid-cols-2" style="margin-bottom: 1.25rem;">
            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Shift Label</label>
              <input type="text" [(ngModel)]="shift()!.name" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Active Working Days (comma-separated)</label>
              <input type="text" [(ngModel)]="shift()!.workDays" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Shift Start Time</label>
              <input type="text" [(ngModel)]="shift()!.startHour" placeholder="07:00" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Shift End Time</label>
              <input type="text" [(ngModel)]="shift()!.endHour" placeholder="17:00" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Scheduled Lunch / Break Start</label>
              <input type="text" [(ngModel)]="shift()!.breakStart" placeholder="12:00" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Scheduled Lunch / Break End</label>
              <input type="text" [(ngModel)]="shift()!.breakEnd" placeholder="13:00" class="input" />
            </div>
          </div>

          <div style="display: flex; gap: 1rem; align-items: center;">
            <button class="btn btn-success" (click)="saveShift()">💾 Save Operating Schedule</button>
            @if (saveMessage()) {
              <span style="color: var(--success); font-weight: 600; font-size: 0.9rem;">{{ saveMessage() }}</span>
            }
          </div>
        </div>
      }
    </div>
  `
})
export class WorkShiftsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly shift = signal<WorkShift | null>(null);
  readonly saveMessage = signal<string>('');

  ngOnInit(): void {
    this.api.getWorkShift().subscribe(s => this.shift.set(s));
  }

  saveShift(): void {
    if (!this.shift()) return;
    this.api.updateWorkShift(this.shift()!).subscribe(updated => {
      this.shift.set(updated);
      this.saveMessage.set('Operating schedule updated successfully!');
      setTimeout(() => this.saveMessage.set(''), 3000);
    });
  }
}
