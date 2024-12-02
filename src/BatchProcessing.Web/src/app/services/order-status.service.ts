import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class OrderStatusService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.apiBaseUrl + '/hub')
      .build();
  }

  async startConnection() : Promise<void> {
    return this.hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while starting connection: ' + err));
  }

  async subscribeToGroup(groupId: string) : Promise<void> {
    return this.hubConnection
      .send("subscribe", groupId);
  }

  onStatusChanged(callback: (id: string, status: string) => void) {
    this.hubConnection.on("statusChanged", callback);
  }
}
