import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface VectorHit { id: string; score: number; text: string | null; }

export const sampleTexts = [
  'The cat sat on the warm windowsill.',
  'Invoices are due thirty days after delivery.',
  'A kitten curled up on the sofa to sleep.',
  'Quarterly revenue grew by twelve percent.',
].join('\n');

export class VectorStoreViewModel {
  readonly collection = ref('notes');
  readonly texts = ref(sampleTexts);
  readonly query = ref('a sleeping pet');
  readonly store = ref('');
  readonly stored = ref<string[]>([]);
  readonly hits = ref<VectorHit[]>([]);
  readonly index = new IndexCommand(this);
  readonly search = new SearchCommand(this);
  readonly list = new ListCommand(this);

  /** The non-empty lines of the text box, one document each. */
  lines() { return this.texts.value.split('\n').map(l => l.trim()).filter(l => l !== ''); }
}

export class IndexCommand extends Command {
  constructor(private readonly vm: VectorStoreViewModel) { super(); }
  protected isAllowed() { return this.vm.collection.value.trim() !== '' && this.vm.lines().length > 0; }
  protected async run() {
    const r = await http.postJson<{ ids: string[]; store: string }>('/api/VectorStore/index', {
      collection: this.vm.collection.value.trim(), texts: this.vm.lines(),
    });
    this.vm.stored.value = r.ids;
    this.vm.store.value = r.store;
  }
}

export class SearchCommand extends Command {
  constructor(private readonly vm: VectorStoreViewModel) { super(); }
  protected isAllowed() { return this.vm.collection.value.trim() !== '' && this.vm.query.value.trim() !== ''; }
  protected async run() {
    this.vm.hits.value = await http.postJson<VectorHit[]>('/api/VectorStore/query', {
      collection: this.vm.collection.value.trim(), query: this.vm.query.value, limit: 5,
    });
  }
}

export class ListCommand extends Command {
  constructor(private readonly vm: VectorStoreViewModel) { super(); }
  protected isAllowed() { return this.vm.collection.value.trim() !== ''; }
  protected async run() {
    this.vm.hits.value = await http.getJson<VectorHit[]>(`/api/VectorStore/${encodeURIComponent(this.vm.collection.value.trim())}`);
  }
}
