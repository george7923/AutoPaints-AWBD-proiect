import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { Cos } from '../models/Cos';
import { Produs } from '../models/Produs';
import { CartDTO, SubprodusUpdateDTO, CartDetailsDTO } from '../models/NewDTOs';

@Injectable({
  providedIn: 'root',
})
export class CosService {
  private apiUrl = 'https://localhost:7160/api/cos';

  constructor(private http: HttpClient) { }

  // Get all Carts
  getAllCarts(): Observable<Cos[]> {
    return this.http.get<Cos[]>(`${this.apiUrl}`);
  }
  getCartContentByUserId(userId: number): Observable<{
    cart: any;           // Or type Cos
    products: {
      idProdus: number;
      nume: string;
      pret: number;
      categorie: string;
      esteSpray: boolean;
      codCuloare: string | null;
      imagine: any;       // could be byte[] or string
      cantitatea: number;
    }[];
  }> {
    return this.http.get<{
      cart: any;
      products: any[];
    }>(`${this.apiUrl}/cart-content/${userId}`);
  }

  // Get a specific Cart by ID
  getCartById(id: number): Observable<Cos> {
    return this.http.get<Cos>(`${this.apiUrl}/${id}`);
  }

  // GET: api/Cos/user/{userId}
  getCartByUserId(userId: number): Observable<Cos> {
    console.log(`[CosService] Fetching cart for userId=${userId}`);
    return this.http.get<Cos>(`${this.apiUrl}/user/${userId}`).pipe(
      tap((cart) => {
        console.log('[CosService] Cart received from API:', cart);
        if (!cart) {
          console.warn(`[CosService] No cart found for userId=${userId}`);
        } else if (!cart.idCos) {
          console.error(`[CosService] cart.idCos is missing! Cart:`, cart);
        } else {
          console.log(`[CosService] cart.idCos received: ${cart.idCos}`);
        }
      }),
      catchError((err) => {
        console.error(`[CosService] Error fetching cart:`, err);
        return throwError(() => err);
      })
    );
  }

  // Create a new Cart
  createCart(cos: Cos): Observable<Cos> {
    return this.http.post<Cos>(`${this.apiUrl}`, cos);
  }
  checkOrCreateCart(userId: number): Observable<Cos> {
    return this.http.post<Cos>(`${this.apiUrl}/check-or-create/${userId}`, {});
  }


  // Update an existing Cart
  updateCart(cos: Cos): Observable<Cos> {
    return this.http.put<Cos>(`${this.apiUrl}/${cos.idCos}`, cos);
  }

  // Delete a Cart by ID
  deleteCart(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Delete old Carts based on a threshold date
  deleteOldCarts(thresholdDate: Date): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/old/${thresholdDate.toISOString()}`);
  }

  // NOU: numărul de subproduse din coș
  getSubproductCount(cartId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/count/${cartId}`);
  }




  setSubprodusToCart(idProdus: number, idCos: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/add-to-cart`, {
      idProdus,
      idCos,
    });
  }



  removeOneSubproduseFromCart(idCos: number, idProdus: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/remove-one-from-cart`, {
      IdCos: idCos,
      IdProdus: idProdus,
    });
  }

  addSubproduseToCart(cartId: number, productId: number, quantity: number, userId: number): Observable<any> {
    const requestBody = {
      idCos: cartId,
      IdProdus: productId,
      Quantity: quantity,
      IdUser: userId,
    };
    return this.http.post(`${this.apiUrl}/addSubproduseToCart`, requestBody, {
      responseType: 'text',
    });
  }


  getProdusByCosId(
    idCos: number
  ): Observable<
    {
      IdSubprodus: number;
      IdProdus: number;
      Nume: string;
      Pret: number;
      Categorie: string;
      EsteSpray: boolean;
      CodCuloare?: string;
    }[]
  > {
    return this.http.get<
      {
        IdSubprodus: number;
        IdProdus: number;
        Nume: string;
        Pret: number;
        Categorie: string;
        EsteSpray: boolean;
        CodCuloare?: string;
      }[]
    >(`${this.apiUrl}/produse-in-cos/${idCos}`);
  }

  // NOU: cart details
  getCartDetails(userId: number): Observable<CartDetailsDTO> {
    return this.http.get<CartDetailsDTO>(`${this.apiUrl}/cart-details/${userId}`);
  }

  //Add One Subproduct to Cart (unificat cu CosController -> /add-one)
  addOneSubprodusToCart(request: SubprodusUpdateDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/add-one`, request);
  }

  // Remove One Subproduct from Cart (-> /remove-one)
  removeOneSubprodusFromCart(request: SubprodusUpdateDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/remove-one`, request);
  }

  // Remove All Subproducts of a Product from Cart (-> /remove-all)
  removeAllSubproduseFromCart(request: SubprodusUpdateDTO): Observable<any> {
    return this.http.put(`${this.apiUrl}/remove-all`, request);
  }


}
