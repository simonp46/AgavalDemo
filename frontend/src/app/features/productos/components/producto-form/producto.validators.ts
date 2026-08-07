import {
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';

const INT32_MIN = -2_147_483_648;
const INT32_MAX = 2_147_483_647;

export const nonBlankValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value: unknown = control.value;

  return typeof value === 'string' && value.trim().length > 0
    ? null
    : { nonBlank: true };
};

export const integerValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value: unknown = control.value;

  if (value === null || value === '') {
    return null;
  }

  return typeof value === 'number' && Number.isInteger(value)
    ? null
    : { integer: true };
};

export const int32Validator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value: unknown = control.value;

  if (typeof value !== 'number') {
    return null;
  }

  return value >= INT32_MIN && value <= INT32_MAX
    ? null
    : { int32: true };
};

export const twoDecimalPlacesValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const value: unknown = control.value;

  if (typeof value !== 'number') {
    return null;
  }

  return /^\d+(?:\.\d{1,2})?$/.test(value.toString())
    ? null
    : { decimalPlaces: true };
};
