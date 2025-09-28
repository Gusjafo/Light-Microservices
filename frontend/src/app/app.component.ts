import { Component } from '@angular/core';

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
export class AppComponent {
  title = 'Light Microservices';
  navItems: NavItem[] = [
    { label: 'Usuarios', icon: 'people', route: '/users' },
    { label: 'Productos', icon: 'inventory', route: '/products' },
    { label: 'Pedidos', icon: 'shopping_cart', route: '/orders' },
    { label: 'Logs', icon: 'list_alt', route: '/logs' },
    { label: 'Eventos', icon: 'bolt', route: '/events' }
  ];
}
