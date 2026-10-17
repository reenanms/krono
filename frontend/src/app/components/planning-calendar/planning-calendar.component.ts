import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Product, ScheduledStage, SimulationResult, ProductionOrder } from '../../models';

@Component({
  selector: 'app-planning-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <!-- Top Actions Bar -->
      <div class="card" style="display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem;">
        <div>
          <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Production Planning Calendar</h2>
          <p style="color: var(--text-muted); font-size: 0.875rem;">
            Schedule manufacturing orders, simulate cycle times, and resolve equipment contention.
          </p>
        </div>

        <div style="display: flex; gap: 0.75rem; align-items: center;">
          <input 
            type="date" 
            [ngModel]="selectedDate()"
            (ngModelChange)="onDateChange($event)"
            class="input"
            style="width: auto;"
          />
          <button class="btn btn-secondary" (click)="loadEvents()">🔄 Refresh</button>
          <button class="btn btn-primary" (click)="openNewOrderModal()">➕ New Production Order</button>
        </div>
      </div>

      <!-- Machine Conflict Banner (if any conflict present) -->
      @if (hasAnyConflicts()) {
        <div class="conflict-alert">
          <span style="font-size: 1.25rem;">⚠️</span>
          <div>
            <strong>Machine Fleet Contention Detected!</strong>
            <p style="font-size: 0.85rem; margin-top: 0.2rem;">
              One or more scheduled stages require more physical machines than currently available in factory inventory. Review red-highlighted stages below to resequence or stagger timings.
            </p>
          </div>
        </div>
      }

      <!-- Order Creation & Simulation Drawer / Panel -->
      @if (showOrderModal()) {
        <div class="card" style="border: 2px solid var(--primary-light); background-color: #f0f7ff;">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.25rem;">
            <h3 style="font-size: 1.25rem; font-weight: 700; color: var(--primary);">Schedule New Production Order</h3>
            <button class="btn btn-secondary" style="padding: 0.3rem 0.6rem;" (click)="showOrderModal.set(false)">✕ Close</button>
          </div>

          <div class="grid grid-cols-4" style="margin-bottom: 1.25rem;">
            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Product</label>
              <select [(ngModel)]="selectedProductId" class="select">
                <option value="" disabled>Select a product...</option>
                @for (prod of products(); track prod.id) {
                  <option [value]="prod.id">{{ prod.name }} ({{ prod.code }})</option>
                }
              </select>
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Order Code</label>
              <input type="text" [(ngModel)]="newOrderCode" placeholder="e.g. PO-101" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Batch Quantity (Units)</label>
              <input type="number" [(ngModel)]="newOrderQuantity" min="1" class="input" />
            </div>

            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Planned Start Time</label>
              <input type="datetime-local" [(ngModel)]="newOrderStartTime" class="input" />
            </div>
          </div>

          <div style="display: flex; gap: 0.75rem; margin-bottom: 1.5rem;">
            <button class="btn btn-secondary" (click)="simulateOrder()" [disabled]="!selectedProductId">
              🧪 Simulate Routing & Durations
            </button>
            <button class="btn btn-success" (click)="createOrder()" [disabled]="!selectedProductId">
              ✅ Schedule Order
            </button>
          </div>

          <!-- Simulation Preview -->
          @if (simulationResult()) {
            <div class="card" style="background: #ffffff; margin-bottom: 0;">
              <h4 style="font-size: 1rem; font-weight: 700; color: var(--primary-dark); margin-bottom: 0.75rem;">
                Routing Simulation Preview — Estimated Span: {{ simulationResult()?.totalDurationMinutes }} mins ({{ (simulationResult()!.totalDurationMinutes / 60) | number:'1.1-1' }} hrs)
              </h4>
              <div class="table-container">
                <table class="table">
                  <thead>
                    <tr>
                      <th>Seq</th>
                      <th>Stage</th>
                      <th>Labor Mode</th>
                      <th>Assigned Labor</th>
                      <th>Machine</th>
                      <th>Duration</th>
                      <th>Start Time</th>
                      <th>End Time</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (st of simulationResult()!.stages; track st.stageId) {
                      <tr>
                        <td><strong>{{ st.sequenceOrder }}</strong></td>
                        <td>{{ st.stageName }}</td>
                        <td>
                          <span class="badge" [ngClass]="{
                            'badge-passive': st.laborMode === 'Passive',
                            'badge-fixed': st.laborMode === 'FixedTeam',
                            'badge-scalable': st.laborMode === 'ScalableLabor'
                          }">
                            {{ st.laborMode }}
                          </span>
                        </td>
                        <td>{{ st.assignedOperators }} × {{ st.skillName || 'None' }}</td>
                        <td>{{ st.machineName || 'None (Ambient)' }}</td>
                        <td>{{ st.durationMinutes }} min</td>
                        <td>{{ st.startTime | date:'shortTime' }}</td>
                        <td>{{ st.endTime | date:'shortTime' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            </div>
          }
        </div>
      }

      <!-- Timeline & Scheduled Stages View -->
      <div class="card">
        <h3 style="font-size: 1.15rem; font-weight: 700; margin-bottom: 1rem; color: var(--text-main);">
          Scheduled Operations for Horizon ({{ scheduledEvents().length }} stages active)
        </h3>

        @if (scheduledEvents().length === 0) {
          <div style="text-align: center; padding: 3rem; color: var(--text-muted);">
            <p style="font-size: 1.1rem; margin-bottom: 0.5rem;">No operations scheduled for this period.</p>
            <button class="btn btn-primary" (click)="openNewOrderModal()">Schedule an Order Now</button>
          </div>
        } @else {
          <div style="display: flex; flex-direction: column; gap: 1rem;">
            @for (event of scheduledEvents(); track event.id) {
              <div 
                class="card" 
                style="margin-bottom: 0; transition: transform 0.15s ease;"
                [style.border-color]="event.hasMachineConflict ? 'var(--danger)' : (event.laborMode === 'Passive' ? 'var(--passive-border)' : 'var(--border-color)')"
                [style.border-width]="event.hasMachineConflict ? '2px' : '1px'"
                [style.border-style]="event.laborMode === 'Passive' ? 'dashed' : 'solid'"
                [style.background-color]="event.hasMachineConflict ? '#fff8f8' : (event.laborMode === 'Passive' ? 'var(--passive-bg)' : '#ffffff')"
              >
                <div style="display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 0.75rem;">
                  <div>
                    <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.25rem;">
                      <span style="font-weight: 700; font-size: 1.1rem; color: var(--primary);">
                        {{ event.orderCode }}
                      </span>
                      <span style="color: var(--text-muted);">|</span>
                      <span style="font-weight: 600;">{{ event.productName }}</span>
                      <span class="badge" [ngClass]="{
                        'badge-passive': event.laborMode === 'Passive',
                        'badge-fixed': event.laborMode === 'FixedTeam',
                        'badge-scalable': event.laborMode === 'ScalableLabor'
                      }">
                        {{ event.laborMode }}
                      </span>

                      @if (event.hasMachineConflict) {
                        <span class="badge badge-conflict">
                          ⚠️ Machine Fleet Overload
                        </span>
                      }
                    </div>

                    <div style="font-size: 0.9rem; color: var(--text-muted); display: flex; gap: 1.5rem; flex-wrap: wrap; margin-top: 0.5rem;">
                      <div><strong>Stage:</strong> {{ event.sequenceOrder }}. {{ event.stageName }}</div>
                      <div><strong>Machine:</strong> {{ event.machineTypeName || 'None (Ambient)' }}</div>
                      <div><strong>Specialty:</strong> {{ event.operatorSkillName || 'Unassigned' }}</div>
                      <div><strong>Timing:</strong> {{ event.startTime | date:'MMM d, HH:mm' }} → {{ event.endTime | date:'HH:mm' }} ({{ event.durationMinutes }} min)</div>
                    </div>
                  </div>

                  <!-- Actions / Dynamic Adjustments -->
                  <div style="display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap;">
                    <!-- Scalable Headcount Adjustment -->
                    @if (event.laborMode === 'ScalableLabor') {
                      <div style="display: flex; align-items: center; gap: 0.4rem; background: #fffbe6; padding: 0.3rem 0.6rem; border-radius: 0.375rem; border: 1px solid #fde68a;">
                        <span style="font-size: 0.8rem; font-weight: 600; color: #92400e;">Crew:</span>
                        <button class="btn btn-secondary" style="padding: 0.1rem 0.4rem; font-size: 0.75rem;" (click)="adjustOperators(event, -1)" [disabled]="event.assignedOperators <= 1">-</button>
                        <strong style="font-size: 0.9rem; min-width: 1.2rem; text-align: center;">{{ event.assignedOperators }}</strong>
                        <button class="btn btn-secondary" style="padding: 0.1rem 0.4rem; font-size: 0.75rem;" (click)="adjustOperators(event, 1)">+</button>
                      </div>
                    } @else {
                      <span style="font-size: 0.85rem; color: var(--text-muted);">
                        Crew: <strong>{{ event.assignedOperators }}</strong>
                      </span>
                    }

                    <!-- Drag/Shift Time Adjustments -->
                    <div style="display: flex; gap: 0.25rem;">
                      <button class="btn btn-secondary" style="padding: 0.3rem 0.5rem; font-size: 0.8rem;" title="Shift earlier 30m" (click)="shiftStageTime(event, -30)">-30m</button>
                      <button class="btn btn-secondary" style="padding: 0.3rem 0.5rem; font-size: 0.8rem;" title="Shift later 30m" (click)="shiftStageTime(event, 30)">+30m</button>
                    </div>

                    <button class="btn btn-danger" style="padding: 0.3rem 0.6rem; font-size: 0.8rem;" (click)="deleteOrder(event.productionOrderId)">
                      🗑️ Delete Order
                    </button>
                  </div>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `
})
export class PlanningCalendarComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly selectedDate = signal<string>(new Date().toISOString().substring(0, 10));
  readonly products = signal<Product[]>([]);
  readonly scheduledEvents = signal<ScheduledStage[]>([]);
  readonly showOrderModal = signal<boolean>(false);
  readonly simulationResult = signal<SimulationResult | null>(null);

  selectedProductId = '';
  newOrderCode = '';
  newOrderQuantity = 1000;
  newOrderStartTime = '';

  ngOnInit(): void {
    // Default start time: today at 08:00
    const today = new Date();
    today.setHours(8, 0, 0, 0);
    this.newOrderStartTime = today.toISOString().substring(0, 16);

    this.loadProducts();
    this.loadEvents();
  }

  loadProducts(): void {
    this.api.getProducts().subscribe(prods => {
      this.products.set(prods);
      if (prods.length > 0 && !this.selectedProductId) {
        this.selectedProductId = prods[0].id;
      }
    });
  }

  loadEvents(): void {
    const curDate = new Date(this.selectedDate());
    const startDate = new Date(curDate);
    startDate.setDate(startDate.getDate() - 2);
    const endDate = new Date(curDate);
    endDate.setDate(endDate.getDate() + 7);

    this.api.getEvents(startDate.toISOString().substring(0, 10), endDate.toISOString().substring(0, 10))
      .subscribe(events => {
        this.scheduledEvents.set(events);
      });
  }

  onDateChange(newDate: string): void {
    this.selectedDate.set(newDate);
    this.loadEvents();
  }

  hasAnyConflicts(): boolean {
    return this.scheduledEvents().some(e => e.hasMachineConflict);
  }

  openNewOrderModal(): void {
    this.newOrderCode = `PO-${Math.floor(100 + Math.random() * 900)}`;
    this.showOrderModal.set(true);
    if (this.selectedProductId) {
      this.simulateOrder();
    }
  }

  simulateOrder(): void {
    if (!this.selectedProductId) return;

    this.api.simulate({
      productId: this.selectedProductId,
      quantity: this.newOrderQuantity,
      plannedStartTime: new Date(this.newOrderStartTime).toISOString()
    }).subscribe(res => {
      this.simulationResult.set(res);
    });
  }

  createOrder(): void {
    if (!this.selectedProductId) return;

    this.api.createOrder({
      code: this.newOrderCode,
      productId: this.selectedProductId,
      quantity: this.newOrderQuantity,
      plannedStartTime: new Date(this.newOrderStartTime).toISOString()
    }).subscribe(() => {
      this.showOrderModal.set(false);
      this.loadEvents();
    });
  }

  adjustOperators(event: ScheduledStage, delta: number): void {
    const newCount = event.assignedOperators + delta;
    if (newCount < 1) return;

    this.api.updateScheduledStage(event.id, { assignedOperators: newCount })
      .subscribe(() => {
        this.loadEvents();
      });
  }

  shiftStageTime(event: ScheduledStage, deltaMinutes: number): void {
    const current = new Date(event.startTime);
    current.setMinutes(current.getMinutes() + deltaMinutes);

    this.api.updateScheduledStage(event.id, { startTime: current.toISOString() })
      .subscribe(() => {
        this.loadEvents();
      });
  }

  deleteOrder(orderId: string): void {
    if (confirm('Are you sure you want to delete this order from the schedule?')) {
      this.api.deleteOrder(orderId).subscribe(() => {
        this.loadEvents();
      });
    }
  }
}
