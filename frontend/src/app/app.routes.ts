import { Routes } from '@angular/router';
import { ScreenHubComponent } from './components/screen-hub/screen-hub.component';
import { PlanningCalendarComponent } from './components/planning-calendar/planning-calendar.component';
import { ScheduleReportComponent } from './components/schedule-report/schedule-report.component';
import { ProductCatalogComponent } from './components/product-catalog/product-catalog.component';
import { MachineTypesComponent } from './components/machine-types/machine-types.component';
import { OperatorSkillsComponent } from './components/operator-skills/operator-skills.component';
import { WorkShiftsComponent } from './components/work-shifts/work-shifts.component';

export const routes: Routes = [
  { path: '', redirectTo: 'hub', pathMatch: 'full' },
  { path: 'hub', component: ScreenHubComponent, title: 'Krono - Navigation Hub' },
  { path: 'planning', component: PlanningCalendarComponent, title: 'Krono - Planning Calendar' },
  { path: 'reports', component: ScheduleReportComponent, title: 'Krono - Production Schedule Report' },
  { path: 'products', component: ProductCatalogComponent, title: 'Krono - Products & Routings' },
  { path: 'machines', component: MachineTypesComponent, title: 'Krono - Machine Types' },
  { path: 'skills', component: OperatorSkillsComponent, title: 'Krono - Operator Skills' },
  { path: 'shifts', component: WorkShiftsComponent, title: 'Krono - Factory Shifts' },
  { path: '**', redirectTo: 'hub' }
];
