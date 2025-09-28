import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LogEntry {
  timestamp: string;
  level: 'info' | 'warning' | 'error';
  message: string;
  context?: string;
}

@Injectable({ providedIn: 'root' })
export class LogService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/logs`;

  constructor(private readonly http: HttpClient) {}

  getLogs(level: 'info' | 'warning' | 'error'): Observable<LogEntry[]> {
    return this.http.get<LogEntry[]>(`${this.baseUrl}/${level}`);
  }
}
