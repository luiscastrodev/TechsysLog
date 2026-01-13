import { OrderStatus } from "./OrderStatus";

export interface OrderHistory {
  id: string;
  previousStatus: OrderStatus;
  previousStatusDescription: OrderStatus;

  newStatus: OrderStatus;
    newStatustatusDescription: OrderStatus;

  createdAt: string;
  changedByUserId: string;
  reason?: string;
}
