import { User } from './User';
import { Adresa } from './Adresa';
import { Card_Plati } from './Card_Plati';

// models/Comanda.ts
export interface Comanda {
  IdComanda: number;
  IdUser: number;
  User?: any;          // navigational property, opțional
  IdAdresa: number;
  Adresa?: any;        // navigational property, opțional
  IdCard?: number;     // backend-l îl numește "IdCard_CC" sau similar
  Card?: any;          // navigational property, opțional
  ETA?: Date | string; // Data estimată de livrare
  PretTotal: number;   // => corespunde double PretTotal din C#
  IsPlaced: boolean;   // => corespunde bool IsPlaced
}

