import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface UiDefinitionInfo {
  key: string;
  name: string;
  description?: string;
  source: string;
}

export interface UiCondition {
  id: string;
  values: string[];
}

export interface UiControl {
  kind: 'button' | 'toggle' | 'slider' | 'numeric' | 'choice' | 'textField' | 'indicator' | 'barGraph' | 'stripChart' | 'vector';
  id: string;
  label: string;
  description?: string;
  options?: string[];
  style?: string;
  minimum?: number;
  maximum?: number;
  step?: number;
  unit?: string;
  maxLength?: number;
  commandId?: string;
}

export interface UiSection {
  label?: string;
  controls: UiControl[];
}

export interface UiDefinition {
  name: string;
  description?: string;
  sections: UiSection[];
}

export interface ControlState {
  id: string;
  visible: boolean;
  enabled: boolean;
  disabledReason?: string;
  valid: boolean;
  value: string;
  error?: string;
}

export class ServerUiViewModel {
  readonly definitions = ref<UiDefinitionInfo[]>([]);
  readonly selected = ref<UiDefinitionInfo | null>(null);
  readonly definition = ref<UiDefinition | null>(null);
  readonly values = ref<Record<string, string>>({});
  readonly states = ref<Record<string, ControlState>>({});
  readonly lastAction = ref('');
  readonly list = new ListDefinitionsCommand(this);
  readonly open = new OpenDefinitionCommand(this);
  readonly evaluate = new EvaluateCommand(this);
  readonly press = new PressButtonCommand(this);

  /** The text a control shows: what the user typed, else the server-evaluated default. */
  display(id: string): string {
    return this.values.value[id] ?? this.states.value[id]?.value ?? '';
  }

  state(id: string): ControlState | undefined {
    return this.states.value[id];
  }

  /** A section shows when at least one of its controls is visible (the server folds the section condition into each control). */
  sectionVisible(section: UiSection): boolean {
    return section.controls.some(c => this.states.value[c.id]?.visible ?? true);
  }

  async setValue(id: string, value: string): Promise<void> {
    this.values.value = { ...this.values.value, [id]: value };
    await this.evaluate.execute();
  }
}

export class ListDefinitionsCommand extends Command {
  constructor(private readonly vm: ServerUiViewModel) { super(); }
  protected async run() {
    this.vm.definitions.value = await http.getJson<UiDefinitionInfo[]>('/api/UiDefinitions');
  }
}

export class OpenDefinitionCommand extends Command<UiDefinitionInfo> {
  constructor(private readonly vm: ServerUiViewModel) { super(); }
  protected async run(info?: UiDefinitionInfo) {
    if (!info) return;
    this.vm.definition.value = await http.getJson<UiDefinition>(`/api/UiDefinitions/${encodeURIComponent(info.key)}`);
    this.vm.selected.value = info;
    this.vm.values.value = {};
    this.vm.lastAction.value = '';
    await this.vm.evaluate.execute();
  }
}

export class EvaluateCommand extends Command {
  constructor(private readonly vm: ServerUiViewModel) { super(); }
  protected isAllowed() { return this.vm.selected.value !== null; }
  protected async run() {
    const key = this.vm.selected.value!.key;
    const result = await http.postJson<ControlState[]>(`/api/UiDefinitions/${encodeURIComponent(key)}/evaluate`, { values: this.vm.values.value });
    this.vm.states.value = Object.fromEntries(result.map(s => [s.id, s]));
  }
}

export class PressButtonCommand extends Command<UiControl> {
  constructor(private readonly vm: ServerUiViewModel) { super(); }
  protected async run(control?: UiControl) {
    if (!control) return;
    this.vm.lastAction.value = `Button ${control.id} would send command ${control.commandId ?? control.id}.`;
  }
}
