# Krono — Production Planning & Control System (PPC / APS)

> *"Smart Production Planning & Scheduling | Just Krono it"*

Krono is an advanced Production Planning, Scheduling, and Resource Sizing system engineered for batch manufacturing workflows (piloted with industrial bakery and confectionery production).

---

## 🚀 Key Features

1. **Clean Architecture .NET Backend**:
   - `Krono.Domain`: Entities, DAG routing constraints, duration formulas (`Passive`, `FixedTeam`, `ScalableLabor`), SOLID strategies.
   - `Krono.Application`: Scheduling engine, `ShiftCalendarEngine` (handles operating hours & lunch breaks), `MachineConflictDetector` (fleet contention), `LaborDemandAggregator` (man-hours and peak concurrency).
   - `Krono.Infrastructure`: Entity Framework Core with PostgreSQL provider & automatic In-Memory fallback for immediate zero-config local runs, seeded with full Cupcake pilot data.
   - `Krono.API`: RESTful endpoints for Master Data, Order Scheduling, Timeline Events, Reports, and CSV exports.
2. **Modern Angular 18+ Frontend**:
   - Built exclusively with **Standalone Components** and **Angular Signals**.
   - **Central Navigation Hub**: Instant in-memory search bar reactive filtering.
   - **Interactive Planning Calendar**: Order simulation, timeline view, scalable operator crew adjustments, and machine conflict warnings.
   - **Consolidated Production Schedule Report**: Day & week horizon toggles, machine and order grouping, labor sizing breakdown (peak workers & total man-hours), dedicated A4 print layout, and CSV download.
   - **Master Data**: Products & Routings, Machine Types, Operator Skills, and Factory Shifts.
3. **Containerized Deployment**:
   - Multi-container `docker-compose.yml` (`krono-db`, `krono-backend`, `krono-frontend`).

---

## 🛠️ Quick Start

### 1. Run via Docker Compose (Recommended)

```bash
docker compose up --build
```
- **Frontend**: [http://localhost:4200](http://localhost:N4200)
- **Backend API**: [http://localhost:5000](http://localhost:5000)
- **Swagger Documentation**: [http://localhost:5000/swagger](http://localhost:5000/swagger)
- **PostgreSQL**: `localhost:5432`

---

### 2. Run Locally (Development)

#### Backend (.NET 8/9):
```bash
dotnet run --project backend/src/Krono.API/Krono.API.csproj --urls "http://localhost:5000"
```
*(If PostgreSQL is not running locally, it automatically falls back to an in-memory database with pre-seeded pilot data).*

#### Frontend (Angular 18):
```bash
cd frontend
npm start
```
Access the application at [http://localhost:4200](http://localhost:4200).

---

### 3. Run Automated Tests

```bash
dotnet test backend/Krono.sln
```
All unit tests validate:
- Fixed, passive, and scalable stage duration formulas.
- Factory shift progression across lunch breaks and weekends.
- Machine fleet concurrency overload detection.
- Labor demand aggregation (peak concurrent workers & total man-hours).
