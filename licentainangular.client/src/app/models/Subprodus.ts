import { Produs } from './Produs';
import { Cos } from './Cos';
export interface Subprodus {
  IdSubprodus?: number;
  IdProdus: number;
  Produs: Produs | null; // Reference to the Produs object
  Valabil: boolean;
  idCos: number | null;
  Cos: Cos | null; // Reference to the Cos object
}
