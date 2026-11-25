import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export type CatalogRow = Record<string, string | number | boolean>;
interface Page { value?: CatalogRow[]; '@odata.count'?: number }

export class QueryViewModel {
  readonly filter = ref("Category eq 'Instruments' and Price lt 100");
  readonly orderBy = ref('Price desc');
  readonly top = ref('5');
  readonly rows = ref<CatalogRow[] | null>(null);
  readonly total = ref<number | null>(null);
  readonly run = new QueryCommand(this);

  /** The request path with the system query options that are filled in. */
  queryPath(): string {
    const options: string[] = ['$count=true'];
    if (this.filter.value.trim()) options.push(`$filter=${encodeURIComponent(this.filter.value.trim())}`);
    if (this.orderBy.value.trim()) options.push(`$orderby=${encodeURIComponent(this.orderBy.value.trim())}`);
    options.push(`$top=${Number(this.top.value)}`);
    return `/api/SampleCatalog?${options.join('&')}`;
  }
}

export class QueryCommand extends Command {
  constructor(private readonly vm: QueryViewModel) { super(); }
  protected isAllowed() {
    const top = Number(this.vm.top.value);
    return Number.isInteger(top) && top > 0;
  }
  protected async run() {
    const body = await http.getJson<Page | CatalogRow[]>(this.vm.queryPath());
    const rows = Array.isArray(body) ? body : (body.value ?? []);
    this.vm.rows.value = rows;
    this.vm.total.value = Array.isArray(body) ? null : (body['@odata.count'] ?? null);
  }
}
