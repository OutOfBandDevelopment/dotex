import { ref } from 'vue';
import { Command } from '../core/command';

export class WebhooksViewModel {
  readonly channel = ref('email');
  readonly body = ref('{ "event": "delivered" }');
  readonly status = ref('');
  readonly result = ref('');
  readonly send = new SendWebhookCommand(this);
}

export class SendWebhookCommand extends Command {
  constructor(private readonly vm: WebhooksViewModel) { super(); }
  protected isAllowed() { return this.vm.channel.value.trim() !== ''; }
  protected async run() {
    const response = await fetch(`/webhooks/${encodeURIComponent(this.vm.channel.value.trim())}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: this.vm.body.value,
    });
    this.vm.status.value = `${response.status} ${response.statusText}`.trim();
    this.vm.result.value = await response.text();
  }
}
