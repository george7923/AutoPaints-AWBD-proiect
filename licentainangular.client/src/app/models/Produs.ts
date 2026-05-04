import { User } from './User';

export interface Produs {
  IdProdus: number;
  Nume: string;
  Descriere?: string; // Optional
  EsteSpray: boolean;
  CodCuloare?: string; // Optional
  Imagine: Uint8Array | null; // Optional
  Pret: number;
  Valabil: boolean;
  Categorie: string;
  IdUser: number | null; 
  User: User | null;  // Mark User as nullable (null allowed)
}
