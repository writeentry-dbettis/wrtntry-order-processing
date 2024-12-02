export interface IOrder {
    id: string;
    poNumber: string;
    totalAmount: number;
    tax: number;
    createdTime: Date;
    status: string;
}
