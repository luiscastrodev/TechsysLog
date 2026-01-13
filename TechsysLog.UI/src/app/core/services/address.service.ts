import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { API_CONFIG } from '../config/api.config';

export interface Address {
  id: string;
  zipCode: string;
  street: string;
  number: string;
  neighborhood: string;
  city: string;
  state: string;
  createdAt: string;
  updatedAt: string;
  deleted: boolean;
}

export interface AddressResponse {
  zipCode: string;
  street: string;
  number: string;
  neighborhood: string;
  city: string;
  state: string;
}

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private readonly apiUrl =  API_CONFIG.baseUrl + '/address';

  constructor(private http: HttpClient) {}

  /**
   * Busca endereço pelo CEP
   * @param zipcode Deve ser formatado como: 13050-740 ou 13050740
   */
  searchByZipcode(zipcode: string): Observable<Address> {
    // Remove formatação se existir
    const cleanZipcode = zipcode.replace(/\D/g, '');

    console.log('🔍 Buscando CEP:', cleanZipcode);

    return this.http.get<Address>(
      `${this.apiUrl}/search-zipcode?zipcode=${cleanZipcode}`
    )
      .pipe(
        tap(address => {
          console.log('✅ Endereço encontrado:', address);
        }),
        catchError(error => {
          if (error.status === 204) {
            console.warn('⚠️ CEP não encontrado');
            return throwError(() => new Error('CEP não encontrado'));
          }
          console.error('❌ Erro ao buscar CEP:', error);
          return throwError(() => new Error('Erro ao buscar CEP'));
        })
      );
  }

  /**
   * Formata CEP para padrão brasileiro (XXXXX-XXX)
   */
  formatZipcode(zipcode: string): string {
    const clean = zipcode.replace(/\D/g, '');
    return clean.replace(/(\d{5})(\d{3})/, '$1-$2');
  }

  /**
   * Converte Address para AddressResponse
   */
  addressToResponse(address: Address): AddressResponse {
    return {
      zipCode: address.zipCode,
      street: address.street,
      number: address.number,
      neighborhood: address.neighborhood,
      city: address.city,
      state: address.state
    };
  }
}
