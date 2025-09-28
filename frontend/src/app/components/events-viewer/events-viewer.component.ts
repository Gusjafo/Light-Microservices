import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventService, IntegrationEvent } from '../../services/event.service';

@Component({
  selector: 'app-events-viewer',
  templateUrl: './events-viewer.component.html'
})
export class EventsViewerComponent implements OnInit, OnDestroy {
  events: IntegrationEvent[] = [];
  isConnected = false;
  private subscriptions = new Subscription();

  constructor(private readonly eventService: EventService) {}

  ngOnInit(): void {
    this.eventService.connect();
    this.subscriptions.add(this.eventService.events$.subscribe(events => (this.events = events)));
    this.subscriptions.add(this.eventService.isConnected$.subscribe(state => (this.isConnected = state)));
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.eventService.disconnect();
  }

  clear(): void {
    this.eventService.clearEvents();
  }
}
