import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface DataQueryResult { columns: string[]; rows: (string | null)[][]; truncated: boolean; elapsedMs: number; }

export class DataAccessViewModel {
  readonly sql = ref('SELECT TOP (10) name, create_date FROM sys.databases ORDER BY name');
  readonly result = ref<DataQueryResult | null>(null);
  readonly run = new RunQueryCommand(this);
}

export class RunQueryCommand extends Command {
  constructor(private readonly vm: DataAccessViewModel) { super(); }
  protected isAllowed() { return this.vm.sql.value.trim() !== ''; }
  protected async run() {
    this.vm.result.value = await http.postJson<DataQueryResult>('/api/DataAccess/query', { sql: this.vm.sql.value });
  }
}
