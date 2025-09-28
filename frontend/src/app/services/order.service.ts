import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface OrderItem {
  productId: number;
  quantity: number;
}

export interface Order {
  id: number;
  userId: number;
  status: string;
  totalAmount: number;
  createdAt?: string;
  items: OrderItem[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/orders`;

  constructor(private readonly http: HttpClient) {}

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  createOrder(order: { userId: number; items: OrderItem[] }): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, order);
  }
}
