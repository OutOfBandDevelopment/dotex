import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface DirectoryUser {
  id: string;
  userName: string;
  email: string | null;
  displayName: string | null;
  enabled: boolean;
  roles: string[];
  rights: string[];
}

export interface DirectoryRole {
  name: string;
  rights: string[];
}

export class IdentityDirectoryViewModel {
  readonly users = ref<DirectoryUser[]>([]);
  readonly roles = ref<DirectoryRole[]>([]);
  readonly search = ref('');
  readonly load = new LoadDirectoryCommand(this);

  rightsOf(role: DirectoryRole): string {
    return role.rights.length > 0 ? role.rights.join(', ') : 'none';
  }
}

export class LoadDirectoryCommand extends Command {
  constructor(private readonly vm: IdentityDirectoryViewModel) { super(); }
  protected async run() {
    const query = this.vm.search.value.trim() === '' ? '' : `?search=${encodeURIComponent(this.vm.search.value.trim())}`;
    const [users, roles] = await Promise.all([
      http.getJson<DirectoryUser[]>(`/api/Identity/users${query}`),
      http.getJson<DirectoryRole[]>('/api/Identity/roles'),
    ]);
    this.vm.users.value = users;
    this.vm.roles.value = roles;
  }
}
