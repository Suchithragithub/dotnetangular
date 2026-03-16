import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'change-password',
    loadComponent: () => import('./features/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pincode-verification',
    loadComponent: () => import('./features/pincode-verification/pincode-verification.component').then(m => m.PincodeVerificationComponent),
    canActivate: [authGuard]
  },
  {
    path: 'enroll',
    loadComponent: () => import('./features/enroll/enroll.component').then(m => m.EnrollComponent),
    canActivate: [authGuard]
  },
  {
    path: 'enroll-history',
    loadComponent: () => import('./features/enroll-history/enroll-history.component').then(m => m.EnrollHistoryComponent),
    canActivate: [authGuard]
  },
  {
    path: 'my-profile',
    loadComponent: () => import('./features/my-profile/my-profile.component').then(m => m.MyProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: 'print',
    loadComponent: () => import('./features/print/print.component').then(m => m.PrintComponent),
    canActivate: [authGuard]
  },
  {
    path: 'admin/login',
    loadComponent: () => import('./features/admin/login/admin/login.component').then(m => m.AdminLoginComponent)
  },
  {
    path: 'admin/change-password',
    loadComponent: () => import('./features/admin/change-password/admin-change-password.component').then(m => m.AdminChangePasswordComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/session',
    loadComponent: () => import('./features/admin/session/admin/session.component').then(m => m.AdminSessionComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/semester',
    loadComponent: () => import('./features/admin/semester/admin/semester.component').then(m => m.AdminSemesterComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/department',
    loadComponent: () => import('./features/admin/department/admin/department.component').then(m => m.AdminDepartmentComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/level',
    loadComponent: () => import('./features/admin/level/admin/level.component').then(m => m.AdminLevelComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/course',
    loadComponent: () => import('./features/admin/course/admin/course.component').then(m => m.AdminCourseComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/edit-course',
    loadComponent: () => import('./features/admin/edit-course/admin/edit-course.component').then(m => m.EditCourseComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/student-registration',
    loadComponent: () => import('./features/admin/student-registration/admin/student-registration.component').then(m => m.StudentRegistrationComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/manage-students',
    loadComponent: () => import('./features/admin/manage-students/admin/manage-students.component').then(m => m.ManageStudentsComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/edit-student-profile',
    loadComponent: () => import('./features/admin/edit-student-profile/admin/edit-student-profile.component').then(m => m.AdminEditStudentProfileComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/enroll-history',
    loadComponent: () => import('./features/admin/enroll-history/admin/enroll-history.component').then(m => m.AdminEnrollHistoryComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/print',
    loadComponent: () => import('./features/admin/print/admin/print.component').then(m => m.AdminPrintComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'admin/user-log',
    loadComponent: () => import('./features/admin/user-log/admin/user-log.component').then(m => m.AdminUserLogComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];