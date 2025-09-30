import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EventsViewerComponent } from './components/events-viewer/events-viewer.component';
import { LoginComponent } from './components/login/login.component';
import { LogsViewerComponent } from './components/logs-viewer/logs-viewer.component';
import { OrdersComponent } from './components/orders/orders.component';
import { ProductsComponent } from './components/products/products.component';
import { UsersComponent } from './components/users/users.component';
import { AuthGuard } from './services/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', redirectTo: 'users', pathMatch: 'full' },
  { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
  { path: 'products', component: ProductsComponent, canActivate: [AuthGuard] },
  { path: 'orders', component: OrdersComponent, canActivate: [AuthGuard] },
  { path: 'logs', component: LogsViewerComponent, canActivate: [AuthGuard] },
  { path: 'events', component: EventsViewerComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: 'users' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
