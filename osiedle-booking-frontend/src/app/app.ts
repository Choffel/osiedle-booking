import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/components/header/header'; // <-- Путь к header.ts

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent], // <-- Зарегистрирован здесь
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class AppComponent {}