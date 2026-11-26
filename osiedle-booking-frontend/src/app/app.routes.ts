import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/pages/home-page/home-page';
import { SalonDetailsPageComponent } from './features/salon-details/pages/salon-details-page/salon-details-page';
import { RegisterPageComponent } from './features/auth/pages/register-page/register-page';
import {MainLayoutComponent} from './core/layouts/main-layout/main-layout';
import {AuthLayoutComponent} from './core/layouts/auth-layout/auth-layout';
import { UserFlowPageComponent } from './features/auth/pages/user-flow/user-flow';
import { LoginPageComponent } from './features/auth/pages/login-page/login-page';
import {ProfilePageComponent} from "./features/profile/pages/profile-page/profile-page";

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomePageComponent },
      { path: 'profile', component: ProfilePageComponent },
      { path: 'salons/:id', component: SalonDetailsPageComponent },
    ],
  },
  {
    path: '',
    component: AuthLayoutComponent,
    children: [
      { path: 'register', component: RegisterPageComponent },
      { path: 'user-flow', component: UserFlowPageComponent },
      { path: 'login', component: LoginPageComponent },
      {
        path: 'Authentication/confirm-email',
        loadComponent: () =>
          import('./features/auth/pages/confirm-email-page-component/confirm-email-page-component').then(
            (m) => m.ConfirmEmailPageComponent,
          ),
      },
    ],
  },
];
