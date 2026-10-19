import { reactive } from 'vue';

export interface Toast { id: number; kind: 'info' | 'error'; text: string; }

let next = 1;

/** Transient messages shown at the bottom of the shell. */
export const toasts = reactive({
  items: [] as Toast[],
  show(text: string, kind: Toast['kind'] = 'info', ms = 4000) {
    const id = next++;
    this.items.push({ id, kind, text });
    setTimeout(() => this.dismiss(id), ms);
  },
  dismiss(id: number) {
    const i = this.items.findIndex(t => t.id === id);
    if (i >= 0) this.items.splice(i, 1);
  },
});
