import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OperatorSkill,
  MachineType,
  WorkShift,
  Product,
  ProductionOrder,
  ScheduledStage,
  SimulationResult,
  ProductionScheduleReport
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = typeof window !== 'undefined' && window.location.port === '4200'
    ? 'http://localhost:5000/api/v1'
    : '/api/v1';

  // Skills
  getSkills(): Observable<OperatorSkill[]> {
    return this.http.get<OperatorSkill[]>(`${this.baseUrl}/operator-skills`);
  }

  createSkill(skill: Partial<OperatorSkill>): Observable<OperatorSkill> {
    return this.http.post<OperatorSkill>(`${this.baseUrl}/operator-skills`, skill);
  }

  deleteSkill(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/operator-skills/${id}`);
  }

  // Machines
  getMachines(): Observable<MachineType[]> {
    return this.http.get<MachineType[]>(`${this.baseUrl}/machine-types`);
  }

  createMachine(machine: Partial<MachineType>): Observable<MachineType> {
    return this.http.post<MachineType>(`${this.baseUrl}/machine-types`, machine);
  }

  deleteMachine(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/machine-types/${id}`);
  }

  // Products
  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.baseUrl}/products`);
  }

  getProduct(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/products/${id}`);
  }

  createProduct(product: Partial<Product>): Observable<Product> {
    return this.http.post<Product>(`${this.baseUrl}/products`, product);
  }

  updateProduct(id: string, product: Partial<Product>): Observable<Product> {
    return this.http.put<Product>(`${this.baseUrl}/products/${id}`, product);
  }

  deleteProduct(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${id}`);
  }

  // Scheduling & Orders
  simulate(payload: {
    productId: string;
    quantity: number;
    plannedStartTime: string;
    stageOperatorOverrides?: Record<string, number>;
  }): Observable<SimulationResult> {
    return this.http.post<SimulationResult>(`${this.baseUrl}/schedules/simulate`, payload);
  }

  createOrder(payload: {
    code?: string;
    productId: string;
    quantity: number;
    plannedStartTime: string;
    stageOperatorOverrides?: Record<string, number>;
  }): Observable<ProductionOrder> {
    return this.http.post<ProductionOrder>(`${this.baseUrl}/schedules/orders`, payload);
  }

  getOrders(): Observable<ProductionOrder[]> {
    return this.http.get<ProductionOrder[]>(`${this.baseUrl}/schedules/orders`);
  }

  getEvents(startDate: string, endDate: string): Observable<ScheduledStage[]> {
    return this.http.get<ScheduledStage[]>(`${this.baseUrl}/schedules/events?startDate=${startDate}&endDate=${endDate}`);
  }

  updateScheduledStage(
    scheduledStageId: string,
    payload: { startTime?: string; assignedOperators?: number }
  ): Observable<ScheduledStage> {
    return this.http.patch<ScheduledStage>(`${this.baseUrl}/schedules/events/${scheduledStageId}`, payload);
  }

  deleteOrder(orderId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/schedules/orders/${orderId}`);
  }

  // Reports
  getScheduleReport(viewType: 'day' | 'week', date: string): Observable<ProductionScheduleReport> {
    return this.http.get<ProductionScheduleReport>(
      `${this.baseUrl}/reports/production-schedule?viewType=${viewType}&date=${date}`
    );
  }

  getExportUrl(viewType: 'day' | 'week', date: string): string {
    return `${this.baseUrl}/reports/production-schedule/export?format=csv&viewType=${viewType}&date=${date}`;
  }

  // Work Shift
  getWorkShift(): Observable<WorkShift> {
    return this.http.get<WorkShift>(`${this.baseUrl}/work-shifts`);
  }

  updateWorkShift(shift: Partial<WorkShift>): Observable<WorkShift> {
    return this.http.put<WorkShift>(`${this.baseUrl}/work-shifts`, shift);
  }
}
