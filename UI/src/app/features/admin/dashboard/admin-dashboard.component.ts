import { Component, OnInit, signal, inject, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { StoreService } from '../../../core/services/store.service';

declare var d3: any;

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit, AfterViewInit {
  private shipmentService = inject(ShipmentService);
  protected store = inject(StoreService);

  @ViewChild('revenueChart') revenueChart!: ElementRef;
  @ViewChild('hubChart') hubChart!: ElementRef;

  stats = signal({
    totalShipments: 0,
    revenue: 0,
    activeHubs: 0,
    pendingPickups: 0,
    exceptions: 0
  });

  recentShipments = signal<any[]>([]);
  isLoading = signal(false);

  ngOnInit() {
    this.loadStats();
  }

  ngAfterViewInit() {
    setTimeout(() => this.createCharts(), 500);
  }

  loadStats() {
    this.isLoading.set(true);
    
    // Fetch real shipments to calculate accurate stats
    this.shipmentService.getShipments().subscribe({
      next: (shipments) => {
        const total = shipments.length;
        const revenue = shipments.reduce((acc, s) => acc + (s.totalAmount || s.totalCost || 0), 0);
        const pending = shipments.filter(s => ['Booked', 'Draft', 'PaymentPending'].includes(s.status)).length;
        const exceptions = shipments.filter(s => ['Failed', 'PaymentFailed', 'Delayed'].includes(s.status)).length;

        this.stats.set({
          totalShipments: total,
          revenue: revenue,
          activeHubs: 5, // Mock until hub API is ready
          pendingPickups: pending,
          exceptions: exceptions
        });

        // Set real recent shipments
        this.recentShipments.set(shipments.slice(0, 5).map(s => ({
          id: s.trackingNumber,
          customer: s.senderAddress?.contactName || 'User',
          amount: s.totalAmount || s.totalCost || 0,
          status: s.status,
          time: new Date(s.bookedAt || s.createdAt || '').toLocaleTimeString()
        })));

        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  createCharts() {
    this.renderRevenueChart();
    this.renderHubChart();
  }

  private renderRevenueChart() {
    const data = [
      { date: new Date(2024, 0, 1), value: 1200 },
      { date: new Date(2024, 0, 5), value: 1800 },
      { date: new Date(2024, 0, 10), value: 1400 },
      { date: new Date(2024, 0, 15), value: 2500 },
      { date: new Date(2024, 0, 20), value: 2100 },
      { date: new Date(2024, 0, 25), value: 3200 },
      { date: new Date(2024, 0, 30), value: 4500 },
    ];

    const element = this.revenueChart.nativeElement;
    const width = element.offsetWidth;
    const height = 200;

    const svg = d3.select(element)
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .append('g')
      .attr('transform', 'translate(0,0)');

    const x = d3.scaleTime()
      .domain(d3.extent(data, (d: any) => d.date) as [Date, Date])
      .range([0, width]);

    const y = d3.scaleLinear()
      .domain([0, d3.max(data, (d: any) => d.value) as number])
      .range([height, 0]);

    const area = d3.area()
      .x((d: any) => x(d.date))
      .y0(height)
      .y1((d: any) => y(d.value))
      .curve(d3.curveBasis);

    svg.append('path')
      .datum(data)
      .attr('fill', 'url(#revenueGradient)')
      .attr('d', area);

    const line = d3.line()
      .x((d: any) => x(d.date))
      .y((d: any) => y(d.value))
      .curve(d3.curveBasis);

    svg.append('path')
      .datum(data)
      .attr('fill', 'none')
      .attr('stroke', '#d4af37')
      .attr('stroke-width', 3)
      .attr('d', line);

    // Add gradient
    const defs = svg.append('defs');
    const gradient = defs.append('linearGradient')
      .attr('id', 'revenueGradient')
      .attr('x1', '0%')
      .attr('y1', '0%')
      .attr('x2', '0%')
      .attr('y2', '100%');

    gradient.append('stop')
      .attr('offset', '0%')
      .attr('stop-color', '#d4af37')
      .attr('stop-opacity', 0.2);

    gradient.append('stop')
      .attr('offset', '100%')
      .attr('stop-color', '#d4af37')
      .attr('stop-opacity', 0);
  }

  private renderHubChart() {
    const data = [
      { name: 'Mumbai', value: 85 },
      { name: 'Delhi', value: 92 },
      { name: 'Bangalore', value: 78 },
      { name: 'Chennai', value: 45 },
    ];

    const element = this.hubChart.nativeElement;
    const width = element.offsetWidth;
    const height = 200;
    const radius = Math.min(width, height) / 2;

    const svg = d3.select(element)
      .append('svg')
      .attr('width', width)
      .attr('height', height)
      .append('g')
      .attr('transform', `translate(${width / 2},${height / 2})`);

    const pie = d3.pie().value((d: any) => d.value);
    const arc = d3.arc().innerRadius(radius * 0.7).outerRadius(radius);

    const colors = ['#d4af37', '#10b981', '#3b82f6', '#6366f1'];

    svg.selectAll('path')
      .data(pie(data))
      .enter()
      .append('path')
      .attr('d', arc)
      .attr('fill', (d: any, i: number) => colors[i % colors.length])
      .attr('stroke', 'none');
  }
}
