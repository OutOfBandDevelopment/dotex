import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export const targetTypes = ['text/plain', 'text/html', 'application/pdf', 'text/markdown'];

export class DocumentsViewModel {
  readonly file = ref<File | null>(null);
  readonly storeName = ref('');
  readonly targetType = ref(targetTypes[0]);
  readonly output = ref('');
  readonly convert = new ConvertCommand(this);
  readonly upload = new UploadCommand(this);
  readonly view = new ViewTextCommand(this);
  readonly viewHtml = new ViewStoredCommand(this, 'Html');
  readonly viewSummary = new ViewStoredCommand(this, 'Summary');
}

export class ConvertCommand extends Command {
  constructor(private readonly vm: DocumentsViewModel) { super(); }
  protected isAllowed() { return this.vm.file.value !== null; }
  protected async run() {
    const form = new FormData();
    form.append('content', this.vm.file.value!);
    const r = await http.postForm(`/Document/Convert?targetContentType=${encodeURIComponent(this.vm.targetType.value)}`, form);
    this.vm.output.value = r.contentType.startsWith('text') ? r.text : `(${r.contentType}, binary content not shown)`;
  }
}

export class UploadCommand extends Command {
  constructor(private readonly vm: DocumentsViewModel) { super(); }
  protected isAllowed() { return this.vm.file.value !== null && this.vm.storeName.value.trim() !== ''; }
  protected async run() {
    const form = new FormData();
    form.append('content', this.vm.file.value!);
    const r = await http.postForm(`/Document/Upload/${this.vm.storeName.value}`, form);
    this.vm.output.value = r.text || 'Uploaded.';
  }
}

/** Shows a stored document through one of the read actions (Html, Summary). */
export class ViewStoredCommand extends Command {
  constructor(private readonly vm: DocumentsViewModel, private readonly action: 'Html' | 'Summary') { super(); }
  protected isAllowed() { return this.vm.storeName.value.trim() !== ''; }
  protected async run() {
    this.vm.output.value = await http.getText(`/Document/${this.action}/${this.vm.storeName.value}`);
  }
}

export class ViewTextCommand extends Command {
  constructor(private readonly vm: DocumentsViewModel) { super(); }
  protected isAllowed() { return this.vm.storeName.value.trim() !== ''; }
  protected async run() {
    this.vm.output.value = await http.getText(`/Document/Text/${this.vm.storeName.value}`);
  }
}
