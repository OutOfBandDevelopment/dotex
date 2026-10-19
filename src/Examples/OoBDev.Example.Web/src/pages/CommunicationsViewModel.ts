import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export type Channel = 'public' | 'queued' | 'private';

export class CommunicationsViewModel {
  readonly channel = ref<Channel>('public');
  readonly to = ref('someone@example.test');
  readonly subject = ref('Hello');
  readonly body = ref('Message text');
  readonly result = ref('');
  readonly send = new SendEmailCommand(this);
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
