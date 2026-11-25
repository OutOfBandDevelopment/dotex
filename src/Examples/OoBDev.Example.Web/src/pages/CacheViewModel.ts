import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface CacheLookup { key: string; found: boolean; value?: string | null; provider: string; }

export class CacheViewModel {
  readonly key = ref('greeting');
  readonly value = ref('hello from the cache');
  readonly seconds = ref(30);
  readonly lookupResult = ref<CacheLookup | null>(null);
  readonly status = ref('');
  readonly store = new StoreCommand(this);
  readonly lookup = new LookupCommand(this);
  readonly flush = new FlushCommand(this);
}

abstract class KeyedCommand extends Command {
  constructor(protected readonly vm: CacheViewModel) { super(); }
  protected isAllowed() { return this.vm.key.value.trim() !== ''; }
  protected get query() { return `/api/Cache?key=${encodeURIComponent(this.vm.key.value)}`; }
}

export class StoreCommand extends KeyedCommand {
  protected async run() {
    await http.sendJson('PUT', '/api/Cache', { key: this.vm.key.value, value: this.vm.value.value, seconds: this.vm.seconds.value });
    this.vm.status.value = `Stored "${this.vm.key.value}" for ${this.vm.seconds.value} seconds.`;
  }
}

export class LookupCommand extends KeyedCommand {
  protected async run() {
    this.vm.lookupResult.value = await http.getJson<CacheLookup>(this.query);
    this.vm.status.value = '';
  }
}

export class FlushCommand extends KeyedCommand {
  protected async run() {
    await http.sendJson('DELETE', this.query);
    this.vm.status.value = `Removed "${this.vm.key.value}".`;
  }
}
