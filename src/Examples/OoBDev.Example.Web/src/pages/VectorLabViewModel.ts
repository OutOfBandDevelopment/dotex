import { ref } from 'vue';
import { Command } from '../core/command';
import { http } from '../core/http';

export const metrics = ['Cosine', 'Euclidean', 'DotProduct', 'Manhattan'];

export interface MatrixResult { matrix: number[][]; magnitudes: number[]; centroid: number[]; }

/** Reads one vector per line, numbers separated by commas or spaces; null when a line is not numeric or lengths differ. */
export function parseRows(text: string): number[][] | null {
  const rows = text.split('\n').map(l => l.trim()).filter(l => l !== '')
    .map(l => l.split(/[\s,;]+/).map(Number));
  if (rows.length === 0 || rows.some(r => r.some(Number.isNaN) || r.length !== rows[0].length)) return null;
  return rows;
}

export class VectorLabViewModel {
  readonly rows = ref('1, 0, 0\n0, 1, 0\n0.9, 0.1, 0\n0.5, 0.5, 0.7');
  readonly metric = ref('Cosine');
  readonly result = ref<MatrixResult | null>(null);
  readonly compute = new MatrixCommand(this);
}

export class MatrixCommand extends Command {
  constructor(private readonly vm: VectorLabViewModel) { super(); }
  protected isAllowed() { return parseRows(this.vm.rows.value) !== null; }
  protected async run() {
    this.vm.result.value = await http.postJson<MatrixResult>('/api/VectorLab/matrix', {
      vectors: parseRows(this.vm.rows.value), metric: this.vm.metric.value,
    });
  }
}
