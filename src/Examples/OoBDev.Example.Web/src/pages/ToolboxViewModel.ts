import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export const hashAlgorithms = ['sha512', 'sha256', 'md5'];
export const hmacAlgorithms = ['hmac-256', 'hmac-512', 'hmac3-256', 'hmac3-512'];

export interface ToolResult { algorithm: string; result: string; }
export interface VectorDistances { cosine: number; euclidean: number; dotProduct: number; manhattan: number; }

/** Parses numbers separated by commas, semicolons or whitespace; returns null when any item is not a number. */
export function parseNumbers(text: string): number[] | null {
  const items = text.split(/[\s,;]+/).filter(s => s !== '');
  const numbers = items.map(Number);
  return items.length > 0 && numbers.every(Number.isFinite) ? numbers : null;
}

export class ToolboxViewModel {
  readonly hashAlgorithm = ref(hashAlgorithms[0]);
  readonly hashText = ref('hello');
  readonly hashResult = ref('');
  readonly hmacAlgorithm = ref(hmacAlgorithms[0]);
  readonly secret = ref('shared-secret');
  readonly message = ref('order 42 shipped');
  readonly hmacResult = ref('');
  readonly vectorA = ref('1, 2, 3');
  readonly vectorB = ref('2, 4, 6.5');
  readonly distances = ref<VectorDistances | null>(null);
  readonly hash = new HashCommand(this);
  readonly hmac = new HmacCommand(this);
  readonly distance = new DistanceCommand(this);
}

export class HashCommand extends Command {
  constructor(private readonly vm: ToolboxViewModel) { super(); }
  protected isAllowed() { return this.vm.hashText.value !== ''; }
  protected async run() {
    const r = await http.postJson<ToolResult>('/api/Toolbox/hash', { algorithm: this.vm.hashAlgorithm.value, value: this.vm.hashText.value });
    this.vm.hashResult.value = r.result;
  }
}

export class HmacCommand extends Command {
  constructor(private readonly vm: ToolboxViewModel) { super(); }
  protected isAllowed() { return this.vm.secret.value !== '' && this.vm.message.value !== ''; }
  protected async run() {
    const r = await http.postJson<ToolResult>('/api/Toolbox/hmac', { algorithm: this.vm.hmacAlgorithm.value, secret: this.vm.secret.value, message: this.vm.message.value });
    this.vm.hmacResult.value = r.result;
  }
}

export class DistanceCommand extends Command {
  constructor(private readonly vm: ToolboxViewModel) { super(); }
  protected isAllowed() {
    const a = parseNumbers(this.vm.vectorA.value), b = parseNumbers(this.vm.vectorB.value);
    return a !== null && b !== null && a.length === b.length;
  }
  protected async run() {
    this.vm.distances.value = await http.postJson<VectorDistances>('/api/Toolbox/vector-distance', {
      a: parseNumbers(this.vm.vectorA.value), b: parseNumbers(this.vm.vectorB.value),
    });
  }
}
