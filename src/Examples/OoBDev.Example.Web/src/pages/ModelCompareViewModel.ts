import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export const modelKeys = ['all-minilm-l6-v2', 'all-mpnet-base-v2', 'nomic-embed-text-v1.5'];

export interface ModelScore { model: string; length: number; similarity: number; milliseconds: number; }

export class ModelCompareViewModel {
  readonly a = ref('The cat sat on the mat.');
  readonly b = ref('A kitten is resting on a rug.');
  readonly selected = ref<string[]>([...modelKeys.slice(0, 1)]);
  readonly scores = ref<ModelScore[]>([]);
  readonly compare = new CompareModelsCommand(this);
}

export class CompareModelsCommand extends Command {
  constructor(private readonly vm: ModelCompareViewModel) { super(); }
  protected isAllowed() { return this.vm.a.value.trim() !== '' && this.vm.b.value.trim() !== '' && this.vm.selected.value.length > 0; }
  protected async run() {
    this.vm.scores.value = await http.postJson<ModelScore[]>('/api/ModelCompare/similarity', {
      a: this.vm.a.value, b: this.vm.b.value, models: this.vm.selected.value,
    });
  }
}
