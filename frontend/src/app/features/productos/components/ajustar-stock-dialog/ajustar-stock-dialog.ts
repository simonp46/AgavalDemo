import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import {
  AjustarStockRequest,
  ProductoResponse,
  TipoAjusteStock,
} from '../../models/producto.models';
import {
  int32Validator,
  integerValidator,
} from '../producto-form/producto.validators';

interface AjustarStockFormControls {
  tipo: FormControl<TipoAjusteStock>;
  cantidad: FormControl<number | null>;
}

@Component({
  selector: 'app-ajustar-stock-dialog',
  standalone: true,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
  ],
  templateUrl: './ajustar-stock-dialog.html',
  styleUrl: './ajustar-stock-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AjustarStockDialog {
  private readonly dialogRef = inject<
    MatDialogRef<AjustarStockDialog, AjustarStockRequest>
  >(MatDialogRef);

  protected readonly producto = inject<ProductoResponse>(MAT_DIALOG_DATA);
  protected readonly form = new FormGroup<AjustarStockFormControls>({
    tipo: new FormControl<TipoAjusteStock>('ENTRADA', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    cantidad: new FormControl<number | null>(null, {
      validators: [
        Validators.required,
        Validators.min(1),
        integerValidator,
        int32Validator,
      ],
    }),
  });

  protected confirmar(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();

    if (value.cantidad === null) {
      return;
    }

    this.dialogRef.close({
      tipo: value.tipo,
      cantidad: value.cantidad,
    });
  }
}
