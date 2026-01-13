import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, interval } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';

// Dashboards
import { ClientDashboardComponent } from '../dashboard/client-dashboard/client-dashboard.component';
import { OperatorDashboardComponent } from '../dashboard/operator-dashboard/operator-dashboard.component';

// Models
import { User } from '../../core/models/User';
import { Order } from '../../core/models/Order';
import { AppNotification } from '../../core/models/Notification';

// Services
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ClientDashboardComponent,
    OperatorDashboardComponent
  ],
  template: `
    <div class="dashboard-container" [class.operator-view]="isOperator">

      <header class="dashboard-header">
        <div class="header-left">
          <h1>TechsysLog</h1>
         <span class="user-role" [class.operator]="isOperator">
          <span
            class="material-icons"
            style="font-size: 18px; vertical-align: middle; margin-right: 4px;"
          >
            {{ isOperator ? 'badge' : 'person' }}
          </span>
          {{ isOperator ? 'Operador Logístico' : 'Cliente' }}
        </span>
        </div>

        <div class="header-right">

          <!-- Status de Conexão (Notificações) -->
          <div class="connection-status">
          <span
              class="status-indicator"
              [ngClass]="signalRConnected ? 'icon-success' : 'icon-warning'"
              title="{{ signalRConnected ? 'Conectado' : 'Desconectado' }}"
            >
              <span
                class="material-icons"
                style="font-size: 16px; vertical-align: middle; margin-right: 4px;"
              >
                {{ signalRConnected ? 'check_circle' : 'warning' }}
              </span>

              {{ signalRConnected ? 'ON: atualização automática' : 'OFF: atualização automática' }}
            </span>

          </div>

          <!-- Notificações -->
          <div class="notifications-wrapper">
            <button
              class="notification-btn btn"
              style="font-size:14px;"
              (click)="toggleNotificationsPanel()"
              [class.has-unread]="unreadCount > 0"
            >
              <span
                class="material-icons"
                style="font-size: 20px; vertical-align: middle; margin-right: 6px; color:#22c55e;"
              >
                notifications
              </span>
              
               {{ isOperator ? 'Notificações Enviadas' : 'Minhas Notificações' }}  

              <span *ngIf="unreadCount > 0 && !isOperator" class="badge">
                {{ unreadCount }}
              </span>
            </button>


                    
            <div class="notifications-panel" *ngIf="showNotificationsPanel">
              <div class="panel-header">
                <h3>Notificações</h3>

                <button
                  *ngIf="unreadCount > 0 && !isOperator"
                  class="btn-mark-all"
                  (click)="markAllNotificationsAsRead()"
                >
                  Marcar todas como lidas
                </button>
              </div>

              <div class="notifications-list">
                <div
                  *ngIf="notifications.length === 0"
                  class="empty-notifications"
                >
                  Nenhuma notificação
                </div>

                <div
                  *ngFor="let notification of notifications"
                  class="notification-item"
                  [class.unread]="!notification.isRead"
                  (click)="markNotificationAsRead(notification.id)"
                >
                  <h4>{{ notification.title }}</h4>
                  <p>{{ notification.message }}</p>
                  <small>
                    {{ notification.createdAt | date:'short' }}
                  </small>
                </div>
              </div>
            </div>
          </div>

          <!-- Usuário -->
          <div class="user-menu">
            <span class="user-name">
              {{ currentUser?.name }}
            </span>
            <button class="logout-btn" (click)="logout()">
              Sair
            </button>
          </div>

        </div>
      </header>

      <main class="dashboard-content">

        <div *ngIf="loading" class="loading-spinner">
           Carregando...
        </div>

        <ng-container *ngIf="!loading && !isOperator">
          <app-client-dashboard [orders]="orders" />
        </ng-container>

        <ng-container *ngIf="!loading && isOperator">
          <app-operator-dashboard (orderCreated)="loadOrders()" [orders]="orders" />
        </ng-container>

      </main>
    </div>
  `,
  styleUrls: ['./dashboard.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent implements OnInit, OnDestroy {

  currentUser: User | null = null;
  isOperator = false;
  loading = true;

  orders: Order[] = [];
  notifications: AppNotification[] = [];

  unreadCount = 0;
  showNotificationsPanel = false;
  signalRConnected = false;

  private destroy$ = new Subject<void>();
  private refreshOrders$ = new Subject<void>();

  // Intervalo de refresh automático (em ms)
  private readonly AUTO_REFRESH_INTERVAL = 30000; // 30 segundos

  constructor(
    private readonly authService: AuthService,
    private readonly orderService: OrderService,
    private readonly notificationService: NotificationService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {

    // Verifica autenticação
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    // Define usuário e role
    this.currentUser = this.authService.getCurrentUser();
    this.isOperator = this.authService.isOperator();

    // Inicia conexão SignalR
    const token = this.authService.getStoredAccessToken();
    if (token) {
      this.startSignalRConnection(token);
    }

    // Carrega dados iniciais
    this.loadOrders();
    this.loadNotifications();
    this.subscribeToNotifications();

    // Setup de refresh automático
    this.setupAutoRefresh();
  }

  ngOnDestroy(): void {
    this.notificationService.stopSignalRConnection();
    this.destroy$.next();
    this.destroy$.complete();
  }

  /* =============== ORDERS =============== */

  public loadOrders(): void {

    const source$ = this.isOperator
      ? this.orderService.getAllOrders()
      : this.orderService.getUserOrders();

    source$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.orders = this.orderService.getCachedOrders();
          this.loading = false;
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Setup de refresh automático de pedidos a cada 30 segundos
   */
  private setupAutoRefresh(): void {
    interval(this.AUTO_REFRESH_INTERVAL)
      .pipe(
        switchMap(() => {
          const source$ = this.isOperator
            ? this.orderService.getAllOrders()
            : this.orderService.getUserOrders();
          return source$;
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.orders = this.orderService.getCachedOrders();
          this.cdr.markForCheck();
        },
        error: (error) => {
        }
      });
  }

  /* =============== NOTIFICATIONS =============== */

  private loadNotifications(): void {

    this.notificationService.getUserNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.notifications =
            this.notificationService.getCachedNotifications();

          this.updateUnreadCount();
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.cdr.markForCheck();
        }
      });
  }

  private subscribeToNotifications(): void {

    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.notifications = data ?? [];
          this.updateUnreadCount();
          this.cdr.markForCheck();
        },
        error: (error) => {
        }
      });
  }

  private updateUnreadCount(): void {
    this.unreadCount = this.notifications.filter(n => !n.isRead).length;
    this.cdr.markForCheck();
  }

  toggleNotificationsPanel(): void {
    this.showNotificationsPanel = !this.showNotificationsPanel;
    this.cdr.markForCheck();
  }

  markNotificationAsRead(notificationId: string): void {
    this.notifications = this.notifications.map(n =>
      n.id === notificationId
        ? { ...n, isRead: true }
        : n
    );

    this.updateUnreadCount();

    // Chama API para persistir a leitura
    this.notificationService.markAsRead(notificationId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
        },
        error: (error) => {
        }
      });

    this.cdr.markForCheck();
  }

  markAllNotificationsAsRead(): void {
    this.notifications = this.notifications.map(n => ({
      ...n,
      isRead: true
    }));

    this.updateUnreadCount();

    // Chama API para persistir a leitura em massa
    this.notificationService.markAllAsRead()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
        },
        error: (error) => {
        }
      });

    this.cdr.markForCheck();
  }

  /* =============== SIGNALR CONNECTION =============== */

  private startSignalRConnection(token: string): void {

    this.notificationService.startSignalRConnection(token);

    // Monitora status da conexão
    this.notificationService.connectionStatus$
      ?.pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (isConnected) => {
          this.signalRConnected = isConnected;
          console.log(
            isConnected ? ' SignalR conectado' : ' SignalR desconectado'
          );
          this.cdr.markForCheck();
        },
        error: (error) => {
          this.signalRConnected = false;
          this.cdr.markForCheck();
        }
      });
  }

  /* =============== LOGOUT =============== */

  logout(): void {
    this.notificationService.stopSignalRConnection();

    this.authService.logout()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.router.navigate(['/auth/login']);
        },
        error: (error) => {
          this.router.navigate(['/auth/login']);
        }
      });
  }
}
