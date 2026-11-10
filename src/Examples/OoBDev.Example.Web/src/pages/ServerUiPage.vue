<script setup lang="ts">
import { onMounted } from 'vue';
import { ServerUiViewModel } from './ServerUiViewModel';

const vm = new ServerUiViewModel();
onMounted(() => vm.list.execute());

function text(e: Event): string {
  return (e.target as HTMLInputElement).value;
}
</script>

<template>
  <h1>Server-driven UI</h1>
  <p>The API returns a <code>UiDefinition</code>, the framework's toolkit-neutral description of a control panel. This page knows nothing about the forms below: it renders whatever controls the definition lists, and asks the host which controls are visible, enabled and valid each time a value changes.</p>
  <p v-if="vm.list.error.value" class="error">{{ vm.list.error.value }}</p>
  <h2>Definitions</h2>
  <ul class="ui-list">
    <li v-for="d in vm.definitions.value" :key="d.key">
      <fluent-button :appearance="vm.selected.value?.key === d.key ? 'primary' : 'neutral'" @click="vm.open.execute(d)">{{ d.name }}</fluent-button>
      <span class="hint"> {{ d.source }}<template v-if="d.description"> — {{ d.description }}</template></span>
    </li>
  </ul>
  <p v-if="vm.open.error.value" class="error">{{ vm.open.error.value }}</p>

  <div v-if="vm.definition.value" class="ui-panel">
    <h2>{{ vm.definition.value.name }}</h2>
    <template v-for="(section, index) in vm.definition.value.sections" :key="index">
      <fieldset v-if="vm.sectionVisible(section)" class="ui-section">
        <legend v-if="section.label">{{ section.label }}</legend>
        <template v-for="c in section.controls" :key="c.id">
          <div v-if="vm.state(c.id)?.visible ?? true" class="ui-control" :data-control="c.id">
            <template v-if="c.kind === 'toggle'">
              <label><input type="checkbox" :aria-label="c.label" :checked="vm.display(c.id) === 'true'" :disabled="!(vm.state(c.id)?.enabled ?? true)" @change="vm.setValue(c.id, ($event.target as HTMLInputElement).checked ? 'true' : 'false')" /> {{ c.label }}</label>
            </template>
            <template v-else-if="c.kind === 'choice'">
              <span class="ui-label">{{ c.label }}</span>
              <template v-if="c.style === 'radioGroup'">
                <label v-for="o in c.options" :key="o" class="ui-option"><input type="radio" :name="c.id" :aria-label="o" :checked="vm.display(c.id) === o" :disabled="!(vm.state(c.id)?.enabled ?? true)" @change="vm.setValue(c.id, o)" /> {{ o }}</label>
              </template>
              <select v-else :aria-label="c.label" :value="vm.display(c.id)" :disabled="!(vm.state(c.id)?.enabled ?? true)" @change="vm.setValue(c.id, text($event))">
                <option v-for="o in c.options" :key="o" :value="o">{{ o }}</option>
              </select>
            </template>
            <template v-else-if="c.kind === 'slider'">
              <label>{{ c.label }}
                <input type="range" :aria-label="c.label" :min="c.minimum" :max="c.maximum" :step="c.step" :value="vm.display(c.id)" :disabled="!(vm.state(c.id)?.enabled ?? true)" @change="vm.setValue(c.id, text($event))" />
                {{ vm.display(c.id) }} {{ c.unit }}
              </label>
            </template>
            <template v-else-if="c.kind === 'numeric' || c.kind === 'textField'">
              <label>{{ c.label }}
                <input :aria-label="c.label" :maxlength="c.kind === 'textField' ? c.maxLength : undefined" :value="vm.display(c.id)" :disabled="!(vm.state(c.id)?.enabled ?? true)" @input="vm.setValue(c.id, text($event))" />
                {{ c.unit }}
              </label>
            </template>
            <template v-else-if="c.kind === 'button'">
              <fluent-button :disabled="!(vm.state(c.id)?.enabled ?? true)" @click="vm.press.execute(c)">{{ c.label }}</fluent-button>
            </template>
            <template v-else-if="c.kind === 'indicator'">
              <span class="ui-label">{{ c.label }}:</span> <strong class="ui-indicator">{{ vm.display(c.id) }}</strong>
            </template>
            <template v-else>
              <span class="hint">{{ c.label }} ({{ c.kind }}) needs a live feed and is not drawn here.</span>
            </template>
            <span v-if="vm.state(c.id) && !vm.state(c.id)!.enabled && vm.state(c.id)!.disabledReason" class="hint ui-reason"> {{ vm.state(c.id)!.disabledReason }}</span>
            <span v-if="vm.state(c.id) && !vm.state(c.id)!.valid" class="error ui-error"> {{ vm.state(c.id)!.error }}</span>
          </div>
        </template>
      </fieldset>
    </template>
    <p v-if="vm.lastAction.value" class="ui-action">{{ vm.lastAction.value }}</p>
    <p v-if="vm.evaluate.error.value" class="error">{{ vm.evaluate.error.value }}</p>
  </div>
</template>
