import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface KernelFunction { plugIn: string; name: string; description: string | null; parameters: Record<string, string | null>; }
interface ToolResult { algorithm: string; result: string; }

/** Lists the Semantic Kernel plug-in functions and invokes one with text arguments. */
export class KernelPlugInsViewModel {
  readonly functions = ref<KernelFunction[]>([]);
  readonly selected = ref<KernelFunction | null>(null);
  readonly arguments = ref<Record<string, string>>({});
  readonly result = ref('');
  readonly load = new LoadKernelFunctionsCommand(this);
  readonly invoke = new InvokeKernelFunctionCommand(this);

  select(fn: KernelFunction): void {
    this.selected.value = fn;
    this.arguments.value = Object.fromEntries(Object.keys(fn.parameters).map(k => [k, '']));
    this.result.value = '';
  }
}

export class LoadKernelFunctionsCommand extends Command {
  constructor(private readonly vm: KernelPlugInsViewModel) { super(); }
  protected async run() {
    this.vm.functions.value = await http.getJson<KernelFunction[]>('/api/KernelPlugIns');
  }
}

export class InvokeKernelFunctionCommand extends Command {
  constructor(private readonly vm: KernelPlugInsViewModel) { super(); }
  protected isAllowed() { return this.vm.selected.value !== null; }
  protected async run() {
    const fn = this.vm.selected.value!;
    const r = await http.postJson<ToolResult>('/api/KernelPlugIns/invoke', { plugIn: fn.plugIn, function: fn.name, arguments: this.vm.arguments.value });
    this.vm.result.value = r.result;
  }
}
