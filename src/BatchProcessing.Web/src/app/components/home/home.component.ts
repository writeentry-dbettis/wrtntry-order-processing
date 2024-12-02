import { Component, OnInit } from '@angular/core';
import { OrderStatusService } from '../../services/order-status.service';
import { IOrder } from '../../models/IOrder';
import { BatchUploadService } from '../../services/batch-upload.service';
import { Order } from '../../models/order';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm, NgModel } from '@angular/forms';

@Component({
  selector: 'app-home',
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent implements OnInit {
  orders = new Map<string, IOrder>;
  uploadFile: File | null = null;
  uploadStatus: string = "";

  constructor(
    private orderStatusService: OrderStatusService,
    private batchUploadService: BatchUploadService
  ) {}

  ngOnInit(): void {
    this.orderStatusService.onStatusChanged((id, status) => {
      if (this.orders === null || this.orders.size === 0)
      {
        return;
      }

      const statusElement = document.getElementById('status-' + id);

      if (statusElement !== null) {
        statusElement.innerHTML = `<i class="status-${status.toLowerCase()}"></i>`;
      }      
    })
  }

  async onSubmit($event: any) {
    this.orders.clear();

    const data = new FormData($event.target);

    this.uploadStatus = "Uploading...";

    this.batchUploadService.uploadBatchFile(data)
      .subscribe(res => {
        this.orderStatusService.startConnection()
          .then(() => {
            this.orderStatusService.subscribeToGroup(res.batchId);
          });

        let orderCount = res.results.length;

        for (var i = 0; i < orderCount; i++)
        {
          this.orders.set(res.results[i].id, 
            new Order(
              res.results[i].id,
              res.results[i].poNumber,
              res.results[i].totalAmount,
              res.results[i].tax,
              new Date(res.results[i].createdDate),
              ""));
        }

        this.uploadStatus = `Uploaded ${orderCount} entries. Queueing...`;

        this.batchUploadService.queueOrders(res.batchId, this.orders)
          .subscribe(res => {
            this.uploadStatus = `Queued ${res} entries.`;
          })
      });
  }
}
