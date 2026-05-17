import { Component, inject, signal, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { StoreService } from '../../core/services/store.service';
import { TrackingService } from '../../core/services/tracking.service';
import { ShipmentService } from '../../core/services/shipment.service';

import { SkeletonComponent } from '../shipments/skeleton.component';
import { hoverScale } from '../../shared/animations';

declare var L: any;

interface TrackingResult {
  number: string;
  status: string;
  estimatedDelivery: Date;
  events: { status: string; location: string; time: Date; desc: string }[];
}

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, SkeletonComponent],
  templateUrl: './tracking.component.html',
  animations: [hoverScale]
})
export class TrackingComponent implements OnInit {
  store = inject(StoreService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  trackingNumber = '';
  isSearching = signal(false);
  errorMsg = '';
  result = signal<TrackingResult | null>(null);
  shipmentDetails = signal<any>(null);

  private map: any;
  private tileLayer: any;

  constructor() {
    effect(() => {
      const theme = this.store.theme();
      if (this.map) {
        this.updateMapTheme(theme);
      }
    });
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('trackingNumber');
      if (id) {
        this.trackingNumber = id;
        this.fetchTracking(id);
      }
    });
  }

  private updateMapTheme(theme: string) {
    if (this.tileLayer) {
      this.map.removeLayer(this.tileLayer);
    }
    const url = theme === 'dark' 
      ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
      : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
    
    this.tileLayer = L.tileLayer(url, { maxZoom: 19 }).addTo(this.map);
  }

  private trackingService = inject(TrackingService);
  private shipmentService = inject(ShipmentService);

  handleTrack() {
    if (!this.trackingNumber.trim()) return;
    const num = this.trackingNumber.trim().toUpperCase();
    this.router.navigate(['/track', num]);
  }

  private fetchTracking(num: string) {
    this.isSearching.set(true);
    this.errorMsg = '';
    this.result.set(null);
    this.shipmentDetails.set(null);

    // 1. Fetch shipment details first (Source of truth for existence)
    this.shipmentService.getShipmentByTrackingNumber(num).subscribe({
      next: (shipment) => {
        this.shipmentDetails.set(shipment);
        
        // Initialize a "virtual" tracking result if events aren't there yet
        this.result.set({
          number: shipment.trackingNumber,
          status: shipment.status,
          estimatedDelivery: new Date(shipment.estimatedDeliveryDate),
          events: []
        });

        // 2. Now try to fetch actual tracking events
        this.trackingService.trackShipment(num).subscribe({
          next: (res) => {
            // Determine the most advanced status between Shipment Service and Tracking Service
            const statusOrder = ['Draft', 'Booked', 'PaymentPending', 'Paid', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'];
            const shipmentStatus = shipment.status;
            const trackingStatus = res.currentStatus;
            
            const finalStatus = statusOrder.indexOf(shipmentStatus) > statusOrder.indexOf(trackingStatus) 
              ? shipmentStatus 
              : trackingStatus;

            this.result.set({
              number: res.trackingNumber,
              status: finalStatus,
              estimatedDelivery: new Date(res.estimatedDeliveryDate),
              events: res.events.map((e: any) => ({
                status: e.status,
                location: e.location,
                time: new Date(e.eventTimestamp),
                desc: e.description
              }))
            });
            this.isSearching.set(false);
            setTimeout(() => this.initMap(), 100);
          },
          error: () => {
            // If tracking events fail (e.g. 404), we still have shipment info!
            this.isSearching.set(false);
            setTimeout(() => this.initMap(), 100);
          }
        });
      },
      error: (err) => {
        this.errorMsg = err.error?.error || 'No shipment found with tracking number: ' + num;
        this.isSearching.set(false);
      }
    });
  }

  private cityCoords: { [key: string]: [number, number] } = {
    'Mumbai': [19.0760, 72.8777],
    'Delhi': [28.7041, 77.1025],
    'New Delhi': [28.6139, 77.2090],
    'Bangalore': [12.9716, 77.5946],
    'Chennai': [13.0827, 80.2707],
    'Kolkata': [22.5726, 88.3639],
    'Hyderabad': [17.3850, 78.4867],
    'Pune': [18.5204, 73.8567],
    'Ahmedabad': [23.0225, 72.5714],
    'Surat': [21.1702, 72.8311],
    'Jaipur': [26.9124, 75.7873],
    'Lucknow': [26.8467, 80.9462],
    'Nagpur': [21.1458, 79.0882],
    'Origin': [20.5937, 78.9629] // India Center fallback
  };

  private initMap() {
    if (this.map) {
      this.map.remove();
    }

    const mapEl = document.getElementById('trackingMap');
    if (!mapEl) return;

    const res = this.result();
    const details = this.shipmentDetails();
    
    // Get actual cities from shipment details if available, otherwise fallback to event location or defaults
    const originCity = details?.senderAddress?.city || res?.events?.[res.events.length - 1]?.location || 'Mumbai';
    const destCity = details?.receiverAddress?.city || 'Delhi';
    const currentCity = res?.events?.[0]?.location || 'Origin';
    
    const originPoint = this.cityCoords[originCity] || this.cityCoords['Mumbai'];
    const destPoint = this.cityCoords[destCity] || this.cityCoords['Delhi'];
    const currentPoint = this.cityCoords[currentCity] || this.cityCoords['Origin'];

    this.map = L.map('trackingMap', {
      zoomControl: false,
      attributionControl: false
    }).setView(currentPoint, 5);

    this.updateMapTheme(this.store.theme());

    // Custom ship/truck icon
    const shipIcon = L.divIcon({
      className: 'custom-div-icon',
      html: `<div class="w-8 h-8 bg-accent-gold/20 rounded-full flex items-center justify-center border border-accent-gold/50 shadow-neon">
               <span class="material-symbols-outlined text-accent-gold text-sm">local_shipping</span>
               <div class="absolute inset-0 bg-accent-gold rounded-full animate-ping opacity-20"></div>
             </div>`,
      iconSize: [32, 32],
      iconAnchor: [16, 16]
    });

    L.marker(currentPoint, { icon: shipIcon }).addTo(this.map);
    
    // Origin marker
    L.circleMarker(originPoint, {
      radius: 6,
      fillColor: '#d4af37',
      color: '#fff',
      weight: 2,
      opacity: 1,
      fillOpacity: 1
    }).addTo(this.map).bindPopup('Origin: ' + originCity);

    // Destination marker
    L.circleMarker(destPoint, {
      radius: 6,
      fillColor: '#4CAF50',
      color: '#fff',
      weight: 2,
      opacity: 1,
      fillOpacity: 1
    }).addTo(this.map).bindPopup('Destination: ' + destCity);
    
    // Add path between origin and destination
    const routePoints = [
      originPoint,
      destPoint
    ];
    
    L.polyline(routePoints, {
      color: '#d4af37',
      weight: 3,
      dashArray: '10, 10',
      opacity: 0.6
    }).addTo(this.map);

    // Auto-fit bounds to show the whole journey
    this.map.fitBounds(L.latLngBounds([originPoint, destPoint, currentPoint]), { padding: [50, 50] });
  }
}
