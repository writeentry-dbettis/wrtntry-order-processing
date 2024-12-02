import { IOrder } from "./IOrder";

export class Order implements IOrder
{
  constructor(
    public id: string, 
    public poNumber: string,
    public totalAmount: number,
    public tax: number,
    public createdTime: Date,
    public status: string) {}
}