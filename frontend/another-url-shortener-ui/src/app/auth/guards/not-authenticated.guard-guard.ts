import type { CanMatchFn } from '@angular/router';

export const notAuthenticatedGuardGuard: CanMatchFn = (route, segments) => {
  return true;
};
