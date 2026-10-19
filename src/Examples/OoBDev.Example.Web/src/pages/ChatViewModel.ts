import { computed, ref, type Ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface ChatState {
  promptDetails: Ref<string>;
  data: Ref<string>;
  userInput: Ref<string>;
  response: Ref<string>;
}

const body = (s: ChatState) => ({ promptDetails: s.promptDetails.value, data: s.data.value, userInput: s.userInput.value });

export class SendPromptCommand extends Command {
  constructor(private readonly s: ChatState) { super(); }
  protected isAllowed() { return this.s.userInput.value.trim().length > 0; }
  protected async run() {
    this.s.response.value = '';
    this.s.response.value = await http.postJson<string>('/api/AI', body(this.s));
  }
}

export class StreamPromptCommand extends Command {
  private abort: AbortController | null = null;
  constructor(private readonly s: ChatState) { super(); }
  protected isAllowed() { return this.s.userInput.value.trim().length > 0; }
  protected async run() {
    this.s.response.value = '';
    this.abort = new AbortController();
    try {
      await http.postStream('/api/AI/Streamed', body(this.s), t => { this.s.response.value += t; }, this.abort.signal);
    } finally {
      this.abort = null;
    }
  }
  cancel() { this.abort?.abort(); }
}

export class ClearChatCommand extends Command {
  constructor(private readonly s: ChatState) { super(); }
  protected async run() {
    this.s.userInput.value = '';
    this.s.response.value = '';
  }
}

export class ChatViewModel implements ChatState {
  readonly promptDetails = ref('You are a helpful assistant.');
  readonly data = ref('');
  readonly userInput = ref('');
  readonly response = ref('');
  readonly send = new SendPromptCommand(this);
  readonly stream = new StreamPromptCommand(this);
  readonly clear = new ClearChatCommand(this);
  readonly busy = computed(() => this.send.running.value || this.stream.running.value);
  readonly error = computed(() => this.send.error.value ?? this.stream.error.value);
}
