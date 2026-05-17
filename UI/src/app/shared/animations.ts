import { trigger, transition, style, query, animate, group, stagger } from '@angular/animations';

export const slideInAnimation =
  trigger('routeAnimations', [
    transition('* <=> *', [
      style({ position: 'relative' }),
      query(':enter, :leave', [
        style({
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%',
          opacity: 0
        })
      ], { optional: true }),
      query(':enter', [
        style({ transform: 'translateY(15px)', opacity: 0 })
      ], { optional: true }),
      group([
        query(':leave', [
          animate('400ms cubic-bezier(0.4, 0, 0.2, 1)', style({ transform: 'translateY(-15px)', opacity: 0 }))
        ], { optional: true }),
        query(':enter', [
          animate('600ms 100ms cubic-bezier(0.4, 0, 0.2, 1)', style({ transform: 'translateY(0)', opacity: 1 }))
        ], { optional: true })
      ])
    ])
  ]);

export const listAnimation = trigger('listAnimation', [
  transition('* <=> *', [
    query(':enter', [
      style({ opacity: 0, transform: 'translateY(20px)' }),
      stagger('80ms', [
        animate('500ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ], { optional: true })
  ])
]);

export const hoverScale = trigger('hoverScale', [
  transition(':enter', [
    style({ transform: 'scale(0.95)', opacity: 0 }),
    animate('300ms ease-out', style({ transform: 'scale(1)', opacity: 1 }))
  ])
]);
