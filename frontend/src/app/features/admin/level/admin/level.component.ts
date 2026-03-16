import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { LevelService } from '../../../../core/services/level.service';
import { Level } from '../../../../core/models/level.model';

@Component({
  selector: 'app-admin-level',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './admin/level.component.html',
  styleUrls: ['./admin/level.component.scss']
})
export class AdminLevelComponent implements OnInit {
  private fb = inject(FormBuilder);
  private levelService = inject(LevelService);

  levelForm: FormGroup;
  levels = signal<Level[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  constructor() {
    this.levelForm = this.fb.group({
      level: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  ngOnInit(): void {
    this.loadLevels();
  }

  loadLevels(): void {
    this.loading.set(true);
    this.error.set(null);

    this.levelService.getLevels().subscribe({
      next: (data: Level[]) => {
        this.levels.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load levels. Please try again.');
        this.loading.set(false);
        console.error('Error loading levels:', err);
      }
    });
  }

  onSubmit(): void {
    if (this.levelForm.invalid) {
      this.levelForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const levelData: Level = {
      id:0,
      levelName: this.levelForm.value.level.trim()
    };

    this.levelService.createLevel(levelData).subscribe({
      next: (response: Level) => {
        this.successMessage.set('Level created successfully!');
        this.levelForm.reset();
        this.loadLevels();
        this.loading.set(false);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to create level. Please try again.');
        this.loading.set(false);
        console.error('Error creating level:', err);
      }
    });
  }

  deleteLevel(id: number): void {
    if (!confirm('Are you sure you want to delete this level? This action cannot be undone.')) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    this.levelService.deleteLevel(id).subscribe({
      next: () => {
        this.successMessage.set('Level deleted successfully!');
        this.loadLevels();
        this.loading.set(false);
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage.set(null);
        }, 3000);
      },
      error: (err) => {
        this.error.set(err.error?.message || 'Failed to delete level. It may be in use.');
        this.loading.set(false);
        console.error('Error deleting level:', err);
      }
    });
  }

  get levelControl() {
    return this.levelForm.get('level');
  }
}