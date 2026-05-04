export interface ExtendedProdus {
  IdProdus: number;
  Nume: string;
  Pret: number;
  Categorie: string;
  EsteSpray: boolean;
  CodCuloare: string;
  Imagine: string; // Base64 sau fallback image path
  Cantitatea: number;
  Valabil?: boolean; // ✅ Marcat ca opțional
  IdUser?: number; // ✅ Marcat ca opțional
  User?: any; // ✅ Marcat ca opțional
}
