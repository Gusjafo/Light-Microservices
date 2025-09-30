import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from './services/auth.service';
import { EventService } from './services/event.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Light Microservices';
  navItems: NavItem[] = [
    { label: 'Usuarios', icon: 'people', route: '/users' },
    { label: 'Productos', icon: 'inventory', route: '/products' },
    { label: 'Pedidos', icon: 'shopping_cart', route: '/orders' },
    { label: 'Logs', icon: 'list_alt', route: '/logs' },
    { label: 'Eventos', icon: 'bolt', route: '/events' }
  ];
  isAuthenticated = false;
  userEmail = '';
  userRoles: string[] = [];
  private readonly subscriptions = new Subscription();

  constructor(
    private readonly eventService: EventService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const authSubscription = this.authService.authState$.subscribe(state => {
      this.isAuthenticated = !!state;
      this.userEmail = state?.email ?? '';
      this.userRoles = state?.roles ?? [];

      if (state) {
        this.eventService.connect();
      } else {
        this.eventService.disconnect();
        this.eventService.clearEvents();
      }
    });

    this.subscriptions.add(authSubscription);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.eventService.disconnect();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
