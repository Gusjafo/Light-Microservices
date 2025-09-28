import { Injectable, NgZone } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface IntegrationEvent {
  eventType: string;
  payload: unknown;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class EventService {
  private hubConnection?: HubConnection;
  private readonly eventsSubject = new BehaviorSubject<IntegrationEvent[]>([]);
  private readonly isConnectedSubject = new BehaviorSubject<boolean>(false);

  readonly events$: Observable<IntegrationEvent[]> = this.eventsSubject.asObservable();
  readonly isConnected$: Observable<boolean> = this.isConnectedSubject.asObservable();

  constructor(private readonly zone: NgZone) {}

  connect(): void {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.on('BroadcastEvent', (eventType: string, payload: unknown, createdAt: string) => {
      this.zone.run(() => {
        const current = this.eventsSubject.value;
        this.eventsSubject.next([{ eventType, payload, createdAt }, ...current]);
      });
    });

    this.hubConnection.onreconnected(() => this.zone.run(() => this.isConnectedSubject.next(true)));
    this.hubConnection.onreconnecting(() => this.zone.run(() => this.isConnectedSubject.next(false)));
    this.hubConnection.onclose(() => this.zone.run(() => this.isConnectedSubject.next(false)));

    this.hubConnection
      .start()
      .then(() => this.zone.run(() => this.isConnectedSubject.next(true)))
      .catch(error => {
        console.error('Error connecting to SignalR hub', error);
        this.zone.run(() => this.isConnectedSubject.next(false));
      });
  }

  disconnect(): void {
    if (!this.hubConnection) {
      return;
    }

    this.hubConnection.stop().finally(() => {
      this.zone.run(() => this.isConnectedSubject.next(false));
      this.hubConnection = undefined;
    });
  }

  clearEvents(): void {
    this.eventsSubject.next([]);
  }
}
