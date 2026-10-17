import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { Product, MachineType, OperatorSkill, LaborAllocationMode } from '../../models';

@Component({
  selector: 'app-product-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div>
      <div class="card" style="display: flex; justify-content: space-between; align-items: center;">
        <div>
          <h2 style="font-size: 1.5rem; font-weight: 700; color: var(--primary);">Products & Manufacturing Routings</h2>
          <p style="color: var(--text-muted); font-size: 0.875rem;">
            Configure recipes, multi-stage DAG sequences, labor modes, and setup/cycle duration formulas.
          </p>
        </div>
        <button class="btn btn-primary" (click)="toggleCreateForm()">
          {{ showCreateForm() ? '✕ Close Form' : '➕ New Product' }}
        </button>
      </div>

      <!-- New Product Form -->
      @if (showCreateForm()) {
        <div class="card" style="border: 2px solid var(--primary-light); background-color: #f8fafc;">
          <h3 style="font-size: 1.15rem; font-weight: 700; color: var(--primary); margin-bottom: 1rem;">
            Create New Product Recipe
          </h3>

          <div class="grid grid-cols-3" style="margin-bottom: 1.25rem;">
            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Product Code</label>
              <input type="text" [(ngModel)]="newProduct.code" placeholder="e.g. BRD-201" class="input" />
            </div>
            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Product Name</label>
              <input type="text" [(ngModel)]="newProduct.name" placeholder="e.g. Artisanal Sourdough" class="input" />
            </div>
            <div>
              <label style="font-weight: 600; font-size: 0.85rem; display: block; margin-bottom: 0.3rem;">Category</label>
              <input type="text" [(ngModel)]="newProduct.category" placeholder="e.g. Bakery" class="input" />
            </div>
          </div>

          <button class="btn btn-success" (click)="saveProduct()">💾 Save Product</button>
        </div>
      }

      <!-- Products List -->
      <div style="display: flex; flex-direction: column; gap: 1.5rem;">
        @for (prod of products(); track prod.id) {
          <div class="card">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem;">
              <div>
                <span class="badge badge-fixed" style="margin-bottom: 0.25rem;">{{ prod.code }}</span>
                <h3 style="font-size: 1.25rem; font-weight: 700; color: var(--primary-dark);">{{ prod.name }}</h3>
                <span style="font-size: 0.85rem; color: var(--text-muted);">Category: {{ prod.category }}</span>
              </div>
              <button class="btn btn-danger" style="padding: 0.3rem 0.6rem; font-size: 0.8rem;" (click)="deleteProduct(prod.id)">
                🗑️ Delete
              </button>
            </div>

            <!-- Stages Table -->
            <h4 style="font-size: 0.95rem; font-weight: 700; color: var(--text-muted); margin-bottom: 0.5rem; text-transform: uppercase;">
              Routing Stages ({{ prod.stages.length }} steps)
            </h4>
            <div class="table-container">
              <table class="table">
                <thead>
                  <tr>
                    <th>Seq</th>
                    <th>Stage Name</th>
                    <th>Labor Mode</th>
                    <th>Machine</th>
                    <th>Specialty</th>
                    <th>Crew</th>
                    <th>Setup Time (min)</th>
                    <th>Prod Time (min)</th>
                  </tr>
                </thead>
                <tbody>
                  @for (st of prod.stages; track st.id) {
                    <tr>
                      <td><strong>{{ st.sequenceOrder }}</strong></td>
                      <td><strong>{{ st.name }}</strong></td>
                      <td>
                        <span class="badge" [ngClass]="{
                          'badge-passive': st.laborMode === 'Passive',
                          'badge-fixed': st.laborMode === 'FixedTeam',
                          'badge-scalable': st.laborMode === 'ScalableLabor'
                        }">
                          {{ st.laborMode }}
                        </span>
                      </td>
                      <td>{{ st.machineTypeName || 'None (Ambient)' }}</td>
                      <td>{{ st.operatorSkillName || 'None' }}</td>
                      <td>{{ st.defaultOperators }}</td>
                      <td>{{ st.setupFixedMinutes }} fixed + ({{ st.setupMultiplierMinutes }} × Q)</td>
                      <td>{{ st.prodFixedMinutes }} fixed + ({{ st.prodMultiplierMinutes }} × Q)</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class ProductCatalogComponent implements OnInit {
  private readonly api = inject(ApiService);

  readonly products = signal<Product[]>([]);
  readonly showCreateForm = signal<boolean>(false);

  newProduct = {
    code: '',
    name: '',
    category: '',
    stages: []
  };

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.api.getProducts().subscribe(prods => this.products.set(prods));
  }

  toggleCreateForm(): void {
    this.showCreateForm.update(v => !v);
  }

  saveProduct(): void {
    if (!this.newProduct.name || !this.newProduct.code) return;

    this.api.createProduct(this.newProduct).subscribe(() => {
      this.newProduct = { code: '', name: '', category: '', stages: [] };
      this.showCreateForm.set(false);
      this.loadProducts();
    });
  }

  deleteProduct(id: string): void {
    if (confirm('Are you sure you want to delete this product?')) {
      this.api.deleteProduct(id).subscribe(() => this.loadProducts());
    }
  }
}
