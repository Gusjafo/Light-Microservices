import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface LogEntry {
  timestamp: string;
  level: 'info' | 'warning' | 'error';
  message: string;
  context?: string;
}

@Injectable({ providedIn: 'root' })
export class LogService {
  private readonly baseUrl = `${environment.apiUrls.eventHub}/api/logs`;

  constructor(private readonly http: HttpClient) {}

  getLogs(level: 'info' | 'warning' | 'error'): Observable<LogEntry[]> {
    return this.http
      .get(`${this.baseUrl}/${level}`, { responseType: 'text' })
      .pipe(map(response => this.parseLogs(response, level)));
  }

  private parseLogs(rawLogs: string, fallbackLevel: 'info' | 'warning' | 'error'): LogEntry[] {
    return rawLogs
      .split(/\r?\n/)
      .map(line => line.trim())
      .filter(line => !!line)
      .map(line => this.parseLine(line, fallbackLevel))
      .filter((entry): entry is LogEntry => !!entry);
  }

  private parseLine(line: string, fallbackLevel: 'info' | 'warning' | 'error'): LogEntry | null {
    const logRegex = /^(\d{4}-\d{2}-\d{2}) (\d{2}:\d{2}:\d{2}\.\d{3}) ([+-]\d{2}:\d{2}) \[(\w+)\] (.*)$/;
    const match = line.match(logRegex);

    if (!match) {
      return {
        timestamp: new Date().toISOString(),
        level: fallbackLevel,
        message: line
      };
    }

    const [, datePart, timePart, offset, levelCode, rest] = match;
    const isoTimestamp = `${datePart}T${timePart}${offset}`;
    const level = this.mapLevel(levelCode, fallbackLevel);
    const { message, context } = this.extractContext(rest);

    return {
      timestamp: isoTimestamp,
      level,
      message,
      context: context || undefined
    };
  }

  private mapLevel(levelCode: string, fallbackLevel: 'info' | 'warning' | 'error'): 'info' | 'warning' | 'error' {
    switch (levelCode.toUpperCase()) {
      case 'INF':
      case 'INFO':
        return 'info';
      case 'WRN':
      case 'WARN':
        return 'warning';
      case 'ERR':
      case 'FTL':
      case 'CRIT':
        return 'error';
      default:
        return fallbackLevel;
    }
  }

  private extractContext(rawMessage: string): { message: string; context: string | null } {
    const separator = ' - ';
    const lastSeparatorIndex = rawMessage.lastIndexOf(separator);

    if (lastSeparatorIndex === -1) {
      return { message: rawMessage.trim(), context: null };
    }

    const message = rawMessage.slice(0, lastSeparatorIndex).trim();
    const context = rawMessage.slice(lastSeparatorIndex + separator.length).trim();

    return {
      message,
      context: context.length ? context : null
    };
  }
}
