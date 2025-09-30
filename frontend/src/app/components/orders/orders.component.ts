import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { IntegrationEvent, EventService } from '../../services/event.service';
import { Order, OrderService } from '../../services/order.service';
import { Product, ProductService } from '../../services/product.service';
import { User, UserService } from '../../services/user.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit, OnDestroy {
  orderForm: FormGroup;
  orders: Order[] = [];
  users: User[] = [];
  products: Product[] = [];
  private userNameById = new Map<string, string>();
  private productNameById = new Map<string, string>();
  private readonly subscriptions: Subscription[] = [];
  private readonly handledEventIds = new Set<string>();
  private isFirstEventsSnapshot = true;
  displayedColumns = ['id', 'user', 'product', 'quantity', 'createdAtUtc'];
  isLoadingOrders = false;
  isSubmitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly orderService: OrderService,
    private readonly userService: UserService,
    private readonly productService: ProductService,
    private readonly snackBar: MatSnackBar,
    private readonly eventService: EventService
  ) {
    this.orderForm = this.fb.group({
      userId: [null as string | null, Validators.required],
      productId: [null as string | null, Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadProducts();
    this.loadOrders();

    this.subscriptions.push(
      this.eventService.events$.subscribe(events => {
        if (this.isFirstEventsSnapshot) {
          for (const existingEvent of events) {
            const existingId = this.getEventId(existingEvent);
            if (existingId) {
              this.handledEventIds.add(existingId);
            }
          }
          this.isFirstEventsSnapshot = false;
          return;
        }

        const latest = events[0];
        if (!latest) {
          return;
        }

        const eventId = this.getEventId(latest);
        if (eventId && this.handledEventIds.has(eventId)) {
          return;
        }

        if (eventId) {
          this.handledEventIds.add(eventId);
        }

        this.handleIntegrationEvent(latest);
      })
    );
  }

  ngOnDestroy(): void {
    for (const subscription of this.subscriptions) {
      subscription.unsubscribe();
    }
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe(users => {
      this.users = users;
      this.userNameById.clear();
      for (const user of users) {
        this.userNameById.set(user.id, user.name);
      }
    });
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe(products => {
      this.products = products;
      this.productNameById.clear();
      for (const product of products) {
        this.productNameById.set(product.id, product.name);
      }
    });
  }

  loadOrders(): void {
    this.isLoadingOrders = true;
    this.orderService.getOrders().subscribe({
      next: orders => {
        this.orders = orders;
        this.isLoadingOrders = false;
      },
      error: () => {
        this.snackBar.open('Error cargando pedidos', 'Cerrar', { duration: 3000 });
        this.isLoadingOrders = false;
      }
    });
  }

  submit(): void {
    if (this.orderForm.invalid) {
      this.orderForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const { userId, productId, quantity } = this.orderForm.value as {
      userId: string;
      productId: string;
      quantity: number;
    };

    const creatingSnack = this.snackBar.open('Creando pedido...', undefined, { duration: 0 });

    this.orderService.createOrder({ userId, productId, quantity }).subscribe({
      next: order => {
        creatingSnack.dismiss();
        this.snackBar.open(`Pedido ${order.id} registrado. Procesando stock...`, 'Cerrar', {
          duration: 4000
        });
        this.orderForm.reset({ userId: null, productId: null, quantity: 1 });
        this.loadOrders();
        this.isSubmitting = false;
      },
      error: () => {
        creatingSnack.dismiss();
        this.snackBar.open('Error creando pedido', 'Cerrar', { duration: 3000 });
        this.isSubmitting = false;
      }
    });
  }

  getUserName(order: Order): string {
    return this.userNameById.get(order.userId) ?? order.userId;
  }

  getProductName(order: Order): string {
    return this.productNameById.get(order.productId) ?? order.productId;
  }

  private getEventId(event: IntegrationEvent): string | null {
    if (!event?.payload || typeof event.payload !== 'object') {
      return null;
    }

    const payload = event.payload as { eventId?: string };
    const eventId = payload.eventId;
    return typeof eventId === 'string' ? eventId : null;
  }

  private handleIntegrationEvent(event: IntegrationEvent): void {
    switch (event.eventType) {
      case 'OrderCreatedEvent': {
        const payload = event.payload as {
          id?: string;
        };
        const orderId = payload.id ?? 'desconocido';
        this.snackBar.open(
          `Orden ${orderId} creada. Solicitando actualización de stock...`,
          'Cerrar',
          { duration: 4000 }
        );
        break;
      }
      case 'StockDecreasedEvent': {
        const payload = event.payload as {
          productId?: string;
          remainingStock?: number;
        };
        this.snackBar.open(
          `Stock actualizado para el producto ${payload.productId ?? 'desconocido'}. ` +
            `Restante: ${payload.remainingStock ?? 'N/D'}.`,
          'Cerrar',
          { duration: 5000 }
        );
        break;
      }
      case 'StockDecreaseFailedEvent': {
        const payload = event.payload as {
          productId?: string;
          reason?: string;
        };
        this.snackBar.open(
          `No se pudo actualizar el stock del producto ${payload.productId ?? 'desconocido'}: ` +
            `${payload.reason ?? 'motivo no disponible'}.`,
          'Cerrar',
          { duration: 6000 }
        );
        break;
      }
      default:
        break;
    }
  }
}
