import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface ScheduleEntry { key: string; dueAt: string; message: string; firedAt: string | null; }
export interface ScheduleState { pending: ScheduleEntry[]; fired: ScheduleEntry[]; }

export class ScheduleViewModel {
  readonly key = ref('reminder-1');
  readonly inSeconds = ref(10);
  readonly message = ref('Stand up and stretch');
  readonly state = ref<ScheduleState>({ pending: [], fired: [] });
  readonly refresh = new RefreshScheduleCommand(this);
  readonly add = new AddScheduleCommand(this);
  readonly release = new ReleaseScheduleCommand(this);
}

export class RefreshScheduleCommand extends Command {
  constructor(private readonly vm: ScheduleViewModel) { super(); }
  protected async run() {
    this.vm.state.value = await http.getJson<ScheduleState>('/api/Schedule');
  }
}

export class AddScheduleCommand extends Command {
  constructor(private readonly vm: ScheduleViewModel) { super(); }
  protected isAllowed() { return this.vm.key.value.trim() !== '' && this.vm.inSeconds.value >= 0; }
  protected async run() {
    this.vm.state.value = await http.postJson<ScheduleState>('/api/Schedule', {
      key: this.vm.key.value, inSeconds: Number(this.vm.inSeconds.value), message: this.vm.message.value,
    });
  }
}

export class ReleaseScheduleCommand extends Command<string> {
  constructor(private readonly vm: ScheduleViewModel) { super(); }
  protected async run(key: string) {
    await http.sendJson('DELETE', `/api/Schedule/${encodeURIComponent(key)}`);
    this.vm.state.value = await http.getJson<ScheduleState>('/api/Schedule');
  }
}
