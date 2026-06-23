import { Persoana } from '../models/Persoana';
export interface User {
  IdUser: number;              
  Username: string;            
  Password: string;       
  IdPersoana: number;     
  IdAdresa: number;       
  Persoana: Persoana;         
}
