import { Produs } from './Produs';
import { Comanda } from './Comanda'
// models/Subcomanda.ts
export interface Subcomanda {
  IdSubcomanda: number;
  IdProdus: number;
  Produs?: any;       // navigational property, opțional
  IdComanda: number;
  Comanda?: any;      // navigational property, opțional
  TotalSubproduse: number;  // => important: corespunde "TotalSubproduse" din C#
}

