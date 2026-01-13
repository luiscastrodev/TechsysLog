import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';

import { API_CONFIG } from '../config/api.config';
import { User } from '../models/User';
import { LoginRequest } from '../models/LoginRequest';
import { AuthenticationResponse } from '../models/AuthenticationResponse';
import { RegisterRequest } from '../models/RegisterRequest';
import { UserRole } from '../models/UserRole';

export interface LoginError {
  message: string;
  status?: number;
  originalError?: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = API_CONFIG.baseUrl;
  private readonly currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  private readonly accessTokenSubject = new BehaviorSubject<string | null>(null);
  public accessToken$ = this.accessTokenSubject.asObservable();

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {
    this.loadStoredAuth();
  }

  /**
   * Carrega dados de autenticação armazenados no localStorage
   */
  private loadStoredAuth(): void {
    const token = this.getStoredAccessToken();
    const user = this.getStoredUser();

    if (token) {
      this.accessTokenSubject.next(token);
    }
    if (user) {
      this.currentUserSubject.next(user);
    }
  }

  /**
   * Extrai mensagem de erro do response HTTP
   */
  private extractErrorMessage(error: HttpErrorResponse): string {
    console.log('🔍 Analisando erro:', {
      status: error.status,
      statusText: error.statusText,
      body: error.error,
      type: typeof error.error
    });

    // Se há mensagem na propriedade 'message'
    if (error.error?.message) {
      return error.error.message;
    }

    // Se há array de erros
    if (error.error?.errors && Array.isArray(error.error.errors)) {
      return error.error.errors[0] || 'Erro ao processar requisição';
    }

    // Se há objeto de erros (validação)
    if (error.error?.errors && typeof error.error.errors === 'object') {
      const firstError = Object.values(error.error.errors)[0];
      if (Array.isArray(firstError)) {
        return String(firstError[0]);
      }
      return String(firstError);
    }

    // Se há body como string
    if (typeof error.error === 'string') {
      return error.error;
    }

    // Mensagens padrão por status
    switch (error.status) {
      case 0:
        return 'Erro de conexão. Verifique se o servidor está online.';
      case 400:
        return 'Email ou senha incorretos.';
      case 401:
        return 'Credenciais inválidas. Verifique seus dados.';
      case 403:
        return 'Acesso proibido.';
      case 404:
        return 'Servidor não encontrado.';
      case 500:
        return 'Erro no servidor. Tente novamente mais tarde.';
      case 503:
        return 'Serviço indisponível.';
      default:
        return `Erro ${error.status}: ${error.statusText || 'Erro desconhecido'}`;
    }
  }

  /**
   * Converte de {isSuccess, data} para {success, data}
   */
  private normalizeResponse(response: any): AuthenticationResponse {
    return {
      isSuccess: response.isSuccess || response.success,
      message: response.message,
      data: response.data
    };
  }

  /**
   * Realiza login do usuário
   */
  login(credentials: any): Observable<AuthenticationResponse> {
    const loginRequest: LoginRequest = {
      login: credentials.login || credentials.email,
      password: credentials.password
    };

    return this.http.post<any>(
      `${this.apiUrl}/auth/login`,
      loginRequest
    ).pipe(
      map(response => {
        const normalized = this.normalizeResponse(response);
        return normalized;
      }),
      tap(response => {
        if (response.isSuccess && response.data) {
          this.storeAuthData(response.data);
          this.accessTokenSubject.next(response.data.accessToken);

          // Decodificar e armazenar dados do usuário
          const user = this.decodeToken(response.data.accessToken);
          this.currentUserSubject.next(user);
          localStorage.setItem('currentUser', JSON.stringify(user));

        }
      }),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = this.extractErrorMessage(error);
        console.error('Erro HTTP capturado:', {
          status: error.status,
          message: errorMessage
        });

        const loginError: LoginError = {
          message: errorMessage,
          status: error.status,
          originalError: error
        };

        console.log('Retornando erro:', loginError);
        return throwError(() => loginError);
      })
    );
  }

  /**
   * Registra novo usuário
   */
  register(request: RegisterRequest): Observable<any> {
    // ✅ Converte role para número se for string
    const normalizedRequest = {
      ...request,
      role: typeof request.role === 'string' ? parseInt(request.role, 10) : request.role
    };


    return this.http.post<any>(
      `${this.apiUrl}/User/register`,  // ✅ User com U maiúsculo
      normalizedRequest
    ).pipe(
      map(response => {
        // ✅ Normaliza a resposta
        const normalized = this.normalizeResponse(response);
        return normalized;
      }),
      catchError((error: HttpErrorResponse) => {
        const errorMessage = this.extractErrorMessage(error);

        return throwError(() => ({
          message: errorMessage,
          status: error.status,
          originalError: error
        } as LoginError));
      })
    );
  }

  /**
   * Faz logout do usuário
   */
  logout(): Observable<any> {
    const refreshToken = this.getStoredRefreshToken();

    if (!refreshToken) {
      this.clearAuthData();
      return new Observable(observer => {
        observer.next(null);
        observer.complete();
      });
    }

    return this.http.post(`${this.apiUrl}/auth/logout`, { token: refreshToken }).pipe(
      tap(() => {
        this.clearAuthData();
        console.log('✅ Logout bem-sucedido');
      }),
      catchError((error: HttpErrorResponse) => {
        this.clearAuthData();
        console.error('❌ Logout error:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Atualiza o token de acesso usando o refresh token
   */
  refreshAccessToken(): Observable<AuthenticationResponse> {
    const refreshToken = this.getStoredRefreshToken();

    if (!refreshToken) {
      return throwError(() => ({
        message: 'Refresh token não encontrado',
        status: 401
      } as LoginError));
    }

    return this.http.post<any>(
      `${this.apiUrl}/auth/refresh-token`,
      { token: refreshToken }
    ).pipe(
      map(response => this.normalizeResponse(response)),
      tap(response => {
        if (response.isSuccess && response.data) {
          this.storeAuthData(response.data);
          this.accessTokenSubject.next(response.data.accessToken);
          console.log('✅ Token atualizado');
        }
      }),
      catchError((error: HttpErrorResponse) => {
        this.clearAuthData();
        console.error('❌ Token refresh error:', error);
        return throwError(() => ({
          message: this.extractErrorMessage(error),
          status: error.status,
          originalError: error
        } as LoginError));
      })
    );
  }

  /**
   * Decodifica JWT token para extrair dados do usuário
   */
  private decodeToken(token: string): User {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      const decoded = JSON.parse(jsonPayload);

      return {
        id: decoded.sub || decoded.nameid,
        name: decoded.name || 'Usuário',
        email: decoded.email,
        role: decoded.role as UserRole,
        failedLoginAttempts: 0
      };
    } catch (error) {
      console.error('❌ Erro ao decodificar token:', error);
      return {
        id: '',
        name: 'Usuário',
        email: '',
        role: UserRole.User,
        failedLoginAttempts: 0
      };
    }
  }

  /**
   * Armazena dados de autenticação
   */
  private storeAuthData(data: any): void {
    try {
      console.log('💾 Armazenando dados de autenticação...');
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      localStorage.setItem('refreshTokenExpiresAt', data.refreshTokenExpiresAt);
      localStorage.setItem('userId', data.userId);
      console.log('✅ Dados armazenados com sucesso');
    } catch (error) {
      console.error('❌ Erro ao armazenar dados:', error);
    }
  }

  /**
   * Limpa dados de autenticação
   */
  private clearAuthData(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('refreshTokenExpiresAt');
    localStorage.removeItem('userId');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.accessTokenSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  // ========== GETTERS ==========

  getStoredAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getStoredRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  getUserId(): string | null {
    return localStorage.getItem('userId');
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getStoredAccessToken() && !!this.getCurrentUser();
  }

  getUserRole(): UserRole | null {
    return this.currentUserSubject.value?.role || null;
  }

  isOperator(): boolean {
    const role = this.getUserRole();
    return role === UserRole.Operator || role === UserRole.Admin;
  }

  private getStoredUser(): User | null {
    try {
      const stored = localStorage.getItem('currentUser');
      return stored ? JSON.parse(stored) : null;
    } catch (error) {
      console.error('❌ Erro ao recuperar usuário:', error);
      return null;
    }
  }
}
