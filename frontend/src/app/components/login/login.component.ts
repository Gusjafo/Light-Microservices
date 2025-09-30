import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  isSubmitting = false;
  readonly form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const { email, password } = this.form.value;

    this.authService
      .login({ email: email ?? '', password: password ?? '' })
      .subscribe({
        next: () => {
          this.snackBar.open('Inicio de sesión exitoso', undefined, { duration: 2500 });
          this.router.navigate(['/users']);
        },
        error: () => {
          this.snackBar.open('Credenciales inválidas. Inténtalo nuevamente.', undefined, { duration: 3500 });
          this.isSubmitting = false;
        },
        complete: () => {
          this.isSubmitting = false;
        }
      });
  }
}
