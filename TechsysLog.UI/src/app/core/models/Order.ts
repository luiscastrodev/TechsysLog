import { Address } from "./Address";
import { OrderHistory } from "./OrderHistory";
import { OrderStatus } from "./OrderStatus";

export interface Order {
  id: string;
  orderNumber: string;
  userId: string;
  description: string;
  clientName: string;
  amount: number;
  status: OrderStatus;
  statusDescription: string;
  shippingAddress: Address;
  createdAt: string;
  updatedAt: string;
  history?: OrderHistory[];
}
