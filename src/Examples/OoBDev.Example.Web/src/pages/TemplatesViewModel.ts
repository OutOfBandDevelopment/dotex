import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface TemplateInfo {
  name: string;
  file: string;
  templateContentType: string;
  targetExtension: string;
}

interface TemplateDetail {
  info: TemplateInfo;
  text: string;
}

export class TemplatesViewModel {
  readonly supported = ref('');
  readonly templates = ref<TemplateInfo[]>([]);
  readonly selected = ref<TemplateInfo | null>(null);
  readonly text = ref('');
  readonly name = ref('order-shipped');
  readonly data = ref('{ "name": "Ada", "orderId": 1042 }');
  readonly output = ref('');
  readonly load = new LoadSupportedCommand(this);
  readonly list = new ListTemplatesCommand(this);
  readonly open = new OpenTemplateCommand(this);
  readonly preview = new PreviewTemplateCommand(this);
  readonly save = new SaveTemplateCommand(this);
  readonly apply = new ApplyTemplateCommand(this);
}

export class LoadSupportedCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected async run() {
    this.vm.supported.value = JSON.stringify(await http.postJson('/api/TextTemplate/SupportedTemplates', {}), null, 2);
  }
}

export class ListTemplatesCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected async run() {
    this.vm.templates.value = await http.getJson<TemplateInfo[]>('/api/TextTemplate/List');
  }
}

export class OpenTemplateCommand extends Command<TemplateInfo> {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected async run(info?: TemplateInfo) {
    if (!info) return;
    const detail = await http.getJson<TemplateDetail>(`/api/TextTemplate/Read?file=${encodeURIComponent(info.file)}`);
    this.vm.selected.value = detail.info;
    this.vm.text.value = detail.text;
    this.vm.name.value = detail.info.name;
  }
}

export class PreviewTemplateCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected isAllowed() {
    return this.vm.text.value.trim() !== '' && isJson(this.vm.data.value);
  }
  protected async run() {
    const selected = this.vm.selected.value;
    this.vm.output.value = await http.postText('/api/TextTemplate/Preview', {
      text: this.vm.text.value,
      templateContentType: selected?.templateContentType,
      targetExtension: selected?.targetExtension || '.txt',
      data: JSON.parse(this.vm.data.value || '{}'),
    });
  }
}

export class SaveTemplateCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected isAllowed() {
    return this.vm.selected.value !== null && this.vm.text.value.trim() !== '';
  }
  protected async run() {
    const selected = this.vm.selected.value;
    if (!selected) return;
    await http.sendJson('PUT', '/api/TextTemplate/Save', { file: selected.file, text: this.vm.text.value });
    this.vm.output.value = `Saved ${selected.file}`;
  }
}

export class ApplyTemplateCommand extends Command {
  constructor(private readonly vm: TemplatesViewModel) { super(); }
  protected isAllowed() {
    return this.vm.name.value.trim() !== '' && isJson(this.vm.data.value);
  }
  protected async run() {
    this.vm.output.value = await http.postText(
      `/api/TextTemplate/Apply?templateName=${encodeURIComponent(this.vm.name.value)}`,
      JSON.parse(this.vm.data.value || '{}'),
    );
  }
}

function isJson(value: string) {
  try { JSON.parse(value || '{}'); return true; } catch { return false; }
}
