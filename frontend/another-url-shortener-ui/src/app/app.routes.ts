import { Routes } from '@angular/router';
import { HomePage } from './home/pages/home-page/home-page';
import { LoginPage } from './auth/pages/login-page/login-page';
import { SignupPage } from './auth/pages/signup-page/signup-page';
import { UrlListPage } from './urls/pages/url-list-page/url-list-page';
import { UrlCreatePage } from './urls/pages/url-create-page/url-create-page';
import { UrlStatsPage } from './urls/pages/url-stats-page/url-stats-page';
import { authGuard } from './auth/guards/auth-guard';
import { notAuthenticatedGuard as notAuthenticatedGuard } from './auth/guards/not-authenticated-guard';

export const routes: Routes = [
  { path: '', component: HomePage },

  { path: 'login', component: LoginPage, canActivate: [notAuthenticatedGuard] },
  { path: 'signup', component: SignupPage, canActivate: [notAuthenticatedGuard] },

  { path: 'urls', component: UrlListPage, canActivate: [authGuard] },
  { path: 'urls/create', component: UrlCreatePage, canActivate: [authGuard] },
  { path: 'urls/:id/stats', component: UrlStatsPage, canActivate: [authGuard] },

  { path: '**', redirectTo: '' }
];
