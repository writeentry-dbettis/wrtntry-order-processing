import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { IOrder } from '../models/IOrder';

@Injectable({
  providedIn: 'root'
})
export class BatchUploadService {

  constructor(private httpClient: HttpClient) { }

  public uploadBatchFile(form: FormData)
  {
    return this.httpClient.post<any>(environment.apiBaseUrl + '/api/batch/new', form);
  }

  public queueOrders(batchId: string, orders: Map<string, IOrder>)
  {
    const orderArray = Array.from(orders.values());

    return this.httpClient.post<number>(
      environment.apiBaseUrl + `/api/batch/${batchId}/queue`, 
      JSON.stringify(orderArray));
  }
}
