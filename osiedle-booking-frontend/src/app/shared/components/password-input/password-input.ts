import { Component, signal, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-password-input',
  styleUrl: './password-input.scss',
  templateUrl: './password-input.html',
})
export class PasswordInputComponent {
  control = input.required<FormControl<string>>();

  inputId = input('password');
  placeholder = input('Wpisz hasło');
  autocomplete = input('current-password');

  isVisible = signal(false);
  toggleVisibility(): void {
    this.isVisible.update((value) => !value);
  }
}
