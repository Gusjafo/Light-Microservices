import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { User, UserService } from '../../services/user.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html'
})
export class UsersComponent implements OnInit {
  displayedColumns = ['id', 'name', 'email', 'actions'];
  users: User[] = [];
  selectedUser?: User;
  userForm: FormGroup;
  isLoading = false;

  constructor(
    private readonly userService: UserService,
    private readonly fb: FormBuilder,
    private readonly snackBar: MatSnackBar
  ) {
    this.userForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getUsers().subscribe({
      next: users => {
        this.users = users;
        this.isLoading = false;
      },
      error: () => {
        this.snackBar.open('Error cargando usuarios', 'Cerrar', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  selectUser(user: User): void {
    this.selectedUser = user;
    this.userForm.patchValue(user);
  }

  resetForm(): void {
    this.selectedUser = undefined;
    this.userForm.reset();
  }

  submit(): void {
    if (this.userForm.invalid) {
      return;
    }

    const formValue = this.userForm.value;

    if (this.selectedUser) {
      this.userService.updateUser(this.selectedUser.id, formValue).subscribe({
        next: () => {
          this.snackBar.open('Usuario actualizado', 'Cerrar', { duration: 2000 });
          this.loadUsers();
          this.resetForm();
        },
        error: () => this.snackBar.open('Error actualizando usuario', 'Cerrar', { duration: 3000 })
      });
    } else {
      this.userService.createUser(formValue).subscribe({
        next: () => {
          this.snackBar.open('Usuario creado', 'Cerrar', { duration: 2000 });
          this.loadUsers();
          this.resetForm();
        },
        error: () => this.snackBar.open('Error creando usuario', 'Cerrar', { duration: 3000 })
      });
    }
  }

  deleteUser(user: User): void {
    if (!confirm(`¿Eliminar usuario ${user.name}?`)) {
      return;
    }

    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.snackBar.open('Usuario eliminado', 'Cerrar', { duration: 2000 });
        this.loadUsers();
      },
      error: () => this.snackBar.open('Error eliminando usuario', 'Cerrar', { duration: 3000 })
    });
  }
}
