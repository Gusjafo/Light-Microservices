import { Component, OnInit } from '@angular/core';
import { LogEntry, LogService } from '../../services/log.service';

@Component({
  selector: 'app-logs-viewer',
  templateUrl: './logs-viewer.component.html'
})
export class LogsViewerComponent implements OnInit {
  levels: Array<'info' | 'warning' | 'error'> = ['info', 'warning', 'error'];
  selectedLevel: 'info' | 'warning' | 'error' = 'info';
  logs: LogEntry[] = [];
  isLoading = false;

  constructor(private readonly logService: LogService) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.logService.getLogs(this.selectedLevel).subscribe({
      next: logs => {
        this.logs = logs;
        this.isLoading = false;
      },
      error: () => {
        this.logs = [];
        this.isLoading = false;
      }
    });
  }
}
