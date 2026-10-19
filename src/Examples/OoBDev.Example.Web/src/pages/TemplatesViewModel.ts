import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export class TemplatesViewModel {
  readonly supported = ref('');
  readonly name = ref('welcome.hbs');
  readonly data = ref('{ "name": "Ada" }');
  readonly output = ref('');
  readonly load = new LoadSupportedCommand(this);
  readonly apply = new ApplyTemplateCommand(this);
}

export class LoadSupportedCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected async run() {
    this.vm.supported.value = JSON.stringify(await http.postJson('/api/TextTemplate/SupportedTemplates', {}), null, 2);
  }
}

export class ApplyTemplateCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected isAllowed() {
    try { JSON.parse(this.vm.data.value || '{}'); return this.vm.name.value.trim() !== ''; } catch { return false; }
  }
  protected async run() {
    this.vm.output.value = await http.postText(
      `/api/TextTemplate/Apply?templateName=${encodeURIComponent(this.vm.name.value)}`,
      JSON.parse(this.vm.data.value || '{}'),
    );
  }
}
