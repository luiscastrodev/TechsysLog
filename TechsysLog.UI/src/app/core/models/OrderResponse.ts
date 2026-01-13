import { Order } from "./Order";

export interface OrderResponse {
  isSuccess: boolean;
  data: Order;
  message?: string;
}
