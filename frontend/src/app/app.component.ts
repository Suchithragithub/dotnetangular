import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'ModernApiProject';
  authService = inject(AuthService);
  router = inject(Router);
  // isLoggedIn$ = this.authService.isLoggedIn$;
  // isAdmin$ = this.authService.isAdmin$;

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/Admin/login']);
  }
}