import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { ProductionScheduleReport, ScheduledStage } from '../../models';

@Component({
  selector: 'app-schedule-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <!-- Report Controls (Hidden on Print) -->
      <div class="card no-print" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem;">
        <div>
          <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Consolidated Production Schedule Report</h2>
          <p style="color: var(--text-muted); font-size: 0.875rem;">
            Resource sizing breakdown, machine timeline, and shop-floor executive view.
          </p>
        </div>

        <div style="display: flex; gap: 0.75rem; align-items: center; flex-wrap: wrap;">
          <!-- Horizon Toggle -->
          <div style="display: flex; border: 1px solid var(--border-color); border-radius: 0.5rem; overflow: hidden;">
            <button 
              class="btn" 
              style="border-radius: 0; padding: 0.5rem 1rem;" 
              [ngClass]="viewType() === 'day' ? 'btn-primary' : 'btn-secondary'"
              (click)="setViewType('day')">
              Daily View
            </button>
            <button 
              class="btn" 
              style="border-radius: 0; padding: 0.5rem 1rem;" 
              [ngClass]="viewType() === 'week' ? 'btn-primary' : 'btn-secondary'"
              (click)="setViewType('week')">
              Weekly View
            </button>
          </div>

          <input 
            type="date" 
            [ngModel]="selectedDate()"
            (ngModelChange)="onDateChange($event)"
            class="input"
            style="width: auto;"
          />

          <!-- Perspective Toggle -->
          <div style="display: flex; border: 1px solid var(--border-color); border-radius: 0.5rem; overflow: hidden;">
            <button 
              class="btn" 
              style="border-radius: 0; padding: 0.5rem 0.75rem; font-size: 0.8rem;" 
              [ngClass]="perspective() === 'machine' ? 'btn-primary' : 'btn-secondary'"
              (click)="perspective.set('machine')">
              By Machine
            </button>
            <button 
              class="btn" 
              style="border-radius: 0; padding: 0.5rem 0.75rem; font-size: 0.8rem;" 
              [ngClass]="perspective() === 'order' ? 'btn-primary' : 'btn-secondary'"
              (click)="perspective.set('order')">
              By Order
            </button>
          </div>

          <button class="btn btn-secondary" (click)="printReport()">🖨️ Print (A4 / PDF)</button>
          <a [href]="exportUrl()" class="btn btn-primary" download>📥 Export CSV</a>
        </div>
      </div>

      <!-- Printable Document Header (Shown in Print and Screen) -->
      <div class="card" style="border-left: 5px solid var(--primary);">
        <div style="display: flex; justify-content: space-between; align-items: flex-start;">
          <div>
            <h1 style="font-size: 1.5rem; font-weight: 800; color: var(--primary-dark); text-transform: uppercase;">
              KRONO PRODUCTION SCHEDULE
            </h1>
            <p style="font-size: 0.95rem; color: var(--text-main); margin-top: 0.25rem;">
              Horizon: <strong>{{ report()?.startDate | date:'fullDate' }}</strong> to <strong>{{ report()?.endDate | date:'fullDate' }}</strong>
            </p>
          </div>
          <div style="text-align: right;">
            <span class="badge badge-fixed" style="font-size: 0.85rem; padding: 0.4rem 0.75rem;">
              View: {{ viewType() | uppercase }}
            </span>
          </div>
        </div>
      </div>

      <!-- 1. Labor Sizing Summary Table (Focus Central) -->
      <div class="card">
        <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary); margin-bottom: 0.75rem; display: flex; align-items: center; gap: 0.5rem;">
          <span>👥</span> Labor Sizing & Capacity Dimensioning Summary
        </h3>
        <p style="color: var(--text-muted); font-size: 0.85rem; margin-bottom: 1rem;">
          Calculated headcount peaks and accumulated man-hours necessary to achieve the planned production volume.
        </p>

        <div class="table-container">
          <table class="table">
            <thead>
              <tr>
                <th>Operator Specialty / Skill</th>
                <th style="text-align: center;">Peak Concurrent Workers Required</th>
                <th style="text-align: right;">Total Labor Demand (Man-Hours)</th>
              </tr>
            </thead>
            <tbody>
              @for (labor of report()?.laborSummary; track labor.skillId) {
                <tr>
                  <td><strong>{{ labor.skillName }}</strong></td>
                  <td style="text-align: center;">
                    <span class="badge" [ngClass]="labor.peakWorkers > 0 ? 'badge-fixed' : 'badge-passive'">
                      {{ labor.peakWorkers }} workers peak
                    </span>
                  </td>
                  <td style="text-align: right; font-weight: 600;">
                    {{ labor.totalManHours }} hrs
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="3" style="text-align: center; color: var(--text-muted); padding: 1.5rem;">
                    No labor demand recorded for this horizon.
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- 2. Detailed Production Operations -->
      <div class="card">
        <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary); margin-bottom: 0.75rem; display: flex; align-items: center; gap: 0.5rem;">
          <span>📋</span> Detailed Operations Schedule ({{ perspective() === 'machine' ? 'Grouped by Machine' : 'Grouped by Order' }})
        </h3>

        @if (perspective() === 'machine') {
          <!-- Machine Perspective -->
          @for (grp of eventsByMachine(); track grp.machine) {
            <div style="margin-bottom: 1.5rem;">
              <h4 style="background: #f1f5f9; padding: 0.5rem 0.75rem; border-radius: 0.375rem; font-size: 0.95rem; font-weight: 700; color: var(--primary-dark); margin-bottom: 0.5rem;">
                ⚙️ Machine: {{ grp.machine }}
              </h4>
              <div class="table-container">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Time Slot</th>
                      <th>Order Code</th>
                      <th>Product</th>
                      <th>Stage</th>
                      <th>Assigned Labor</th>
                      <th>Duration</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (ev of grp.events; track ev.id) {
                      <tr>
                        <td><strong>{{ ev.startTime | date:'HH:mm' }} - {{ ev.endTime | date:'HH:mm' }}</strong></td>
                        <td>{{ ev.orderCode }}</td>
                        <td>{{ ev.productName }}</td>
                        <td>{{ ev.stageName }}</td>
                        <td>{{ ev.assignedOperators }} × {{ ev.operatorSkillName || 'None' }}</td>
                        <td>{{ ev.durationMinutes }} min</td>
                        <td>
                          @if (ev.hasMachineConflict) {
                            <span class="badge badge-conflict">⚠️ Overload</span>
                          } @else {
                            <span class="badge badge-fixed">OK</span>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          } @empty {
            <p style="text-align: center; color: var(--text-muted); padding: 2rem;">No machine operations found.</p>
          }
        } @else {
          <!-- Order Perspective -->
          @for (grp of eventsByOrder(); track grp.orderCode) {
            <div style="margin-bottom: 1.5rem;">
              <h4 style="background: #f1f5f9; padding: 0.5rem 0.75rem; border-radius: 0.375rem; font-size: 0.95rem; font-weight: 700; color: var(--primary-dark); margin-bottom: 0.5rem;">
                📦 Order: {{ grp.orderCode }} — {{ grp.productName }}
              </h4>
              <div class="table-container">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Seq</th>
                      <th>Stage</th>
                      <th>Labor Mode</th>
                      <th>Machine</th>
                      <th>Crew</th>
                      <th>Start Time</th>
                      <th>End Time</th>
                      <th>Duration</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (ev of grp.events; track ev.id) {
                      <tr>
                        <td><strong>{{ ev.sequenceOrder }}</strong></td>
                        <td>{{ ev.stageName }}</td>
                        <td>
                          <span class="badge" [ngClass]="{
                            'badge-passive': ev.laborMode === 'Passive',
                            'badge-fixed': ev.laborMode === 'FixedTeam',
                            'badge-scalable': ev.laborMode === 'ScalableLabor'
                          }">
                            {{ ev.laborMode }}
                          </span>
                        </td>
                        <td>{{ ev.machineTypeName || 'None (Ambient)' }}</td>
                        <td>{{ ev.assignedOperators }} × {{ ev.operatorSkillName || 'None' }}</td>
                        <td>{{ ev.startTime | date:'MMM d, HH:mm' }}</td>
                        <td>{{ ev.endTime | date:'HH:mm' }}</td>
                        <td>{{ ev.durationMinutes }} min</td>
                        <td>
                          @if (ev.hasMachineConflict) {
                            <span class="badge badge-conflict">⚠️ Overload</span>
                          } @else {
                            <span class="badge badge-fixed">OK</span>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          } @empty {
            <p style="text-align: center; color: var(--text-muted); padding: 2rem;">No orders scheduled in this period.</p>
          }
        }
      </div>
    </div>
  `
})
export class ScheduleReportComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly viewType = signal<'day' | 'week'>('week');
  readonly selectedDate = signal<string>(new Date().toISOString().substring(0, 10));
  readonly perspective = signal<'machine' | 'order'>('machine');
  readonly report = signal<ProductionScheduleReport | null>(null);

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.api.getScheduleReport(this.viewType(), this.selectedDate()).subscribe(rep => {
      this.report.set(rep);
    });
  }

  setViewType(type: 'day' | 'week'): void {
    this.viewType.set(type);
    this.loadReport();
  }

  onDateChange(date: string): void {
    this.selectedDate.set(date);
    this.loadReport();
  }

  printReport(): void {
    window.print();
  }

  exportUrl(): string {
    return this.api.getExportUrl(this.viewType(), this.selectedDate());
  }

  eventsByMachine() {
    const events = this.report()?.events || [];
    const map = new Map<string, ScheduledStage[]>();

    for (const ev of events) {
      const key = ev.machineTypeName || 'Ambient / No Machine';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(ev);
    }

    return Array.from(map.entries()).map(([machine, evts]) => ({
      machine,
      events: evts.sort((a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime())
    }));
  }

  eventsByOrder() {
    const events = this.report()?.events || [];
    const map = new Map<string, { productName: string; events: ScheduledStage[] }>();

    for (const ev of events) {
      const key = ev.orderCode;
      if (!map.has(key)) {
        map.set(key, { productName: ev.productName, events: [] });
      }
      map.get(key)!.events.push(ev);
    }

    return Array.from(map.entries()).map(([orderCode, data]) => ({
      orderCode,
      productName: data.productName,
      events: data.events.sort((a, b) => a.sequenceOrder - b.sequenceOrder)
    }));
  }
}
