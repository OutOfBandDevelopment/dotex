import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface ImageLabel { label: string; probability: number; }
export interface ImageEmbedding { length: number; norm: number; first: number[]; }

export class ImagesViewModel {
  readonly file = ref<File | null>(null);
  readonly labels = ref('a cat, a dog, a car, a landscape');
  readonly ranked = ref<ImageLabel[]>([]);
  readonly embedding = ref<ImageEmbedding | null>(null);
  readonly classify = new ClassifyCommand(this);
  readonly embed = new EmbedCommand(this);
}

export class ClassifyCommand extends Command {
  constructor(private readonly vm: ImagesViewModel) { super(); }
  protected isAllowed() { return this.vm.file.value !== null && this.vm.labels.value.split(',').filter(l => l.trim() !== '').length >= 2; }
  protected async run() {
    const form = new FormData();
    form.append('file', this.vm.file.value!);
    form.append('labels', this.vm.labels.value);
    this.vm.ranked.value = await http.postFormJson<ImageLabel[]>('/api/Images/classify', form);
  }
}

export class EmbedCommand extends Command {
  constructor(private readonly vm: ImagesViewModel) { super(); }
  protected isAllowed() { return this.vm.file.value !== null; }
  protected async run() {
    const form = new FormData();
    form.append('file', this.vm.file.value!);
    this.vm.embedding.value = await http.postFormJson<ImageEmbedding>('/api/Images/embed', form);
  }
}
