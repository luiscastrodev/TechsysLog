import { Address } from './Address';

export interface CreateOrderRequest {
  description: string;
  amount: number;
  address: Address;
  userId?: string;
}
