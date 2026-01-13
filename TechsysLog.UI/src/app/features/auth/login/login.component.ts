import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html', 
  styleUrls: ['./login.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  loading = false;
  submitted = false;
  errorMessage: string | null = null;
  showPassword = false;

  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.initializeForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    const loginControl = new FormControl('', [Validators.required, Validators.email]);
    const passwordControl = new FormControl('', [Validators.required, Validators.minLength(6)]);

    this.loginForm = this.formBuilder.group({
      login: loginControl,
      password: passwordControl
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    this.submitted = true;
    this.cdr.markForCheck();

    if (this.loginForm.invalid) {
      this.cdr.markForCheck();
      return;
    }

    this.loading = true;
    this.errorMessage = null;
    this.cdr.markForCheck();

    const credentials = this.loginForm.value;

    this.authService.login(credentials)
      .pipe(
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (response) => {
          const isSuccess = response?.isSuccess ;
          const hasAccessToken = response?.data?.accessToken;

          if (isSuccess && hasAccessToken) {

            this.notificationService.startSignalRConnection(response.data.accessToken)
              .then(() => console.log(' SignalR OK'))
              .catch((err) => console.warn(' SignalR falhou:', err));

            this.router.navigate(['/dashboard']);
          } else {
            this.loading = false;
            this.errorMessage = 'Resposta inválida do servidor';
            this.cdr.markForCheck();
          }
        },
        error: (error) => {
          this.loading = false;
          this.submitted = false;
          this.errorMessage = error?.message || 'Erro desconhecido ao fazer login';

          this.cdr.markForCheck();
        },
        complete: () => {
        }
      });
  }

  clearError(): void {
    this.errorMessage = null;
    this.cdr.markForCheck();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
    this.cdr.markForCheck();
  }

  goToRegister(): void {
    this.router.navigate(['/auth/register']);
  }
}
