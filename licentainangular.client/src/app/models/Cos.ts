import { User } from './User';
export interface Cos {
  idCos: number; //Sau idCos
  CodUnic: string;
  IdUser: number;
  User: User | null;  // Ideally, replace 'any' with the actual User model
  DataCreare: Date;
}
