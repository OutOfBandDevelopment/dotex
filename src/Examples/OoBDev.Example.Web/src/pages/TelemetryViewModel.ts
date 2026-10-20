import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface TelemetryTrace { traceId: string; name: string; started: string; durationMs: number; }
export interface TelemetryRecent { dashboardUrl: string; traces: TelemetryTrace[]; }

/** Link that opens one trace in the Aspire dashboard. */
export function traceUrl(dashboardUrl: string, traceId: string): string {
  return `${dashboardUrl.replace(/\/$/, '')}/traces/detail/${traceId}`;
}

export class TelemetryViewModel {
  readonly recent = ref<TelemetryRecent | null>(null);
  readonly refresh = new RefreshTracesCommand(this);
}

export class RefreshTracesCommand extends Command {
  constructor(private readonly vm: TelemetryViewModel) { super(); }
  protected isAllowed() { return true; }
  protected async run() {
    this.vm.recent.value = await http.getJson<TelemetryRecent>('/api/Telemetry/recent');
  }
}
