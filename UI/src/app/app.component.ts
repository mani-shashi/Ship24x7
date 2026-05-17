import { Component, inject } from '@angular/core';
import { ChildrenOutletContexts, RouterOutlet } from '@angular/router';
import { NotificationComponent } from './shared/components/notification.component';
import { MobileNavComponent } from './shared/components/mobile-nav.component';
import { slideInAnimation } from './shared/animations';
import { StoreService } from './core/services/store.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NotificationComponent, MobileNavComponent],
  template: `
    <app-notification />
    <div [@routeAnimations]="getRouteAnimationData()">
      <router-outlet />
    </div>
    @if (store.isAuthenticated()) {
      <app-mobile-nav />
    }
  `,
  animations: [slideInAnimation]
})
export class AppComponent {
  private contexts = inject(ChildrenOutletContexts);
  public store = inject(StoreService);

  getRouteAnimationData() {
    return this.contexts.getContext('primary')?.route?.snapshot?.data?.['animation'];
  }
}
