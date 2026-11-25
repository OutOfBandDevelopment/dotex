import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

interface CompletionResponse { context: number[]; response: string; }

export interface Turn { role: 'user' | 'model'; text: string; }

/** Completion console state; `context` carries the model context between turns so a conversation continues. */
export class CompletionViewModel {
  readonly model = ref('');
  readonly system = ref('');
  readonly prompt = ref('');
  readonly keepContext = ref(true);
  readonly context = ref<number[]>([]);
  readonly turns = ref<Turn[]>([]);

  readonly send = new SendCompletionCommand(this);
  readonly reset = new ResetConversationCommand(this);

  constructor(readonly basePath: string) { }
}

export class SendCompletionCommand extends Command {
  constructor(private readonly vm: CompletionViewModel) { super(); }
  protected isAllowed() { return this.vm.prompt.value.trim().length > 0; }
  protected async run() {
    const prompt = this.vm.prompt.value;
    const result = await http.postJson<CompletionResponse>(`${this.vm.basePath}/Completion`, {
      prompt,
      model: this.vm.model.value || null,
      system: this.vm.system.value || null,
      context: this.vm.keepContext.value && this.vm.context.value.length ? this.vm.context.value : null,
    });
    this.vm.turns.value.push({ role: 'user', text: prompt }, { role: 'model', text: result.response });
    this.vm.context.value = this.vm.keepContext.value ? result.context : [];
    this.vm.prompt.value = '';
  }
}

export class ResetConversationCommand extends Command {
  constructor(private readonly vm: CompletionViewModel) { super(); }
  protected async run() {
    this.vm.turns.value = [];
    this.vm.context.value = [];
    this.vm.prompt.value = '';
  }
}

/** Embedding probe for the Ollama controller (`GET api/Ollama?text=`). */
export class OllamaEmbedViewModel {
  readonly text = ref('');
  readonly model = ref('');
  readonly vector = ref<number[]>([]);
  readonly embed = new EmbedCommand(this);
}

export class EmbedCommand extends Command {
  constructor(private readonly vm: OllamaEmbedViewModel) { super(); }
  protected isAllowed() { return this.vm.text.value.trim().length > 0; }
  protected async run() {
    const q = new URLSearchParams({ text: this.vm.text.value });
    if (this.vm.model.value) q.set('model', this.vm.model.value);
    this.vm.vector.value = await http.getJson<number[]>(`/api/Ollama?${q}`);
  }
}
