import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface QueueChannel {
  channel: string;
  provider: string;
  queueName?: string;
  hostName?: string;
  receiverEnabled: boolean;
}

export interface ReceivedMessage {
  receivedAt: string;
  correlationId?: string;
  message: string;
}

export class QueuesViewModel {
  readonly channels = ref<QueueChannel[]>([]);
  readonly received = ref<ReceivedMessage[]>([]);
  readonly refreshChannels = new RefreshChannelsCommand(this);
  readonly refreshReceived = new RefreshReceivedCommand(this);
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
    const id = await http.postText(`/api/MessageQueueing/${this.vm.queue.value}`, {
      input: this.vm.input.value,
      data: JSON.parse(this.vm.data.value || 'null'),
    });
    this.vm.result.value = `Queued${id ? ` (message ${id})` : ''}`;
  }
}

export class RefreshChannelsCommand extends Command {
  constructor(private readonly vm: QueuesViewModel) { super(); }
  protected async run() {
    this.vm.channels.value = await http.getJson<QueueChannel[]>('/api/MessageQueueing/channels');
  }
}

export class RefreshReceivedCommand extends Command {
  constructor(private readonly vm: QueuesViewModel) { super(); }
  protected async run() {
    this.vm.received.value = await http.getJson<ReceivedMessage[]>('/api/MessageQueueing/received');
  }
}
