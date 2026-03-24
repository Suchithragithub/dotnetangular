import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { SessionService } from '../../../../core/services/session.service';
import { Session } from '../../../../core/models/session.model';

@Component({
  selector: 'app-admin-session',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.scss']
})
export class AdminSessionComponent implements OnInit {
  private fb = inject(FormBuilder);
  private sessionService = inject(SessionService);
  private router = inject(Router);

  sessionForm: FormGroup;
  sessions = signal<Session[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    this.sessionForm = this.fb.group({
      sesssion: ['', [Validators.required, Validators.pattern(/^\d{4}\/\d{4}$/)]]
    });
  }

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions(): void {
    this.loading.set(true);
    this.error.set(null);

    this.sessionService.getSessions().subscribe({
      next: (data: Session[]) => {
        this.sessions.set(data);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.error.set('Failed to load sessions. Please try again.');
        this.loading.set(false);
        console.error('Error loading sessions:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.sessionForm.invalid) {
      this.sessionForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const sessionData: Session = {
      sessionName: this.sessionForm.value.sesssion
    };

    this.sessionService.createSession(sessionData).subscribe({
      next: (_response: Session) => {
        this.successMessage.set('Session created successfully!');
        this.sessionForm.reset();
        this.loadSessions();
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Failed to create session. Please try again.');
        this.loading.set(false);
        console.error('Error creating session:', err);
      }
    });
  }

  deleteSession(id: number): void {
    if (!confirm('Are you sure you want to delete this session? This action cannot be undone.')) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    this.sessionService.deleteSession(id).subscribe({
      next: () => {
        this.successMessage.set('Session deleted successfully!');
        this.loadSessions();
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Failed to delete session. Please try again.');
        this.loading.set(false);
        console.error('Error deleting session:', err);
      }
    });
  }

  get sesssionControl() {
    return this.sessionForm.get('sesssion');
  }
}
