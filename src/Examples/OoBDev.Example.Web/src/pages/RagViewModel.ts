import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface Passage { id: string; text: string; }
export interface RagSource { id: string; score: number; text: string; }
export interface RagResult { answer: string; sources: RagSource[]; }

/** Passages are separated by a blank line and named p1, p2, ... in order. */
export function parsePassages(text: string): Passage[] {
  return text.split(/\r?\n\s*\r?\n/).map(p => p.trim()).filter(p => p !== '').map((p, i) => ({ id: `p${i + 1}`, text: p }));
}

export const samplePassages = [
  'The framework resolves every provider through a keyed service, chosen from a configuration path.',
  'Templates are files named after the template, the target type and the template engine, for example order-shipped.html.hbs.',
  'Messages are put on a queue by an adapter: RabbitMQ, Azure Service Bus, Azure Storage queues or Amazon SQS.',
  'Sign-in uses OpenID Connect; the API only validates JWT bearer tokens and never stores passwords.',
].join('\n\n');

export class RagViewModel {
  readonly question = ref('How are message queues supported?');
  readonly passages = ref(samplePassages);
  readonly topK = ref(2);
  readonly result = ref<RagResult | null>(null);
  readonly ask = new AskCommand(this);
}

export class AskCommand extends Command {
  constructor(private readonly vm: RagViewModel) { super(); }
  protected isAllowed() { return this.vm.question.value.trim() !== '' && parsePassages(this.vm.passages.value).length > 0; }
  protected async run() {
    this.vm.result.value = await http.postJson<RagResult>('/api/Rag', {
      question: this.vm.question.value,
      passages: parsePassages(this.vm.passages.value),
      topK: this.vm.topK.value,
    });
  }
}
