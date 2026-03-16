import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  appTitle = 'ModernApiProject';
  isCollapsed = true;

  navLinks = [
    { name: 'Student Login', route: '/login', requiresAuth: false, isAdmin: false },
    { name: 'Change Password', route: '/change-password', requiresAuth: true, isAdmin: false },
    { name: 'Pincode Verification', route: '/pincode-verification', requiresAuth: true, isAdmin: false },
    { name: 'Course Enroll', route: '/enroll', requiresAuth: true, isAdmin: false },
    { name: 'Enroll History', route: '/enroll-history', requiresAuth: true, isAdmin: false },
    { name: 'My Profile', route: '/my-profile', requiresAuth: true, isAdmin: false },
    { name: 'Print Enrollment', route: '/print', requiresAuth: true, isAdmin: false }
  ];

  get isAuthenticated(): boolean {
    return this.authService.isLoggedIn();
  }

  get visibleLinks() {
    return this.navLinks.filter(link => {
      if (link.requiresAuth && !this.isAuthenticated) {
        return false;
      }
      if (!link.requiresAuth && this.isAuthenticated) {
        return false;
      }
      return !link.isAdmin;
    });
  }

  toggleNavbar(): void {
    this.isCollapsed = !this.isCollapsed;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}