import { computed, ref } from 'vue';
import { Command } from '../core/command';
import { ApiError, http } from '../core/http';

export interface RightAuditRow { at: string; subject: string; rights: string; result: string; }

/** The query string for the audit endpoint; empty fields are left out. */
export function auditQuery(subject: string, result: string, maxCount: number): string {
  const q = new URLSearchParams();
  if (subject.trim() !== '') q.set('subject', subject.trim());
  if (result.trim() !== '') q.set('result', result.trim());
  q.set('maxCount', String(maxCount));
  return q.toString();
}

/** Whether a failed call means the host has no durable audit trail (the endpoint is only mapped when a connection string is set). */
export const isNotConfigured = (error: unknown): boolean => error instanceof ApiError && error.status === 404;

export class RightAuditViewModel {
  readonly rows = ref<RightAuditRow[]>([]);
  readonly subject = ref('');
  readonly result = ref('');
  readonly maxCount = ref(100);
  readonly notConfigured = ref(false);
  readonly summary = computed(() => `${this.rows.value.length} entries, newest first`);
  readonly refresh = new RefreshAuditCommand(this);
}

export class RefreshAuditCommand extends Command {
  constructor(private readonly vm: RightAuditViewModel) { super(); }
  protected async run() {
    try {
      this.vm.rows.value = await http.getJson<RightAuditRow[]>(`/api/v1/right-audit?${auditQuery(this.vm.subject.value, this.vm.result.value, this.vm.maxCount.value)}`);
      this.vm.notConfigured.value = false;
    } catch (error) {
      if (!isNotConfigured(error)) throw error;
      this.vm.rows.value = [];
      this.vm.notConfigured.value = true;
    }
  }
}
