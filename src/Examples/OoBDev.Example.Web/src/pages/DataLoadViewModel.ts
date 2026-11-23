import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface LoadError { line: number; message: string; }
export interface LoadPreview { read: number; written: number; replaced: number; skipped: number; errors: LoadError[]; columns: string[]; rows: (string | null)[][]; }

export const sampleCsv = 'id,name,city\n1,Ann,Oslo\n2,Bo,Lima\n1,Ann again,Oslo';

export class DataLoadViewModel {
  readonly format = ref('csv');
  readonly text = ref(sampleCsv);
  readonly keyColumn = ref('id');
  readonly duplicates = ref('Skip');
  readonly preview = ref<LoadPreview | null>(null);
  readonly load = new DataLoadCommand(this);
}

export class DataLoadCommand extends Command {
  constructor(private readonly vm: DataLoadViewModel) { super(); }
  protected isAllowed() { return this.vm.text.value.trim() !== '' && this.vm.keyColumn.value.trim() !== ''; }
  protected async run() {
    this.vm.preview.value = await http.postJson<LoadPreview>('/api/DataLoad/preview', {
      format: this.vm.format.value, text: this.vm.text.value, keyColumn: this.vm.keyColumn.value, duplicates: this.vm.duplicates.value,
    });
  }
}
