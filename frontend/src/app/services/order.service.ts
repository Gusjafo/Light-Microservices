import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Order {
  id: string;
  userId: string;
  productId: string;
  quantity: number;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly baseUrl = `${environment.apiUrls.order}/api/orders`;

  constructor(private readonly http: HttpClient) {}

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  createOrder(order: { userId: string; productId: string; quantity: number }): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, order);
  }
}
