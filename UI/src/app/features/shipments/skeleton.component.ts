import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [],
  template: `
    <div 
      [class]="'animate-pulse bg-slate-200 dark:bg-white/5 ' + customClass"
      [style.width]="width"
      [style.height]="height"
      [style.border-radius]="borderRadius"
    ></div>
  `,
  styles: [`
    :host { display: block; }
  `]
})
export class SkeletonComponent {
  @Input() width: string = '100%';
  @Input() height: string = '20px';
  @Input() borderRadius: string = '8px';
  @Input() customClass: string = '';
}
