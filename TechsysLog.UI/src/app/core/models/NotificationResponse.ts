import { AppNotification } from "./Notification";

export interface NotificationResponse {
  isSuccess: boolean;
  data: AppNotification[];
  message?: string;
}
