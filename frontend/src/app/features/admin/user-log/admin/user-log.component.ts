import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserlogService } from '../../../../core/services/userlog.service';
import { UserLog } from '../../../../core/models/user-log.model';

@Component({
  selector: 'app-admin-user-log',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-log.component.html',
  styleUrls: ['./user-log.component.scss']
})
export class AdminUserLogComponent implements OnInit {
  private userlogService = inject(UserlogService);

  userLogs = signal<UserLog[]>([]);
  filteredLogs = signal<UserLog[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  searchTerm = signal('');
  filterType = signal<'all' | 'login' | 'logout'>('all');
  currentPage = signal(1);
  itemsPerPage = 20;

  ngOnInit(): void {
    this.loadUserLogs();
  }

  loadUserLogs(): void {
    this.loading.set(true);
    this.error.set(null);

    this.userlogService.getUserLogs().subscribe({
      next: (logs) => {
        this.userLogs.set(logs);
        this.applyFilters();
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load user activity logs. Please try again.');
        this.loading.set(false);
        console.error('Error loading user logs:', err);
      }
    });
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchTerm.set(value);
    this.currentPage.set(1);
    this.applyFilters();
  }

  onFilterTypeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as 'all' | 'login' | 'logout';
    this.filterType.set(value);
    this.currentPage.set(1);
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.userLogs()];

    // Filter by activity type
    if (this.filterType() !== 'all') {
      filtered = filtered.filter(log => 
        (log.status === 1 ? 'login' : 'logout') === this.filterType()
      );
    }

    // Filter by search term (registration number or student name)
    const search = this.searchTerm().toLowerCase().trim();
    if (search) {
      filtered = filtered.filter(log => 
        (log.studentRegno || '').toLowerCase().includes(search) ||
        (log.userip || '').toLowerCase().includes(search)
      );
    }

    // Sort by timestamp descending (most recent first)
    filtered.sort((a, b) => {
      const dateA = new Date(a.loginTime || 0).getTime();
      const dateB = new Date(b.loginTime || 0).getTime();
      return dateB - dateA;
    });

    this.filteredLogs.set(filtered);
  }

  get paginatedLogs(): UserLog[] {
    const start = (this.currentPage() - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    return this.filteredLogs().slice(start, end);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredLogs().length / this.itemsPerPage);
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;
    const current = this.currentPage();
    const delta = 2;
    const range: number[] = [];
    const rangeWithDots: number[] = [];

    for (let i = 1; i <= total; i++) {
      if (i === 1 || i === total || (i >= current - delta && i <= current + delta)) {
        range.push(i);
      }
    }

    let prev = 0;
    for (const i of range) {
      if (prev && i - prev > 1) {
        rangeWithDots.push(-1); // -1 represents ellipsis
      }
      rangeWithDots.push(i);
      prev = i;
    }

    return rangeWithDots;
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage.set(page);
    }
  }

  formatTimestamp(timestamp: string | Date | undefined): string {
    if (!timestamp) return 'N/A';
    const date = new Date(timestamp);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  getActivityBadgeClass(activity: string | undefined): string {
    if (!activity) return 'bg-secondary';
    return activity.toLowerCase() === 'login' ? 'bg-success' : 'bg-warning';
  }

  exportToCSV(): void {
    const headers = ['Timestamp', 'Registration Number', 'Student Name', 'Activity', 'IP Address'];
    const rows = this.filteredLogs().map(log => [
      this.formatTimestamp(log.loginTime),
      log.studentRegno || '',
      '',
      log.status === 1 ? 'login' : 'logout',
      log.userip || ''
    ]);

    let csvContent = headers.join(',') + '\n';
    rows.forEach(row => {
      csvContent += row.map(cell => `"${cell}"`).join(',') + '\n';
    });

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `user-activity-log-${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  refresh(): void {
    this.loadUserLogs();
  }

  visibleRangeEnd(): number {
    return Math.min(this.currentPage() * this.itemsPerPage, this.filteredLogs().length);
  }

  loginCount(): number {
    return this.userLogs().filter((log) => log.status === 1).length;
  }

  logoutCount(): number {
    return this.userLogs().filter((log) => log.status !== 1).length;
  }

  trackByLogId(_index: number, log: UserLog): number {
    return log.id;
  }
}
