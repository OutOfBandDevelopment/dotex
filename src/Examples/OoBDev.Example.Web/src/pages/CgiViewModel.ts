import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface CgiProgram {
  name: string;
  path: string;
}

export class CgiViewModel {
  readonly programs = ref<CgiProgram[]>([]);
  readonly selected = ref('');
  readonly name = ref('Ada');
  readonly status = ref('');
  readonly body = ref('');
  readonly load = new LoadProgramsCommand(this);
  readonly open = new OpenProgramCommand(this);
  readonly post = new PostNameCommand(this);
}

export class LoadProgramsCommand extends Command {
  constructor(private readonly vm: CgiViewModel) { super(); }
  protected async run() {
    this.vm.programs.value = await http.getJson<CgiProgram[]>('/api/CgiPrograms');
    if (!this.vm.selected.value && this.vm.programs.value.length > 0) this.vm.selected.value = this.vm.programs.value[0].path;
  }
}

async function show(vm: CgiViewModel, response: Response) {
  vm.status.value = `${response.status} ${response.statusText}`.trim();
  vm.body.value = await response.text();
}

export class OpenProgramCommand extends Command {
  constructor(private readonly vm: CgiViewModel) { super(); }
  protected isAllowed() { return this.vm.selected.value !== ''; }
  protected async run() {
    await show(this.vm, await fetch(`${this.vm.selected.value}?name=${encodeURIComponent(this.vm.name.value)}`));
  }
}

export class PostNameCommand extends Command {
  constructor(private readonly vm: CgiViewModel) { super(); }
  protected isAllowed() { return this.vm.selected.value !== ''; }
  protected async run() {
    const form = new URLSearchParams({ name: this.vm.name.value });
    await show(this.vm, await fetch(this.vm.selected.value, { method: 'POST', body: form }));
  }
}
