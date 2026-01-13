import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { appConfig } from '../../app.config';
import { API_CONFIG } from '../config/api.config';

export interface User {
  id: string;
  name: string;
  email: string;
  active: boolean;
}

export interface UserResponse {
  isSuccess: boolean;
  message: string;
  data: User[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl =  API_CONFIG.baseUrl + '/user/all';
  private cachedUsers$ = new BehaviorSubject<User[]>([]);

  constructor(private http: HttpClient) {}

  /**
   * Carrega todos os usuários da API
   */
  getAllUsers(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.apiUrl}`)
      .pipe(
        tap(response => {
          if (response.isSuccess && response.data) {
            this.cachedUsers$.next(response.data);
          }
        })
      );
  }

  /**
   * Retorna usuários em cache
   */
  getCachedUsers(): User[] {
    return this.cachedUsers$.value;
  }

  /**
   * Observable dos usuários em cache
   */
  getUsers$(): Observable<User[]> {
    return this.cachedUsers$.asObservable();
  }

  /**
   * Busca um usuário por ID
   */
  getUserById(id: string): User | null {
    return this.cachedUsers$.value.find(u => u.id === id) || null;
  }
}
