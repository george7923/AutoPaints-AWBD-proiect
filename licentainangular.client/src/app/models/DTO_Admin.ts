// src/app/models/DTO_Admin.ts

import { CategoryDTO } from './NewDTOs';

/**
 * Extinde CategoryDTO cu idCategorie,
 * pentru a fi folosit în UI-ul de administrare.
 */
export interface AdminCategoryDTO extends CategoryDTO {
  idCategorie: number;
}
// src/app/models/DTO_Admin.ts

export interface AdminProductDTO {
  idProdus: number;
  nume: string;
  descriere: string;
  esteSpray: boolean;
  valabil: boolean;
  categorie: string;
  pret: number;
  codCuloare?: string;
  imagine?: any;
}

export interface AdminProductUploadDTO {
  Nume: string;
  Descriere: string;
  EsteSpray: boolean;
  Valabil: boolean;
  Categorie: string;
  CodCuloare?: string;
  Pret: number;
  IdUser: number;
  Imagine?: File;
}

export interface AdminSubprodusDTO {
  idSubprodus: number;
  idProdus: number;
  valabil: boolean;
  idCos: number | null;
}

export interface AdminVopseaDTO {
  IdUser: number;
  tipVopsea: string;
  marcaMasinii: string;
  codCuloare: string;
  model: string;
  an: string;
  serieCaroserie: string;
  detaliiSuplimentare?: string;
  cantitate: number;
}
// src/app/models/DTO_Admin.ts
export interface ProductAdminDTO {
  idProdus: number;
  nume: string;
  descriere?: string;
  esteSpray: boolean;
  valabil: boolean;
  idCategorie: number;
  categorie: {
    idCategorie: number;
    denumireCategorie: string;
    descriereCategorie: string;
  };
  idUser: number;
}

export interface SubprodusAdminDTO {
  idSubprodus: number;
  idProdus: number;
  valabil: boolean;
  idCos?: number;
}

export interface VopseaCreateDTO {
  idUser: number;
  tipVopsea: string;
  marcaMasinii: string;
  codCuloare: string;
  model: string;
  an: string;
  serieCaroserie: string;
  detaliiSuplimentare?: string;
  cantitate: number;
}

export interface VopseaResponseAdminDTO {
  message: string;
  idProdus: number;
  idVopsea: number;
  idCos: number;
  subproduseCreateInCos: number;
}
// DTO specific pentru administrație – produsele din ProductService
export interface AdminProdusDTO {
  ProdusId: number;
  Nume: string;
  Descriere?: string;
  EsteSprayText: string;
  Imagine: string | null;
  Pret: number;
  Valabil: boolean;
  Categorie: string;
  Vendor: string;
  IdUser?: number;
}

// Pentru crearea unui produs (fără ProdusId)
export interface AdminNewProdusDTO {
  Nume: string;
  Descriere?: string;
  EsteSprayText: string;
  Imagine?: File | null;
  Pret: number;
  Valabil: boolean;
  Categorie: string;
  Vendor?: string;
  IdUser?: number;
}

// Pentru actualizarea unui produs
export interface AdminUpdateProdusDTO {
  ProdusId: number;
  Nume?: string;
  Descriere?: string;
  EsteSprayText?: string;
  Imagine?: File | null;
  Pret?: number;
  Valabil?: boolean;
  Categorie?: string;
  Vendor?: string;
  IdUser?: number;
}


export interface AdminUpdateProdusDTO {
  idProdus?: number;
  nume?: string;
  descriere?: string;
  esteSpray?: boolean;
  valabil?: boolean;
  denumireCategorie?: string; // trimite lower-case!
  idUser?: number;
  imagineBase64?: string | null;
  cantitate?: number;
}
interface PretProdus {
  pret: number;
  dataExpirare: string | null;
  // ...alte câmpuri dacă ai
}
