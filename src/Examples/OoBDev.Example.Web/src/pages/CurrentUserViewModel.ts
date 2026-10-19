import { ref, type Ref } from 'vue';
import { Command } from '../core/command';
import { api } from '../core/api';

export interface Claim { type: string; value: string; issuer: string; }

export class RefreshClaimsCommand extends Command {
  constructor(private readonly target: Ref<Claim[]>) { super(); }
  protected async run(): Promise<void> {
    this.target.value = (await api.get('/api/User/claims')) as unknown as Claim[];
  }
}

export class CurrentUserViewModel {
  readonly claims = ref<Claim[]>([]);
  readonly refresh = new RefreshClaimsCommand(this.claims);
}
