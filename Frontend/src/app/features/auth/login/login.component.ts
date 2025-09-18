import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  otpForm: FormGroup;
  showOtpForm = false;
  isLoading = false;
  otpSent = false;
  countdown = 0;
  private countdownInterval: any;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  ngOnInit(): void {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated) {
      this.router.navigate(['/dashboard']);
    }
  }

  onSendOtp(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.loginForm.get('email')?.value;

    this.authService.sendOtp({ email, type: 'login' }).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.showOtpForm = true;
        this.otpSent = true;
        this.startCountdown(300); // 5 minutes
        this.snackBar.open('OTP sent to your email', 'Close', { duration: 3000 });
      },
      error: (error) => {
        this.isLoading = false;
        const message = error.error?.message || 'Failed to send OTP';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  onVerifyOtp(): void {
    if (this.otpForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.loginForm.get('email')?.value;
    const code = this.otpForm.get('otp')?.value;

    this.authService.verifyOtp({ email, code, type: 'login' }).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.snackBar.open('Login successful!', 'Close', { duration: 3000 });
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.isLoading = false;
        const message = error.error?.message || 'Invalid OTP';
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  onResendOtp(): void {
    if (this.countdown > 0) {
      return;
    }

    this.onSendOtp();
  }

  private startCountdown(seconds: number): void {
    this.countdown = seconds;
    
    this.countdownInterval = setInterval(() => {
      this.countdown--;
      
      if (this.countdown <= 0) {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }

  getCountdownDisplay(): string {
    const minutes = Math.floor(this.countdown / 60);
    const seconds = this.countdown % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }
}