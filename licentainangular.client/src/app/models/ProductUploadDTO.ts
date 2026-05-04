export interface ProductUploadDTO {
  nume: string;
  descriere: string;
  esteSpray: boolean;
  imagine?: string | null;
  pret: number;
  valabil: boolean;
  categorie: string;
  idUser: number | null;
}
