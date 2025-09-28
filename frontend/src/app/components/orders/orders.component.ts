import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Order, OrderService } from '../../services/order.service';
import { Product, ProductService } from '../../services/product.service';
import { User, UserService } from '../../services/user.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html'
})
export class OrdersComponent implements OnInit {
  orderForm: FormGroup;
  orders: Order[] = [];
  users: User[] = [];
  products: Product[] = [];
  displayedColumns = ['id', 'userId', 'productId', 'quantity', 'createdAtUtc'];
  isLoadingOrders = false;
  isSubmitting = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly orderService: OrderService,
    private readonly userService: UserService,
    private readonly productService: ProductService,
    private readonly snackBar: MatSnackBar
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
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe(users => (this.users = users));
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe(products => (this.products = products));
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

    this.orderService.createOrder({ userId, productId, quantity }).subscribe({
      next: () => {
        this.snackBar.open('Pedido creado', 'Cerrar', { duration: 2000 });
        this.orderForm.reset({ userId: null, productId: null, quantity: 1 });
        this.loadOrders();
        this.isSubmitting = false;
      },
      error: () => {
        this.snackBar.open('Error creando pedido', 'Cerrar', { duration: 3000 });
        this.isSubmitting = false;
      }
    });
  }

}
