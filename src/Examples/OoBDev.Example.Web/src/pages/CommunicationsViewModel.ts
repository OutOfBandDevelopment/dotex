import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface InboxMessage { messageId: string; from?: string | null; to: string[]; subject: string; preview: string; timestamp: string }

export type Channel = 'public' | 'queued' | 'private';

export class CommunicationsViewModel {
  readonly channel = ref<Channel>('public');
  readonly to = ref('someone@example.test');
  readonly subject = ref('Hello');
  readonly body = ref('Message text');
  readonly result = ref('');
  readonly send = new SendEmailCommand(this);
  readonly inbox = ref<InboxMessage[]>([]);
  readonly polled = ref(false);
  readonly poll = new PollInboxCommand(this);
}

export class SendEmailCommand extends Command {
  constructor(private readonly vm: CommunicationsViewModel) { super(); }
  protected isAllowed() { return this.vm.to.value.trim() !== ''; }
  protected async run() {
    this.vm.result.value = await http.postText(`/api/Communications/${this.vm.channel.value}`, {
      referenceId: crypto.randomUUID(),
      toAddresses: [this.vm.to.value],
      subject: this.vm.subject.value,
      textContent: this.vm.body.value,
    });
  }
}

export class PollInboxCommand extends Command {
  constructor(private readonly vm: CommunicationsViewModel) { super(); }
  protected async run() {
    const fetched = await http.postJson<InboxMessage[]>('/api/Communications/inbox/poll?max=10', {});
    this.vm.inbox.value = [...fetched, ...this.vm.inbox.value];
    this.vm.polled.value = true;
  }
}
