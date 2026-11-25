import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export interface DistanceResult { meters: number; kilometers: number; miles: number; bearingDegrees: number; }

export class DistanceViewModel {
  readonly fromLatitude = ref('51.5074');
  readonly fromLongitude = ref('-0.1278');
  readonly toLatitude = ref('48.8566');
  readonly toLongitude = ref('2.3522');
  readonly result = ref<DistanceResult | null>(null);
  readonly measure = new DistanceCommand(this);
}

export class DistanceCommand extends Command {
  constructor(private readonly vm: DistanceViewModel) { super(); }
  protected isAllowed() {
    return [this.vm.fromLatitude, this.vm.fromLongitude, this.vm.toLatitude, this.vm.toLongitude].every(v => v.value.trim() !== '' && Number.isFinite(Number(v.value)));
  }
  protected async run() {
    this.vm.result.value = await http.postJson<DistanceResult>('/api/Distance', {
      fromLatitude: Number(this.vm.fromLatitude.value), fromLongitude: Number(this.vm.fromLongitude.value),
      toLatitude: Number(this.vm.toLatitude.value), toLongitude: Number(this.vm.toLongitude.value),
    });
  }
}
