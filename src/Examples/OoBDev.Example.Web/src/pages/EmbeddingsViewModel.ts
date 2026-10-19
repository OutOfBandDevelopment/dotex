import { computed, ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export function cosine(a: number[], b: number[]): number {
  let dot = 0, na = 0, nb = 0;
  for (let i = 0; i < Math.min(a.length, b.length); i++) {
    dot += a[i] * b[i];
    na += a[i] * a[i];
    nb += b[i] * b[i];
  }
  return na === 0 || nb === 0 ? 0 : dot / (Math.sqrt(na) * Math.sqrt(nb));
}

export const models = [
  { label: 'SBERT (default)', path: '/api/SBERTDefault' },
  { label: 'SBERT', path: '/api/SBERT' },
  { label: 'AllMiniLM L6 v2', path: '/api/AllMiniLM' },
];

export class EmbeddingsViewModel {
  readonly model = ref(models[0].path);
  readonly textA = ref('cats purr');
  readonly textB = ref('kittens make quiet noises');
  readonly vectorA = ref<number[]>([]);
  readonly vectorB = ref<number[]>([]);
  readonly similarity = computed(() => cosine(this.vectorA.value, this.vectorB.value));
  readonly embed = new EmbedCommand(this);
}

export class EmbedCommand extends Command {
  constructor(private readonly vm: EmbeddingsViewModel) { super(); }
  protected isAllowed() { return this.vm.textA.value.trim() !== '' && this.vm.textB.value.trim() !== ''; }
  protected async run() {
    const get = (t: string) => http.getJson<number[]>(`${this.vm.model.value}?text=${encodeURIComponent(t)}`);
    [this.vm.vectorA.value, this.vm.vectorB.value] = await Promise.all([get(this.vm.textA.value), get(this.vm.textB.value)]);
  }
}
