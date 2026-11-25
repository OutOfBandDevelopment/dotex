import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface GenerationResult { sampleType: string; seed: number; value: unknown; }

export class GenerationViewModel {
  readonly sampleTypes = ['person', 'customer', 'order'];
  readonly sampleType = ref('customer');
  readonly seed = ref('42');
  readonly result = ref<GenerationResult | null>(null);
  readonly generate = new GenerateCommand(this);
}

export class GenerateCommand extends Command {
  constructor(private readonly vm: GenerationViewModel) { super(); }
  protected isAllowed() {
    return this.vm.sampleType.value !== '' && Number.isInteger(Number(this.vm.seed.value)) && this.vm.seed.value.trim() !== '';
  }
  protected async run() {
    this.vm.result.value = await http.postJson<GenerationResult>('/api/Generation', {
      sampleType: this.vm.sampleType.value, seed: Number(this.vm.seed.value),
    });
  }
}
