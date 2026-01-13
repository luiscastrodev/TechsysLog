import {
  Component,
  Input,
  OnInit,
  OnChanges,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Order } from '../../../core/models/Order';
import { OrderStatus } from '../../../core/models/OrderStatus';

@Component({
  selector: 'app-client-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './client-dashboard.component.html',
  styleUrls: ['./client-dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientDashboardComponent implements OnInit, OnChanges {

  @Input() orders: Order[] = [];

  filteredOrders: Order[] = [];
  selectedOrder: Order | null = null;
  showOrderDetails = false;

  statusFilter: OrderStatus | 'All' = 'All';
  searchFilter = '';

  OrderStatus = OrderStatus;

  statusOptions: { value: OrderStatus | 'All'; label: string }[] = [
    { value: 'All', label: 'Todos os Status' },
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

  constructor(
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.updateStats();
    this.applyFilters();
  }

  ngOnChanges(): void {
    this.updateStats();
    this.applyFilters();
    this.cdr.markForCheck();
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

  viewOrderDetails(order: Order): void {
    this.selectedOrder = order;
    this.showOrderDetails = true;
    this.cdr.markForCheck();
  }

  closeOrderDetails(): void {
    this.showOrderDetails = false;
    this.selectedOrder = null;
    this.cdr.markForCheck();
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

  getStatusIcon(status: OrderStatus | number): string {
    const icons: { [key: number]: string } = {
      [OrderStatus.Pending]: 'hourglass_empty',
      [OrderStatus.Processing]: 'settings',
      [OrderStatus.Shipped]: 'local_shipping',
      [OrderStatus.Delivered]: 'check_circle',
      [OrderStatus.Cancelled]: 'cancel',
      [OrderStatus.Returned]: 'undo'
    };
    return icons[status as number] || 'help';
  }
}
