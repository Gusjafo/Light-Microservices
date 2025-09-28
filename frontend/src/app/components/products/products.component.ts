import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Product, ProductService } from '../../services/product.service';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  displayedColumns = ['id', 'name', 'price', 'stock', 'actions'];
  products: Product[] = [];
  selectedProduct?: Product;
  productForm: FormGroup;
  isLoading = false;

  constructor(
    private readonly productService: ProductService,
    private readonly fb: FormBuilder,
    private readonly snackBar: MatSnackBar
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      price: [0, [Validators.required, Validators.min(0)]],
      stock: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts().subscribe({
      next: products => {
        this.products = products;
        this.isLoading = false;
      },
      error: () => {
        this.snackBar.open('Error cargando productos', 'Cerrar', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  selectProduct(product: Product): void {
    this.selectedProduct = product;
    this.productForm.patchValue({
      name: product.name,
      price: product.price,
      stock: product.stock
    });
  }

  resetForm(): void {
    this.selectedProduct = undefined;
    this.productForm.reset({ price: 0, stock: 0 });
  }

  submit(): void {
    if (this.productForm.invalid) {
      return;
    }

    const formValue = this.productForm.value;

    if (this.selectedProduct) {
      this.productService.updateProduct(this.selectedProduct.id, formValue).subscribe({
        next: () => {
          this.snackBar.open('Producto actualizado', 'Cerrar', { duration: 2000 });
          this.loadProducts();
          this.resetForm();
        },
        error: () => this.snackBar.open('Error actualizando producto', 'Cerrar', { duration: 3000 })
      });
    } else {
      this.productService.createProduct(formValue).subscribe({
        next: () => {
          this.snackBar.open('Producto creado', 'Cerrar', { duration: 2000 });
          this.loadProducts();
          this.resetForm();
        },
        error: () => this.snackBar.open('Error creando producto', 'Cerrar', { duration: 3000 })
      });
    }
  }

  deleteProduct(product: Product): void {
    if (!confirm(`¿Eliminar producto ${product.name}?`)) {
      return;
    }

    this.productService.deleteProduct(product.id).subscribe({
      next: () => {
        this.snackBar.open('Producto eliminado', 'Cerrar', { duration: 2000 });
        this.loadProducts();
      },
      error: () => this.snackBar.open('Error eliminando producto', 'Cerrar', { duration: 3000 })
    });
  }
}
