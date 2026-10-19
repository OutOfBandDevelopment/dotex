import { toasts } from './toast';
import { computed, ref, type ComputedRef, type Ref } from 'vue';

/** A user action: real, retestable class with injected dependencies (not an inline lambda). */
export abstract class Command<TArg = void> {
  readonly running: Ref<boolean> = ref(false);
  readonly error: Ref<string | null> = ref(null);

  /** Override to disable the command based on view-model state. */
  protected isAllowed(): boolean {
    return true;
  }

  readonly canExecute: ComputedRef<boolean> = computed(() => !this.running.value && this.isAllowed());

  protected abstract run(arg: TArg): Promise<void>;

  async execute(arg: TArg): Promise<void> {
    if (!this.canExecute.value) return;
    this.running.value = true;
    this.error.value = null;
    try {
      await this.run(arg);
    } catch (e) {
      this.error.value = e instanceof Error ? e.message : String(e);
      toasts.show(this.error.value, 'error');
    } finally {
      this.running.value = false;
    }
  }
}
