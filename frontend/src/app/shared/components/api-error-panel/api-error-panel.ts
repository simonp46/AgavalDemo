import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

import { ProblemDetails } from '../../../core/models/problem-details.model';

@Component({
  selector: 'app-api-error-panel',
  standalone: true,
  imports: [MatButtonModule],
  templateUrl: './api-error-panel.html',
  styleUrl: './api-error-panel.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApiErrorPanel {
  readonly problem = input.required<ProblemDetails>();
  readonly mostrarReintentar = input(true);
  readonly reintentar = output<void>();
}
