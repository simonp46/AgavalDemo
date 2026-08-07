import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
} from '@angular/material/dialog';

import { ProductoResponse } from '../../models/producto.models';

@Component({
  selector: 'app-confirmar-eliminacion-dialog',
  standalone: true,
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './confirmar-eliminacion-dialog.html',
  styleUrl: './confirmar-eliminacion-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmarEliminacionDialog {
  protected readonly producto = inject<ProductoResponse>(MAT_DIALOG_DATA);
}
