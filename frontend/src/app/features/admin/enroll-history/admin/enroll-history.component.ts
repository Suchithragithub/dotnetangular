import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { inject } from '@angular/core';
import { EnrollmentDetails, EnrollmentService } from '../../../../core/services/enrollment.service';

@Component({
  selector: 'app-admin-enroll-history',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './enroll-history.component.html',
  styleUrls: ['./enroll-history.component.scss']
})
export class AdminEnrollHistoryComponent implements OnInit {
  private enrollmentService = inject(EnrollmentService);

  // enrollments = signal<CourseEnroll[]>([]);
  // filteredEnrollments = signal<CourseEnroll[]>([]);
  enrollments = signal<EnrollmentDetails[]>([]);
  filteredEnrollments = signal<EnrollmentDetails[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  searchTerm = signal('');
  filterSession = signal('');
  filterDepartment = signal('');
  filterLevel = signal('');
  filterSemester = signal('');

  uniqueSessions = signal<string[]>([]);
  uniqueDepartments = signal<string[]>([]);
  uniqueLevels = signal<string[]>([]);
  uniqueSemesters = signal<string[]>([]);

  currentPage = signal(1);
  itemsPerPage = 20;
  totalPages = signal(1);

  ngOnInit(): void {
    this.loadEnrollments();
  }

  loadEnrollments(): void {
    this.loading.set(true);
    this.error.set(null);

    this.enrollmentService.getAllEnrollments().subscribe({
      next: (data: EnrollmentDetails[]) => {

        
        console.log("🔥 FULL API RESPONSE:", data);

        this.enrollments.set(data);
        this.filteredEnrollments.set(data);
        this.extractUniqueFilters(data);
        this.calculateTotalPages();
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load enrollment records. Please try again.');
        this.loading.set(false);
        console.error('Error loading enrollments:', err);
      }
    });
  }

  extractUniqueFilters(enrollments: EnrollmentDetails[]): void {
    const sessions = new Set<string>();
    const departments = new Set<string>();
    const levels = new Set<string>();
    const semesters = new Set<string>();

    enrollments.forEach(enrollment => {
      if (enrollment.sessionName) sessions.add(enrollment.sessionName);
      if (enrollment.departmentName) departments.add(enrollment.departmentName);
      // if (enrollment.levelName) levels.add(enrollment.levelName);
      if (enrollment.semesterName) semesters.add(enrollment.semesterName);
    });

    this.uniqueSessions.set(Array.from(sessions).sort());
    this.uniqueDepartments.set(Array.from(departments).sort());
    // this.uniqueLevels.set(Array.from(levels).sort());
    this.uniqueSemesters.set(Array.from(semesters).sort());
  }

  onSearchChange(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.applyFilters();
  }

  onSessionFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterSession.set(value);
    this.applyFilters();
  }

  onDepartmentFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterDepartment.set(value);
    this.applyFilters();
  }

  onLevelFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterLevel.set(value);
    this.applyFilters();
  }

  onSemesterFilterChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.filterSemester.set(value);
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = this.enrollments();

    const search = this.searchTerm().toLowerCase();
    if (search) {
      filtered = filtered.filter(enrollment =>
        enrollment.studentRegno?.toLowerCase().includes(search) ||
        enrollment.studentName?.toLowerCase().includes(search) ||
        // enrollment.courseCode?.toLowerCase().includes(search) ||
        enrollment.courseName?.toLowerCase().includes(search)
      );
    }

    if (this.filterSession()) {
      filtered = filtered.filter(enrollment => enrollment.sessionName === this.filterSession());
    }

    if (this.filterDepartment()) {
      filtered = filtered.filter(enrollment => enrollment.departmentName === this.filterDepartment());
    }

    // if (this.filterLevel()) {
    //   filtered = filtered.filter(enrollment => enrollment.levelName === this.filterLevel());
    // }

    if (this.filterSemester()) {
      filtered = filtered.filter(enrollment => enrollment.semesterName === this.filterSemester());
    }

    this.filteredEnrollments.set(filtered);
    this.currentPage.set(1);
    this.calculateTotalPages();
  }

  calculateTotalPages(): void {
    const total = Math.ceil(this.filteredEnrollments().length / this.itemsPerPage);
    this.totalPages.set(total > 0 ? total : 1);
  }

  getPaginatedEnrollments(): EnrollmentDetails[] {
    const start = (this.currentPage() - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    return this.filteredEnrollments().slice(start, end);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  getPageNumbers(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 4) {
        for (let i = 1; i <= 5; i++) pages.push(i);
        pages.push(-1);
        pages.push(total);
      } else if (current >= total - 3) {
        pages.push(1);
        pages.push(-1);
        for (let i = total - 4; i <= total; i++) pages.push(i);
      } else {
        pages.push(1);
        pages.push(-1);
        for (let i = current - 1; i <= current + 1; i++) pages.push(i);
        pages.push(-1);
        pages.push(total);
      }
    }

    return pages;
  }

  visibleRangeEnd(): number {
    return Math.min(this.currentPage() * this.itemsPerPage, this.filteredEnrollments().length);
  }

  clearFilters(): void {
    this.searchTerm.set('');
    this.filterSession.set('');
    this.filterDepartment.set('');
    this.filterLevel.set('');
    this.filterSemester.set('');
    this.filteredEnrollments.set(this.enrollments());
    this.currentPage.set(1);
    this.calculateTotalPages();
  }

  exportToCSV(): void {
    const enrollments = this.filteredEnrollments();
    if (enrollments.length === 0) {
      alert('No data to export');
      return;
    }

    const headers = ['Reg No', 'Student Name', 'Course Code', 'Course Name', 'Units', 'Session', 'Department', 'Level', 'Semester', 'Enrolled Date'];
    const rows = enrollments.map(e => [
      e.studentRegno || '',
      e.studentName || '',
      e.courseId?.toString() || '',
      e.courseName || '',
      '', // units not available
      e.sessionName || '',
      e.departmentName || '',
      '', // level not available
      e.semesterName || '',
      e.enrollDate ? new Date(e.enrollDate).toLocaleDateString() : ''
    ]);

    const csvContent = [
      headers.join(','),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `enrollment-history-${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
