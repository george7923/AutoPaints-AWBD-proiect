import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-splash-screen',
  standalone: true,
  templateUrl: './splash-screen.component.html',
  styleUrls: ['./splash-screen.component.css'],
})
export class SplashScreenComponent implements OnInit {
  constructor(private router: Router) { }

  ngOnInit(): void {
    // Navigate to the home page after 3 seconds
    setTimeout(() => {
      this.router.navigate(['/home']);
    }, 4000);
  }
}
