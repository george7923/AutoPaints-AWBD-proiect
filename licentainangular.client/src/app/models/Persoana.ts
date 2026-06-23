export interface Persoana {
  IdPersoana: number;
  Nume: string;
  Prenume: string;
  Email: string;
  tipPersoana: string; // "Fizica" sau "Juridica"
  Telefon?: string;
  rol?: "Owner" | "Administrator" | "Participant"; // New field for role ion
}
