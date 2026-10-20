import DOMPurify from 'dompurify';
import { computed, ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export const formats = ['json', 'yaml', 'html', 'xml'];

export const samples: Record<string, { content: string; xpath: string }> = {
  json: { content: '{"orders":[{"id":1,"total":12.5},{"id":2,"total":40}]}', xpath: '//total' },
  yaml: { content: 'orders:\n  - id: 1\n    total: 12.5\n  - id: 2\n    total: 40\n', xpath: '//total' },
  html: { content: '<ul><li class="a">One</li><li>Two</li></ul>', xpath: '//li' },
  xml: { content: '<orders><order id="1" total="12.5"/><order id="2" total="40"/></orders>', xpath: '//order[@total>20]' },
};

export interface XPathResult { count: number; matches: string[]; xml: string; }

export class TextToolsViewModel {
  readonly format = ref('json');
  readonly content = ref(samples.json.content);
  readonly xpath = ref(samples.json.xpath);
  readonly result = ref<XPathResult | null>(null);
  readonly markdown = ref('# Title\n\n* one\n* two\n\n| a | b |\n|---|---|\n| 1 | 2 |\n');
  readonly html = ref('');
  /** The rendered HTML with scripts and event handlers removed, safe to bind with v-html. */
  readonly safeHtml = computed(() => DOMPurify.sanitize(this.html.value));
  readonly query = new XPathQueryCommand(this);
  readonly render = new RenderMarkdownCommand(this);

  /** Replaces the document and query with the sample for the chosen format. */
  useSample() {
    const s = samples[this.format.value];
    this.content.value = s.content;
    this.xpath.value = s.xpath;
    this.result.value = null;
  }
}

export class XPathQueryCommand extends Command {
  constructor(private readonly vm: TextToolsViewModel) { super(); }
  protected isAllowed() { return this.vm.content.value.trim() !== '' && this.vm.xpath.value.trim() !== ''; }
  protected async run() {
    this.vm.result.value = await http.postJson<XPathResult>('/api/TextTools/xpath', {
      format: this.vm.format.value, content: this.vm.content.value, xpath: this.vm.xpath.value,
    });
  }
}

export class RenderMarkdownCommand extends Command {
  constructor(private readonly vm: TextToolsViewModel) { super(); }
  protected isAllowed() { return this.vm.markdown.value.trim() !== ''; }
  protected async run() {
    const r = await http.postJson<{ html: string }>('/api/TextTools/markdown', { markdown: this.vm.markdown.value });
    this.vm.html.value = r.html;
  }
}
