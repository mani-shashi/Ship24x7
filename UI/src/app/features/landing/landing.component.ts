import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { Title, Meta } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

import { NavbarComponent } from '../../shared/components/navbar.component';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink, NavbarComponent],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css',
})
export class LandingComponent implements OnInit {
  store = inject(StoreService);
  private title = inject(Title);
  private meta = inject(Meta);

  ngOnInit() {
    this.title.setTitle('Ship24x7™ | Next-Gen Logistics & Global Shipping');
    this.meta.updateTag({ name: 'description', content: 'Ship24x7 is a premium logistics orchestration platform offering real-time satellite tracking, smart routing, and secure global shipping solutions.' });
    this.meta.updateTag({ name: 'keywords', content: 'logistics, shipping, tracking, global trade, supply chain, satellite tracking' });
  }
}
