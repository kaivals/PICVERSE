import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Guards
import { AuthGuard } from './core/guards/auth.guard';

// Components
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ProfileComponent } from './features/profile/profile.component';
import { ChatComponent } from './features/chat/chat.component';
import { CreatePostComponent } from './features/posts/create-post/create-post.component';
import { PostDetailComponent } from './features/posts/post-detail/post-detail.component';
import { UserSearchComponent } from './features/users/user-search/user-search.component';
import { SettingsComponent } from './features/settings/settings.component';

const routes: Routes = [
  // Public routes
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  
  // Protected routes
  { 
    path: 'dashboard', 
    component: DashboardComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'profile/:id', 
    component: ProfileComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'profile', 
    component: ProfileComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'chat', 
    component: ChatComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'chat/:id', 
    component: ChatComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'create-post', 
    component: CreatePostComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'post/:id', 
    component: PostDetailComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'search', 
    component: UserSearchComponent, 
    canActivate: [AuthGuard] 
  },
  { 
    path: 'settings', 
    component: SettingsComponent, 
    canActivate: [AuthGuard] 
  },
  
  // Wildcard route
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }