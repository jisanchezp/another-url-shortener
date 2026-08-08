import type { CanActivateFn } from '@angular/router';

export const notAuthenticatedGuard: CanActivateFn = (route, state) => {
  return true;
};
