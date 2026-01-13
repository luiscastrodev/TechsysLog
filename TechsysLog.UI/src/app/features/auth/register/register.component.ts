import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/UserRole';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl:'./register.components.html',
  styleUrls:['./register.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush  
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerForm!: FormGroup;
  loading = false;
  submitted = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  showPassword = false;
  showConfirmPassword = false;

  private destroy$ = new Subject<void>();

  userRoles = [
    { value: 0, label: 'Cliente' },  
    { value: 1, label: 'Operador Logístico' }
  ];

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef  
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeForm(): void {
    const nameControl = new FormControl('', [Validators.required, Validators.minLength(3)]);
    const emailControl = new FormControl('', [Validators.required, Validators.email]);
    const passwordControl = new FormControl('', [Validators.required, Validators.minLength(6)]);
    const confirmPasswordControl = new FormControl('', Validators.required);
    const roleControl = new FormControl(0, Validators.required); 

    this.registerForm = this.formBuilder.group({
      name: nameControl,
      email: emailControl,
      password: passwordControl,
      confirmPassword: confirmPasswordControl,
      role: roleControl
    }, {
      validators: this.passwordMatchValidator
    });
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  get f() {
    return this.registerForm.controls;
  }

  onSubmit(): void {

    this.submitted = true;
    this.cdr.markForCheck();

    this.errorMessage = null;
    this.successMessage = null;
    this.cdr.markForCheck();

    if (this.registerForm.invalid) {
      this.cdr.markForCheck();
      return;
    }

    this.loading = true;
    this.cdr.markForCheck();

    const { confirmPassword, ...registerData } = this.registerForm.value;

    const normalizedData = {
      ...registerData,
      role: typeof registerData.role === 'string'
        ? parseInt(registerData.role, 10)
        : registerData.role
    };


    this.authService.register(normalizedData)
      .pipe(
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (response) => {
          const isSuccess = response?.success || response?.isSuccess;

          if (isSuccess) {
            this.successMessage = 'Registro realizado com sucesso! Redirecionando para login...';
            this.cdr.markForCheck();

            setTimeout(() => {
              console.log('📍 Navegando para login...');
              this.router.navigate(['/auth/login']);
            }, 2000);
          } else {
            this.loading = false;
            this.errorMessage = response?.message || 'Resposta inválida do servidor';
            this.cdr.markForCheck();
          }
        },
        error: (error) => {

          this.loading = false;

          this.errorMessage = error?.message || 'Erro desconhecido ao registrar';
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

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
    this.cdr.markForCheck();
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
