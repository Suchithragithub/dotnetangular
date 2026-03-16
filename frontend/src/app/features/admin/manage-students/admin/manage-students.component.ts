import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StudentService } from '../../../../core/services/student.service';
import { Student } from '../../../../core/models/student.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-students',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './manage-students.component.html',
  styleUrls: ['./manage-students.component.scss']
})
export class ManageStudentsComponent implements OnInit {
  private studentService = inject(StudentService);

  students = signal<Student[]>([]);
  filteredStudents = signal<Student[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  searchTerm = signal('');
  selectedStudent = signal<Student | null>(null);
  showDeleteModal = signal(false);
  showResetPasswordModal = signal(false);

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.loading.set(true);
    this.error.set(null);

    this.studentService.getAllStudents().subscribe({
      next: (students: Student[]) => {
        this.students.set(students);
        this.filteredStudents.set(students);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load students. Please try again.');
        this.loading.set(false);
        console.error('Error loading students:', err);
      }
    });
  }

  onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    const term = target.value.toLowerCase();
    this.searchTerm.set(term);

    if (!term) {
      this.filteredStudents.set(this.students());
      return;
    }

    const filtered = this.students().filter(student =>
      student.studentRegno?.toLowerCase().includes(term) ||
      student.studentName?.toLowerCase().includes(term) ||
      // student.email?.toLowerCase().includes(term) ||
      student.department?.toLowerCase().includes(term)
    );
    this.filteredStudents.set(filtered);
  }

  openDeleteModal(student: Student): void {
    this.selectedStudent.set(student);
    this.showDeleteModal.set(true);
    this.error.set(null);
    this.successMessage.set(null);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.selectedStudent.set(null);
  }

  confirmDelete(): void {
    const student = this.selectedStudent();
    if (!student || !student.studentRegno) return;

    this.loading.set(true);
    this.error.set(null);

    this.studentService.deleteStudent(student.studentRegno).subscribe({
      next: () => {
        this.successMessage.set(`Student ${student.studentRegno} has been deleted successfully.`);
        this.loading.set(false);
        this.closeDeleteModal();
        this.loadStudents();
        setTimeout(() => this.successMessage.set(null), 5000);
      },
      error: (err) => {
        this.error.set('Failed to delete student. Please try again.');
        this.loading.set(false);
        console.error('Error deleting student:', err);
      }
    });
  }

  openResetPasswordModal(student: Student): void {
    this.selectedStudent.set(student);
    this.showResetPasswordModal.set(true);
    this.error.set(null);
    this.successMessage.set(null);
  }

  closeResetPasswordModal(): void {
    this.showResetPasswordModal.set(false);
    this.selectedStudent.set(null);
  }

  confirmResetPassword(): void {
    const student = this.selectedStudent();
    if (!student || !student.studentRegno) return;

    this.loading.set(true);
    this.error.set(null);

    this.studentService.resetPassword(student.studentRegno).subscribe({
      next: () => {
        this.successMessage.set(`Password for student ${student.studentRegno} has been reset to Test@123.`);
        this.loading.set(false);
        this.closeResetPasswordModal();
        setTimeout(() => this.successMessage.set(null), 5000);
      },
      error: (err) => {
        this.error.set('Failed to reset password. Please try again.');
        this.loading.set(false);
        console.error('Error resetting password:', err);
      }
    });
  }

  getStudentPhoto(student: Student): string {
    return student.studentPhoto || 'assets/images/default-avatar.png';
  }

  formatCGPA(cgpa: number | undefined): string {
    return cgpa ? cgpa.toFixed(2) : 'N/A';
  }
}

// function inject(service: any): any {
//   return new service();
// }