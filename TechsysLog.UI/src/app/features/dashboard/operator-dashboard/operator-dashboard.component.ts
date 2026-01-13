import { Component, Input, OnInit, OnChanges, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl } from '@angular/forms';
import { OrderService } from '../../../core/services/order.service';
import { UserService, User } from '../../../core/services/user.service';
import { AddressService } from '../../../core/services/address.service';
import { HttpClient } from '@angular/common/http';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Order } from '../../../core/models/Order';
import { OrderStatus } from '../../../core/models/OrderStatus';

interface CreateOrderRequest {
  description: string;
  amount: number;
  address: {
    zipCode: string;
    street: string;
    number: string;
    neighborhood: string;
    city: string;
    state: string;
  };
  userId: string;
}

interface DeliveryRequest {
  orderNumber: string;
  userReceived: string;
  notes?: string;
}

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="operator-dashboard">
      <!-- Botões de Ação Rápida -->
      <div class="action-buttons">
        <button class="btn btn-primary" (click)="openCreateOrderForm()">
           Novo Pedido
        </button>
      </div>

      <!-- Estatísticas -->
      <div class="stats-container">
        <div class="stat-card">
          <span class="material-icons stat-icon">inventory_2</span>
          <div class="stat-content">
            <h3>Total</h3>
            <p class="stat-value">{{ stats.total }}</p>
          </div>
        </div>

        <div class="stat-card pending">
          <span class="material-icons stat-icon">settings</span>
          <div class="stat-content">
            <h3>Pendentes</h3>
            <p class="stat-value">{{ stats.pending }}</p>
          </div>
        </div>

        <div class="stat-card">
          <span class="material-icons stat-icon">change_circle</span>
          <div class="stat-content">
            <h3>Em Processamento</h3>
            <p class="stat-value">{{ stats.processing }}</p>
          </div>
        </div>

        <div class="stat-card delivered">
          <span class="material-icons stat-icon">check_circle</span>
          <div class="stat-content">
            <h3>Entregues</h3>
            <p class="stat-value">{{ stats.delivered }}</p>
          </div>
        </div>
      </div>

      <!-- Filtros -->
      <div class="filters-section">
        <div class="filter-group">
          <label for="search">Buscar:</label>
          <input
            id="search"
            type="text"
            placeholder="Número ou descrição..."
            (input)="onSearchChange($any($event.target).value)"
            class="search-input"
          />
        </div>

        <div class="filter-group">
          <label>Status:</label>
          <div class="status-buttons">
            <button
              *ngFor="let status of statusOptions"
              [class.active]="statusFilter === status.value"
              (click)="changeStatusFilter(status.value)"
              class="status-btn"
            >
              {{ status.label }}
            </button>
          </div>
        </div>
      </div>

      <!-- Tabela de Pedidos -->
      <div class="orders-table-section">
        <h2>Pedidos</h2>
        <div *ngIf="filteredOrders.length > 0" class="table-responsive">
          <table class="orders-table">
            <thead>
              <tr>
                <th>Nº Pedido</th>
                <th>Cliente</th>
                <th>Descrição</th>
                <th>Valor</th>
                <th>Status</th>
                <th>Data</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let order of filteredOrders" class="order-row">
                <td class="order-number">
                  <strong>{{ order.orderNumber }}</strong>
                </td>
                <td>{{ order.clientName }}</td>
                <td class="description">
                  {{ order.description | slice: 0: 30 }}...
                </td>
                <td class="amount">R$ {{ order.amount | number: '1.2-2' }}</td>
              <td>
              <span
                class="material-icons stat-icon"
                [ngClass]="getStatusIconClass(order.status)"
              >
                {{ getStatusIcon(order.status) }}
              </span>
              <span class="status-label">
                {{ getStatusLabel(order.status) }}
              </span>
            </td>

                <td>{{ order.createdAt | date: 'short' }}</td>
                <td class="actions">
                  <div class="action-buttons-group">
                    <button
                      class="btn-action view"
                      title="Ver detalhes"
                      (click)="viewOrderDetails(order)"
                    >
                      <span class="material-icons stat-icon icon-warning">
                      visibility
                      </span>
                    </button>
                    <button
                      class="btn-action status"
                      title="Alterar status"
                      (click)="openStatusChangeForm(order)"
                      [disabled]="order.status === OrderStatus.Delivered || order.status === OrderStatus.Cancelled"
                    >
                     <span class="material-icons stat-icon icon-default">edit_note</span>

                    </button>
                    <button
                      class="btn-action delivery"
                      title="Registrar entrega"
                      (click)="openDeliveryForm(order)"
                      [disabled]="order.status === OrderStatus.Delivered || order.status === OrderStatus.Cancelled || order.status === OrderStatus.Pending"
                    >
                    <span class="material-icons stat-icon icon-success">check_circle</span>

                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Modal: Criar Pedido - ATUALIZADO -->
      <div
        *ngIf="showCreateOrderForm"
        class="modal-overlay"
        (click)="closeCreateOrderForm()"
      >
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>Criar Novo Pedido</h2>
            <button class="close-btn" (click)="closeCreateOrderForm()">✕</button>
          </div>

          <div class="modal-body">
            <div *ngIf="errorMessage" class="error-message">
              <span class="material-icons error-icon">warning</span>
              <div>{{ errorMessage }}</div>
            </div>
            <div *ngIf="successMessage" class="success-message">
              <span class="material-icons success-icon">check_circle</span>
              <div>{{ successMessage }}</div>
            </div>

            <form [formGroup]="createOrderForm" (ngSubmit)="onSubmitCreateOrder()">
              <!-- Cliente (Select com usuários) -->
              <div class="form-group">
                <label>Cliente <span class="required">*</span></label>
                <select
                  formControlName="userId"
                  class="form-control"
                  [disabled]="loading || loadingUsers"
                >
                  <option [value]="null">
                    {{ loadingUsers ? 'Carregando usuários...' : 'Selecione um cliente' }}
                  </option>
                  <option *ngFor="let user of users" [value]="user.id">
                    {{ user.name }} ({{ user.email }})
                  </option>
                </select>
              </div>

              <!-- Descrição -->
              <div class="form-group">
                <label>Descrição <span class="required">*</span></label>
                <textarea
                  formControlName="description"
                  placeholder="Descrição do pedido"
                  class="form-control"
                  rows="3"
                  [disabled]="loading"
                ></textarea>
              </div>

              <!-- Valor -->
              <div class="form-group">
                <label>Valor (R$) <span class="required">*</span></label>
                <input
                  type="number"
                  step="0.01"
                  formControlName="amount"
                  placeholder="0.00"
                  class="form-control"
                  [disabled]="loading"
                />
              </div>

              <!-- CEP (com busca automática) -->
              <div class="form-row">
                <div class="form-group">
                  <label>CEP <span class="required">*</span></label>
                  <input
                    type="text"
                    formControlName="zipCode"
                    placeholder="00000-000"
                    class="form-control"
                    [disabled]="loading || searchingCep"
                    (blur)="searchCepAndFillAddress()"
                  />
                <small *ngIf="searchingCep" class="text-info">
                  <span class="material-icons">search</span>
                  Buscando CEP...
                 </small>
                </div>
              </div>

              <!-- Endereço (auto-preenchido) -->
              <div class="form-group">
                <label>Rua <span class="required">*</span></label>
                <input
                  type="text"
                  formControlName="street"
                  placeholder="Rua..."
                  class="form-control"
                  [disabled]="true"
                />
              </div>

              <!-- Bairro -->
              <div class="form-group">
                <label>Bairro <span class="required">*</span></label>
                <input
                  type="text"
                  formControlName="neighborhood"
                  placeholder="Bairro"
                  class="form-control"
                  [disabled]="true"
                />
              </div>

              <!-- Número e Cidade -->
              <div class="form-row">
                <div class="form-group">
                  <label>Número <span class="required">*</span></label>
                  <input
                    type="text"
                    formControlName="number"
                    placeholder="000"
                    class="form-control"
                    [disabled]="loading"
                  />
                </div>
                <div class="form-group">
                  <label>Cidade <span class="required">*</span></label>
                  <input
                    type="text"
                    formControlName="city"
                    placeholder="Cidade"
                    class="form-control"
                    [disabled]="true"
                  />
                </div>
              </div>

              <!-- Estado -->
              <div class="form-row">
                <div class="form-group">
                  <label>Estado <span class="required">*</span></label>
                  <input
                    type="text"
                    formControlName="state"
                    placeholder="UF"
                    class="form-control"
                    maxlength="2"
                    [disabled]="true"
                  />
                </div>
              </div>

              <button type="submit" class="btn btn-primary" [disabled]="loading || loadingUsers">
                {{ loading ? 'Criando...' : 'Criar Pedido' }}
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Modal: Registrar Entrega -->
      <div
        *ngIf="showDeliveryForm && selectedDeliveryOrder"
        class="modal-overlay"
        (click)="closeDeliveryForm()"
      >
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>Registrar Entrega - {{ selectedDeliveryOrder.orderNumber }}</h2>
            <button class="close-btn" (click)="closeDeliveryForm()">✕</button>
          </div>

          <div class="modal-body">
            <div *ngIf="errorMessage" class="error-message">
              <span class="error-icon"></span>
              <div>{{ errorMessage }}</div>
            </div>
            <div *ngIf="successMessage" class="success-message">
              <span class="success-icon"></span>
              <div>{{ successMessage }}</div>
            </div>

            <form [formGroup]="deliveryForm" (ngSubmit)="onSubmitDelivery()">
              <div class="form-group">
                <label>Recebido Por</label>
                <input
                  type="text"
                  formControlName="userReceived"
                  placeholder="Nome de quem recebeu"
                  class="form-control"
                  [disabled]="loading"
                  required
                />
              </div>

              <div class="form-group">
                <label>Notas</label>
                <textarea
                  formControlName="notes"
                  placeholder="Observações sobre a entrega..."
                  class="form-control"
                  rows="3"
                  [disabled]="loading"
                ></textarea>
              </div>

              <button type="submit" class="btn btn-primary" [disabled]="loading">
                {{ loading ? 'Registrando...' : 'Registrar Entrega' }}
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Modal: Alterar Status -->
      <div
        *ngIf="showStatusChangeForm && selectedStatusOrder"
        class="modal-overlay"
        (click)="closeStatusChangeForm()"
      >
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>Alterar Status - {{ selectedStatusOrder.orderNumber }}</h2>
            <button class="close-btn" (click)="closeStatusChangeForm()">✕</button>
          </div>

          <div class="modal-body">
            <div *ngIf="errorMessage" class="error-message">
              <span class="error-icon"></span>
              <div>{{ errorMessage }}</div>
            </div>
            <div *ngIf="successMessage" class="success-message">
              <span class="success-icon"></span>
              <div>{{ successMessage }}</div>
            </div>

            <form [formGroup]="statusChangeForm" (ngSubmit)="onSubmitStatusChange()">
              <div class="form-group">
                <label>Novo Status</label>
                <select formControlName="newStatus" class="form-control" [disabled]="loading">
                  <option [ngValue]="null">Selecione um status</option>
                  <option
                    *ngFor="let status of validStatusOptions"
                    [ngValue]="status.value"
                  >
                    {{ status.label }}
                  </option>
                </select>
              </div>

              <div class="form-group">
                <label>Motivo (opcional)</label>
                <textarea
                  formControlName="reason"
                  placeholder="Motivo da alteração..."
                  class="form-control"
                  rows="3"
                  [disabled]="loading"
                ></textarea>
              </div>

              <button type="submit" class="btn btn-primary" [disabled]="loading">
                {{ loading ? 'Alterando...' : 'Alterar Status' }}
              </button>
            </form>
          </div>
        </div>
      </div>

      <!-- Modal: Detalhes do Pedido -->
      <div
        *ngIf="showOrderDetails && selectedOrder"
        class="modal-overlay"
        (click)="closeOrderDetails()"
      >
        <div class="modal modal-large" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>{{ selectedOrder.orderNumber }}</h2>
            <button class="close-btn" (click)="closeOrderDetails()">✕</button>
          </div>

          <div class="modal-body">
            <div class="detail-row">
              <div class="detail-col">
                <h4>Descrição</h4>
                <p>{{ selectedOrder.description }}</p>
              </div>
            </div>

            <div class="detail-row">
              <div class="detail-col">
                <h4>Valor</h4>
                <p>R$ {{ selectedOrder.amount | number: '1.2-2' }}</p>
              </div>
              <div class="detail-col">
                <h4>Status</h4>
             
              <span class="status-label">
                {{ getStatusLabel(selectedOrder.status) }}
              </span>
              </div>
              
            </div>

            <div class="detail-row">
              <div class="detail-col">
                <h4>Endereço</h4>
                <p>{{ selectedOrder.shippingAddress.street }}, {{ selectedOrder.shippingAddress.number }}</p>
                <p>{{ selectedOrder.shippingAddress.neighborhood }} - {{ selectedOrder.shippingAddress.city }}, {{ selectedOrder.shippingAddress.state }}</p>
                <p>CEP: {{ selectedOrder.shippingAddress.zipCode }}</p>
              </div>
            </div>

            <div class="detail-row" *ngIf="selectedOrder.history && selectedOrder.history.length > 0">
              <div class="detail-col">
                <h4>Histórico</h4>
                <div class="history">
                  <div *ngFor="let item of selectedOrder.history" class="history-item">
                    <p>{{ getStatusLabel(item.previousStatus) }} → {{ getStatusLabel(item.newStatus) }}</p>
                    <p *ngIf="item.reason">Obs: {{ item.reason }}</p>
                    <small>{{ item.createdAt | date: 'short' }}</small>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./operator-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OperatorDashboardComponent implements OnInit, OnChanges, OnDestroy {
  @Input() orders: Order[] = [];

  filteredOrders: Order[] = [];
  selectedOrder: Order | null = null;
  showOrderDetails = false;
  showCreateOrderForm = false;
  showDeliveryForm = false;
  showStatusChangeForm = false;

  // Usuários
  users: User[] = [];
  loadingUsers = false;

  // Endereço
  searchingCep = false;

  // Formulários
  createOrderForm!: FormGroup;
  deliveryForm!: FormGroup;
  statusChangeForm!: FormGroup;

  // Filtros
  statusFilter: OrderStatus | 'All' = 'All';
  searchFilter = '';
  selectedDeliveryOrder: Order | null = null;
  selectedStatusOrder: Order | null = null;

  // Estado
  loading = false;
  successMessage: string | null = null;
  errorMessage: string | null = null;

  private destroy$ = new Subject<void>();

  OrderStatus = OrderStatus;
  statusOptions: { value: OrderStatus | 'All'; label: string }[] = [
    { value: 'All', label: 'Todos' },
    { value: OrderStatus.Pending, label: 'Aguardando processamento' },
    { value: OrderStatus.Processing, label: 'Em processamento' },
    { value: OrderStatus.Shipped, label: 'Enviado' },
    { value: OrderStatus.Delivered, label: 'Entregue' },
    { value: OrderStatus.Cancelled, label: 'Cancelado' },
    { value: OrderStatus.Returned, label: 'Devolvido' }
  ];

  validStatusOptions: { value: OrderStatus; label: string }[] = [
    { value: OrderStatus.Pending, label: 'Aguardando processamento' },
    { value: OrderStatus.Processing, label: 'Em processamento' },
    { value: OrderStatus.Shipped, label: 'Enviado' },
    { value: OrderStatus.Delivered, label: 'Entregue' },
    { value: OrderStatus.Cancelled, label: 'Cancelado' },
    { value: OrderStatus.Returned, label: 'Devolvido' }
  ];

  stats = {
    total: 0,
    pending: 0,
    processing: 0,
    delivered: 0
  };

  @Output() orderCreated = new EventEmitter<void>();

  constructor(
    private formBuilder: FormBuilder,
    private orderService: OrderService,
    private userService: UserService,
    private addressService: AddressService,
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.updateStats();
    this.applyFilters();
    this.loadUsers();
  }

  ngOnChanges(): void {
    this.updateStats();
    this.applyFilters();
    this.cdr.markForCheck();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Carrega usuários da API
   */
  private loadUsers(): void {
    this.loadingUsers = true;
    this.cdr.markForCheck();

    this.userService.getAllUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.isSuccess) {
            this.users = response.data;
          }
          this.loadingUsers = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = 'Erro ao carregar usuários';
          this.loadingUsers = false;
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Busca CEP e preenche endereço automaticamente
   */
  searchCepAndFillAddress(): void {
    const zipCode = this.createOrderForm.get('zipCode')?.value;
    if (!zipCode || zipCode.trim() === '') {
      return;
    }

    this.searchingCep = true;
    this.cdr.markForCheck();

    this.addressService.searchByZipcode(zipCode)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (address) => {

          // Preenche os campos automaticamente
          this.createOrderForm.patchValue({
            street: address.street,
            neighborhood: address.neighborhood,
            city: address.city,
            state: address.state,
            zipCode: address.zipCode
          });

          this.searchingCep = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.errorMessage = 'CEP não encontrado. Preencha manualmente.';
          this.searchingCep = false;
          this.cdr.markForCheck();
        }
      });
  }

  private initializeForms(): void {
    this.createOrderForm = this.formBuilder.group({
      userId: ['', Validators.required],
      description: ['', Validators.required],
      amount: ['', [Validators.required, Validators.min(0)]],
      zipCode: ['', Validators.required],
      street: ['', Validators.required],
      number: ['', Validators.required],
      neighborhood: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required]
    });

    this.deliveryForm = this.formBuilder.group({
      userReceived: ['', Validators.required],
      notes: ['']
    });

    this.statusChangeForm = this.formBuilder.group({
      newStatus: [null, Validators.required],
      reason: ['']
    });
  }

  private updateStats(): void {
    this.stats = {
      total: this.orders.length,
      pending: this.orders.filter(o => o.status === OrderStatus.Pending).length,
      processing: this.orders.filter(o => o.status === OrderStatus.Processing).length,
      delivered: this.orders.filter(o => o.status === OrderStatus.Delivered).length
    };
    this.cdr.markForCheck();
  }

  applyFilters(): void {
    this.filteredOrders = this.orders.filter(order => {
      const statusMatch =
        this.statusFilter === 'All' || order.status === this.statusFilter;

      const searchMatch =
        order.orderNumber.toLowerCase().includes(this.searchFilter.toLowerCase()) ||
        order.description.toLowerCase().includes(this.searchFilter.toLowerCase());

      return statusMatch && searchMatch;
    });
    this.cdr.markForCheck();
  }

  changeStatusFilter(status: OrderStatus | 'All'): void {
    this.statusFilter = status;
    this.applyFilters();
  }

  onSearchChange(search: string): void {
    this.searchFilter = search;
    this.applyFilters();
  }

  // === CRIAR PEDIDO ===
  openCreateOrderForm(): void {
    this.showCreateOrderForm = true;
    this.createOrderForm.reset();
    this.successMessage = null;
    this.errorMessage = null;
    this.cdr.markForCheck();
  }

  closeCreateOrderForm(): void {
    this.showCreateOrderForm = false;
    this.cdr.markForCheck();
  }

  onSubmitCreateOrder(): void {
    if (this.createOrderForm.invalid) {
      this.errorMessage = 'Preencha todos os campos obrigatórios';
      this.cdr.markForCheck();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;
    this.cdr.markForCheck();

    const formValue = this.createOrderForm.value;

    const createOrderRequest: CreateOrderRequest = {
      description: formValue.description,
      amount: parseFloat(formValue.amount),
      address: {
        zipCode: formValue.zipCode,
        street: formValue.street,
        number: formValue.number,
        neighborhood: formValue.neighborhood,
        city: formValue.city,
        state: formValue.state
      },
      userId: formValue.userId
    };


    this.orderService.createOrder(createOrderRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const isSuccess = response?.isSuccess || response?.isSuccess;

          if (isSuccess) {
            this.successMessage = 'Pedido criado com sucesso!';
            this.createOrderForm.reset();
            this.cdr.markForCheck();

            setTimeout(() => {
              this.closeCreateOrderForm();
              this.successMessage = null;
              this.cdr.markForCheck();
              this.orderCreated.emit();
            }, 2000);
          } else {
            this.errorMessage = response?.message || 'Erro ao criar pedido';
            this.cdr.markForCheck();
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || error?.message || 'Erro desconhecido';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  // === REGISTRAR ENTREGA ===
  openDeliveryForm(order: Order): void {
    this.selectedDeliveryOrder = order;
    this.showDeliveryForm = true;
    this.deliveryForm.reset();
    this.errorMessage = null;
    this.successMessage = null;
    this.cdr.markForCheck();
  }

  closeDeliveryForm(): void {
    this.showDeliveryForm = false;
    this.selectedDeliveryOrder = null;
    this.cdr.markForCheck();
  }

  onSubmitDelivery(): void {
    if (this.deliveryForm.invalid || !this.selectedDeliveryOrder) return;

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;
    this.cdr.markForCheck();

    const deliveryRequest: DeliveryRequest = {
      orderNumber: this.selectedDeliveryOrder.orderNumber,
      userReceived: this.deliveryForm.get('userReceived')?.value,
      notes: this.deliveryForm.get('notes')?.value
    };

    this.orderService.registerDelivery(deliveryRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const isSuccess = response?.isSuccess || response?.success;

          if (isSuccess) {
            this.successMessage = 'Entrega registrada com sucesso!';
            this.cdr.markForCheck();

            setTimeout(() => {
              this.closeDeliveryForm();
              this.successMessage = null;
              this.cdr.markForCheck();
            }, 2000);
          } else {
            this.errorMessage = response?.message || 'Erro ao registrar entrega';
            this.cdr.markForCheck();
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || error?.message || 'Erro desconhecido';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }
// === DETALHES DO PEDIDO ===
  viewOrderDetails(order: Order): void {
    this.selectedOrder = order;
    this.showOrderDetails = true;
    this.cdr.markForCheck();
  }
    closeStatusChangeForm(): void {
    this.showStatusChangeForm = false;
    this.selectedStatusOrder = null;
    this.cdr.markForCheck();
  }

  closeOrderDetails(): void {
    this.showOrderDetails = false;
    this.selectedOrder = null;
    this.cdr.markForCheck();
  }

  // === ALTERAR STATUS ===
  openStatusChangeForm(order: Order): void {
    this.selectedStatusOrder = order;
    this.showStatusChangeForm = true;
    this.statusChangeForm.reset();
  }

  onSubmitStatusChange(): void {
    if (this.statusChangeForm.invalid || !this.selectedStatusOrder) return;

    this.loading = true;
    this.errorMessage = null;
    this.successMessage = null;
    this.cdr.markForCheck();

    const newStatus = this.statusChangeForm.get('newStatus')?.value ;
    const reason = this.statusChangeForm.get('reason')?.value;


    this.orderService.changeOrderStatus(
      this.selectedStatusOrder.orderNumber,
      newStatus,
      reason
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const isSuccess = response?.isSuccess || response?.isSuccess;

          if (isSuccess) {
            this.successMessage = 'Status alterado com sucesso!';
            this.cdr.markForCheck();

            setTimeout(() => {
              this.closeStatusChangeForm();
              this.successMessage = null;
              this.cdr.markForCheck();
            }, 2000);
          } else {
            this.errorMessage = response?.message || 'Erro ao alterar status';
            this.cdr.markForCheck();
          }
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = error?.error?.message || error?.message || 'Erro desconhecido';
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

   getStatusColor(status: OrderStatus | number): string {
    const colors: { [key: number]: string } = {
      [OrderStatus.Pending]: '#ffc107',
      [OrderStatus.Processing]: '#17a2b8',
      [OrderStatus.Shipped]: '#007bff',
      [OrderStatus.Delivered]: '#28a745',
      [OrderStatus.Cancelled]: '#dc3545',
      [OrderStatus.Returned]: '#ff6c00'
    };
    return colors[status as number] || '#6c757d';
  }

  getStatusLabel(status: OrderStatus | number): string {
    const labels: { [key: number]: string } = {
      [OrderStatus.Pending]: 'Aguardando processamento',
      [OrderStatus.Processing]: 'Em processamento',
      [OrderStatus.Shipped]: 'Enviado',
      [OrderStatus.Delivered]: 'Entregue',
      [OrderStatus.Cancelled]: 'Cancelado',
      [OrderStatus.Returned]: 'Devolvido'
    };
    return labels[status as number] || 'Desconhecido';
  }



getStatusIcon(status: OrderStatus): string {
  const icons: Record<OrderStatus, string> = {
    [OrderStatus.Pending]: 'hourglass_empty',
    [OrderStatus.Processing]: 'settings',
    [OrderStatus.Shipped]: 'local_shipping',
    [OrderStatus.Delivered]: 'check_circle',
    [OrderStatus.Cancelled]: 'cancel',
    [OrderStatus.Returned]: 'assignment_return'
  };

  return icons[status];
}
  getStatusIconClass(status: OrderStatus): string {
  const classes: Record<OrderStatus, string> = {
    [OrderStatus.Pending]: 'icon-warning',
    [OrderStatus.Processing]: 'icon-info',
    [OrderStatus.Shipped]: 'icon-primary',
    [OrderStatus.Delivered]: 'icon-success',
    [OrderStatus.Cancelled]: 'icon-danger',
    [OrderStatus.Returned]: 'icon-returned'
  };

  return classes[status];
}
}
