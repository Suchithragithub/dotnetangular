// import { Component, OnInit, signal, inject } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { RouterModule } from '@angular/router';
// import {
//   EnrollmentDetails,
//   EnrollmentService
// } from '../../core/services/enrollment.service';
// import { AuthService } from '../../core/services/auth.service';
// import { Student } from '../../core/models/student.model';

// interface EnrollmentHistoryItem {
//   id: number;
//   courseCode: string;
//   courseName: string;
//   courseUnits: number;
//   sessionName: string;
//   departmentName: string;
//   levelName: string;
//   semesterName: string;
//   enrolledDate?: Date;
// }

// @Component({
//   selector: 'app-enroll-history',
//   standalone: true,
//   imports: [CommonModule, RouterModule],
//   templateUrl: './enroll-history.component.html',
//   styleUrls: ['./enroll-history.component.scss']
// })
// export class EnrollHistoryComponent implements OnInit {
//   private enrollmentService = inject(EnrollmentService);
//   private authService = inject(AuthService);

//   loading = signal<boolean>(false);
//   error = signal<string | null>(null);
//   enrollments = signal<EnrollmentHistoryItem[]>([]);
//   studentInfo = signal<Student | null>(null);
//   groupedEnrollments = signal<Map<string, EnrollmentHistoryItem[]>>(new Map());

//   ngOnInit(): void {
//     this.loadEnrollmentHistory();
//   }

//   private loadEnrollmentHistory(): void {
//     this.loading.set(true);
//     this.error.set(null);

//     const currentUser = this.authService.getCurrentUser();
//     if (!currentUser || !currentUser.regno) {
//       this.error.set('Unable to identify logged-in student. Please log in again.');
//       this.loading.set(false);
//       return;
//     }

//     const regno = currentUser.regno;

//     this.enrollmentService.getStudentEnrollments(regno).subscribe({
//       next: (enrollments: EnrollmentDetails[]) => {
//         const historyItems = this.mapEnrollmentsToHistory(enrollments);
//         this.enrollments.set(historyItems);
//         this.groupEnrollmentsBySession(historyItems);
//         this.loading.set(false);
//       },
//       error: (err) => {
//         console.error('Error loading enrollment history:', err);
//         this.error.set('Failed to load enrollment history. Please try again later.');
//         this.loading.set(false);
//       }
//     });
//   }

//   private mapEnrollmentsToHistory(enrollments: EnrollmentDetails[]): EnrollmentHistoryItem[] {
//     return enrollments.map(enrollment => ({
//       id: 0,                                          // not in API response
//       courseCode: enrollment.courseId?.toString() || 'N/A',  // no courseCode in API, use courseId
//       courseName: enrollment.courseName || 'Unknown Course',
//       courseUnits: 0,                                 // not in API response
//       sessionName: enrollment.sessionName || 'N/A',
//       departmentName: enrollment.departmentName || 'N/A',
//       levelName: 'N/A',                               // not in API response
//       semesterName: enrollment.semesterName || 'N/A',
//       enrolledDate: enrollment.enrollDate ? new Date(enrollment.enrollDate) : undefined
//     }));
//   }

//   private groupEnrollmentsBySession(enrollments: EnrollmentHistoryItem[]): void {
//     const grouped = new Map<string, EnrollmentHistoryItem[]>();
    
//     enrollments.forEach(enrollment => {
//       const key = `${enrollment.sessionName} - ${enrollment.semesterName}`;
//       if (!grouped.has(key)) {
//         grouped.set(key, []);
//       }
//       grouped.get(key)!.push(enrollment);
//     });

//     // Sort groups by session/semester (most recent first)
//     const sortedGroups = new Map(
//       Array.from(grouped.entries()).sort((a, b) => b[0].localeCompare(a[0]))
//     );

//     this.groupedEnrollments.set(sortedGroups);
//   }

//   getTotalUnits(enrollments: EnrollmentHistoryItem[]): number {
//     return enrollments.reduce((sum, e) => sum + e.courseUnits, 0);
//   }

//   getGroupKeys(): string[] {
//     return Array.from(this.groupedEnrollments().keys());
//   }

//   getEnrollmentsByGroup(key: string): EnrollmentHistoryItem[] {
//     return this.groupedEnrollments().get(key) || [];
//   }

//   printHistory(): void {
//     window.print();
//   }

//   retry(): void {
//     this.loadEnrollmentHistory();
//   }
// }


import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import {
  EnrollmentDetails,
  EnrollmentService
} from '../../core/services/enrollment.service';
import { AuthService } from '../../core/services/auth.service';
import { Student } from '../../core/models/student.model';
 
interface EnrollmentHistoryItem {
  id: number;
  courseCode: string;
  courseName: string;
  courseUnits: number;
  sessionName: string;
  departmentName: string;
  levelName: string;
  semesterName: string;
  enrolledDate?: Date;
}
 
@Component({
  selector: 'app-enroll-history',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './enroll-history.component.html',
  styleUrls: ['./enroll-history.component.scss']
})
export class EnrollHistoryComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private authService = inject(AuthService);
 
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  enrollments = signal<EnrollmentHistoryItem[]>([]);
  studentInfo = signal<Student | null>(null);
  groupedEnrollments = signal<Map<string, EnrollmentHistoryItem[]>>(new Map());
 
  ngOnInit(): void {
    this.loadEnrollmentHistory();
  }
 
  private loadEnrollmentHistory(): void {
    this.loading.set(true);
    this.error.set(null);
 
    const currentUser = this.authService.getCurrentUser();
 
    // FIX: use studentRegno instead of regno
    if (!currentUser || !currentUser.studentRegno) {
      this.error.set('Unable to identify logged-in student. Please log in again.');
      this.loading.set(false);
      return;
    }
 
    const regno = currentUser.studentRegno; // FIX: was currentUser.regno
 
    this.enrollmentService.getStudentEnrollments(regno).subscribe({
      next: (enrollments: EnrollmentDetails[]) => {
        const historyItems = this.mapEnrollmentsToHistory(enrollments);
        this.enrollments.set(historyItems);
        this.groupEnrollmentsBySession(historyItems);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading enrollment history:', err);
        this.error.set('Failed to load enrollment history. Please try again later.');
        this.loading.set(false);
      }
    });
  }
 

  private mapEnrollmentsToHistory(enrollments: EnrollmentDetails[]): EnrollmentHistoryItem[] {
    return enrollments.map(enrollment => ({
      id: 0,                                          // not in API response
      courseCode: enrollment.courseId?.toString() || 'N/A',  // no courseCode in API, use courseId
      courseName: enrollment.courseName || 'Unknown Course',
      courseUnits: 0,                                 // not in API response
      sessionName: enrollment.sessionName || 'N/A',
      departmentName: enrollment.departmentName || 'N/A',
      levelName: 'N/A',                               // not in API response
      semesterName: enrollment.semesterName || 'N/A',
      enrolledDate: enrollment.enrollDate ? new Date(enrollment.enrollDate) : undefined
    }));
  }
 
  private groupEnrollmentsBySession(enrollments: EnrollmentHistoryItem[]): void {
    const grouped = new Map<string, EnrollmentHistoryItem[]>();
   
    enrollments.forEach(enrollment => {
      const key = `${enrollment.sessionName} - ${enrollment.semesterName}`;
      if (!grouped.has(key)) {
        grouped.set(key, []);
      }
      grouped.get(key)!.push(enrollment);
    });
 
    // Sort groups by session/semester (most recent first)
    const sortedGroups = new Map(
      Array.from(grouped.entries()).sort((a, b) => b[0].localeCompare(a[0]))
    );
 
    this.groupedEnrollments.set(sortedGroups);
  }
 
  getTotalUnits(enrollments: EnrollmentHistoryItem[]): number {
    return enrollments.reduce((sum, e) => sum + e.courseUnits, 0);
  }
 
  getGroupKeys(): string[] {
    return Array.from(this.groupedEnrollments().keys());
  }
 
  getEnrollmentsByGroup(key: string): EnrollmentHistoryItem[] {
    return this.groupedEnrollments().get(key) || [];
  }
 
  printHistory(): void {
    window.print();
  }
 
  retry(): void {
    this.loadEnrollmentHistory();
  }
}
 
 