import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export class QueuesViewModel {
  readonly queue = ref<'public' | 'private'>('public');
  readonly input = ref('hello');
  readonly data = ref('{ "text": "hello" }');
  readonly result = ref('');
  readonly enqueue = new EnqueueCommand(this);
}

export class EnqueueCommand extends Command {
  constructor(private readonly vm: QueuesViewModel) { super(); }
  protected isAllowed() {
    try { JSON.parse(this.vm.data.value || 'null'); return true; } catch { return false; }
  }
  protected async run() {
    this.vm.result.value = await http.postText(`/api/MessageQueueing/${this.vm.queue.value}`, {
      input: this.vm.input.value,
      data: JSON.parse(this.vm.data.value || 'null'),
    });
  }
}
