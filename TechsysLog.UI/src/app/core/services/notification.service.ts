import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { API_CONFIG } from '../config/api.config';
import { NotificationResponse } from '../models/NotificationResponse';
import { AppNotification } from '../models/Notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly apiUrl = API_CONFIG.baseUrl + '/notifications';
  private readonly hubUrl = API_CONFIG.hubbaseUrl + '/hubs/notifications';

  private hubConnection: HubConnection | null = null;

  // Subjects para notificações em tempo real
  private readonly notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private readonly unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  // Subject para status da conexão SignalR
  private readonly connectionStatusSubject = new BehaviorSubject<boolean>(false);
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  // Subjects para eventos em tempo real
  private readonly orderStatusChangedSubject = new BehaviorSubject<any>(null);
  public orderStatusChanged$ = this.orderStatusChangedSubject.asObservable();

  private readonly orderDeliveredSubject = new BehaviorSubject<any>(null);
  public orderDelivered$ = this.orderDeliveredSubject.asObservable();

  private readonly newOrderSubject = new BehaviorSubject<any>(null);
  public newOrder$ = this.newOrderSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  /**
   * Inicializa conexão SignalR
   */
  startSignalRConnection(accessToken: string): Promise<void> {
    return new Promise((resolve, reject) => {
      // Não criar múltiplas conexões
      if (this.hubConnection && this.hubConnection.state === HubConnectionState.Connected) {
        this.connectionStatusSubject.next(true);
        resolve();
        return;
      }

      this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => accessToken
        })
        .withAutomaticReconnect()
        .build();

      // Listeners para eventos em tempo real
      this.hubConnection.on('ReceiveNotification', (notification) => {
        console.log('Notificação recebida:', notification);
        this.addNotification(notification);
        this.loadUnreadCount();
      });

      this.hubConnection.on('OrderStatusChanged', (data) => {
        console.log('Status do pedido alterado:', data);
        this.orderStatusChangedSubject.next(data);
        this.loadUnreadCount();
      });

      this.hubConnection.on('OrderDelivered', (data) => {
        console.log('Pedido entregue:', data);
        this.orderDeliveredSubject.next(data);
        this.loadUnreadCount();
      });

      this.hubConnection.on('NewOrderCreated', (data) => {
        console.log('Novo pedido criado:', data);
        this.newOrderSubject.next(data);
        this.loadUnreadCount();
      });

      // Monitora eventos de reconexão
      this.hubConnection.onreconnecting(() => {
        console.log('Tentando reconectar ao SignalR...');
        this.connectionStatusSubject.next(false);
      });

      this.hubConnection.onreconnected(() => {
        console.log('Reconectado ao SignalR');
        this.connectionStatusSubject.next(true);
      });

      this.hubConnection.onclose(() => {
        console.log('Conexão SignalR fechada');
        this.connectionStatusSubject.next(false);
      });

      this.hubConnection.start()
        .then(() => {
          console.log('Conexão SignalR estabelecida');
          this.connectionStatusSubject.next(true);
          resolve();
        })
        .catch(err => {
          console.error('Erro ao conectar SignalR:', err);
          this.connectionStatusSubject.next(false);
          reject(err);
        });
    });
  }

  /**
   * Encerra conexão SignalR
   */
  stopSignalRConnection(): Promise<void> {
    if (this.hubConnection) {
      this.connectionStatusSubject.next(false);
      return this.hubConnection.stop();
    }
    return Promise.resolve();
  }

  /**
   * Obtém notificações do usuário
   */
  getUserNotifications(): Observable<any> {
    return this.http.get<NotificationResponse>(this.apiUrl).pipe(
      tap(response => {
        if (response.isSuccess && Array.isArray(response.data)) {
          this.notificationsSubject.next(response.data);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Obtém contagem de notificações não lidas
   */
  getUnreadCount(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/unread-count`).pipe(
      tap(response => {
        if (response.success) {
          this.unreadCountSubject.next(response.data);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Carrega contagem de não lidas (sem retornar Observable)
   */
  loadUnreadCount(): void {
    this.http.get<any>(`${this.apiUrl}/unread-count`).pipe(
      tap(response => {
        if (response.success) {
          this.unreadCountSubject.next(response.data);
        }
      }),
      catchError(() => {
        // Silenciar erro de contagem
        return new Observable(observer => observer.complete());
      })
    ).subscribe();
  }

  /**
   * Marca notificação como lida
   */
  markAsRead(notificationId: string): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/${notificationId}/read`,
      {}
    ).pipe(
      tap(() => {
        this.loadUnreadCount();
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Marca todas as notificações como lidas
   */
  markAllAsRead(): Observable<any> {
    return this.http.patch(
      `${this.apiUrl}/read-all`,
      {}
    ).pipe(
      tap(() => {
        const notifications = this.notificationsSubject.value;
        const updated = notifications.map(n => ({ ...n, isRead: true }));
        this.notificationsSubject.next(updated);
        this.loadUnreadCount();
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Adiciona notificação ao cache local
   */
  private addNotification(notification: any): void {
    const current = this.notificationsSubject.value;
    this.notificationsSubject.next([notification, ...current]);
  }

  /**
   * Trata erros HTTP
   */
  private handleError(error: any) {
    console.error('Notification service error:', error);
    return throwError(() =>
      new Error(error.error?.message || 'Erro ao processar notificação')
    );
  }

  /**
   * Obtém notificações do cache
   */
  getCachedNotifications(): AppNotification[] {
    return this.notificationsSubject.value;
  }

  /**
   * Obtém contagem de não lidas do cache
   */
  getCachedUnreadCount(): number {
    return this.unreadCountSubject.value;
  }

  /**
   * Obtém status da conexão SignalR
   */
  isSignalRConnected(): boolean {
    return this.connectionStatusSubject.value;
  }
}