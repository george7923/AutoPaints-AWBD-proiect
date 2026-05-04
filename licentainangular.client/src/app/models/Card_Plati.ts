import { User } from './User';

export interface Card_Plati {
  IdCard: number;   
  NumarCard: string;           
  CVV: string;                 
  DataExpirare: Date;          
  IdUser: number;             
  User: User | null;                 
}
