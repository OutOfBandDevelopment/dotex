import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';
import { parsePassages, samplePassages } from './RagViewModel';

export interface SearchHit { id: string; score: number; text: string; }
export interface SearchResult { lexical: SearchHit[]; semantic: SearchHit[]; hybrid: SearchHit[]; }

export class SearchViewModel {
  readonly query = ref('how do I send messages through a queue');
  readonly passages = ref(samplePassages);
  readonly limit = ref(3);
  readonly result = ref<SearchResult | null>(null);
  readonly search = new SearchCommand(this);
}

export class SearchCommand extends Command {
  constructor(private readonly vm: SearchViewModel) { super(); }
  protected isAllowed() { return this.vm.query.value.trim() !== '' && parsePassages(this.vm.passages.value).length > 0; }
  protected async run() {
    this.vm.result.value = await http.postJson<SearchResult>('/api/Search', {
      query: this.vm.query.value,
      passages: parsePassages(this.vm.passages.value),
      limit: this.vm.limit.value,
    });
  }
}
