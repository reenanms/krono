export type LaborAllocationMode = 'Passive' | 'FixedTeam' | 'ScalableLabor';

export interface OperatorSkill {
  id: string;
  name: string;
  description: string;
}

export interface MachineType {
  id: string;
  name: string;
  description: string;
  totalAvailable: number;
}

export interface WorkShift {
  id: string;
  name: string;
  startHour: string;
  endHour: string;
  breakStart?: string;
  breakEnd?: string;
  workDays: string;
}

export interface ProductionStage {
  id: string;
  productId: string;
  name: string;
  sequenceOrder: number;
  machineTypeId?: string;
  machineTypeName?: string;
  machineQuantity: number;
  operatorSkillId?: string;
  operatorSkillName?: string;
  defaultOperators: number;
  laborMode: LaborAllocationMode;
  setupFixedMinutes: number;
  setupMultiplierMinutes: number;
  prodFixedMinutes: number;
  prodMultiplierMinutes: number;
  predecessorStageIds: string[];
}

export interface Product {
  id: string;
  code: string;
  name: string;
  category: string;
  stages: ProductionStage[];
}

export interface ScheduledStage {
  id: string;
  productionOrderId: string;
  orderCode: string;
  productId: string;
  productName: string;
  productionStageId: string;
  stageName: string;
  sequenceOrder: number;
  laborMode: LaborAllocationMode;
  assignedOperators: number;
  operatorSkillName?: string;
  machineTypeName?: string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  hasMachineConflict: boolean;
}

export interface ProductionOrder {
  id: string;
  code: string;
  productId: string;
  productName: string;
  quantity: number;
  plannedStartTime: string;
  scheduledStages: ScheduledStage[];
}

export interface SimulationStage {
  stageId: string;
  stageName: string;
  sequenceOrder: number;
  laborMode: LaborAllocationMode;
  durationMinutes: number;
  startTime: string;
  endTime: string;
  assignedOperators: number;
  skillName?: string;
  machineName?: string;
  hasConflict: boolean;
}

export interface SimulationResult {
  productId: string;
  productName: string;
  quantity: number;
  totalDurationMinutes: number;
  estimatedStartTime: string;
  estimatedEndTime: string;
  stages: SimulationStage[];
}

export interface LaborSizingSummary {
  skillId: string;
  skillName: string;
  peakWorkers: number;
  totalManHours: number;
}

export interface ProductionScheduleReport {
  viewType: 'day' | 'week';
  startDate: string;
  endDate: string;
  laborSummary: LaborSizingSummary[];
  events: ScheduledStage[];
}
