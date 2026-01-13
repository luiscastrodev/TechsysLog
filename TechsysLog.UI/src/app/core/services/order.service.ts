
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';

import { API_CONFIG } from '../config/api.config';
import { OrderResponse } from '../models/OrderResponse';
import { CreateOrderRequest } from '../models/CreateOrderRequest';
import { Order } from '../models/Order';
import { DeliveryRequest } from '../models/DeliveryRequest';
import { OrderStatus } from '../models/OrderStatus';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly apiUrl = API_CONFIG.baseUrl + '/orders';

  private readonly ordersSubject = new BehaviorSubject<Order[]>([]);
  public orders$ = this.ordersSubject.asObservable();

  constructor(private readonly http: HttpClient) {}

  /**
   * Cria novo pedido
   */
  createOrder(order: CreateOrderRequest): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(
      this.apiUrl,
      order
    ).pipe(
      tap(response => {
        if (response.isSuccess) {
          const currentOrders = this.ordersSubject.value;
          this.ordersSubject.next([...currentOrders, response.data]);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Obtém pedidos do usuário logado
   */
  getUserOrders(): Observable<any> {
    return this.http.get<any>(this.apiUrl).pipe(
      tap(response => {
        if (response.isSuccess && Array.isArray(response.data)) {
          this.ordersSubject.next(response.data);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Obtém todos os pedidos (apenas para operadores)
   */
  getAllOrders(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/all`).pipe(
      tap(response => {
        if (response.isSuccess && Array.isArray(response.data)) {
          this.ordersSubject.next(response.data);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Obtém pedido por número
   */
  getOrderByNumber(orderNumber: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${orderNumber}`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Altera status do pedido
   */
  changeOrderStatus(
    orderNumber: string,
    newStatus: OrderStatus,
    reason?: string
  ): Observable<OrderResponse> {


    return this.http.patch<OrderResponse>(
      `${this.apiUrl}/${orderNumber}/status`,
      {newStatus, reason }
    ).pipe(
      tap(response => {
        if (response.isSuccess) {
          this.updateOrderInCache(response.data);
        }
      }),
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Registra entrega de pedido
   */
  registerDelivery(delivery: DeliveryRequest): Observable<any> {
    return this.http.post(
      API_CONFIG.baseUrl + '/delivery/register',
      delivery
    ).pipe(
      catchError(error => this.handleError(error))
    );
  }

  /**
   * Atualiza pedido no cache local
   */
  private updateOrderInCache(updatedOrder: Order): void {
    const currentOrders = this.ordersSubject.value;
    const index = currentOrders.findIndex(o => o.id === updatedOrder.id);

    if (index > -1) {
      currentOrders[index] = updatedOrder;
      this.ordersSubject.next([...currentOrders]);
    }
  }

  /**
   * Trata erros HTTP
   */
  private handleError(error: any) {
    console.error('Order service error:', error);
    return throwError(() =>
      new Error(error.error?.message || 'Erro ao processar pedido')
    );
  }

  /**
   * Obtém pedidos do cache local
   */
  getCachedOrders(): Order[] {
    return this.ordersSubject.value;
  }
}
