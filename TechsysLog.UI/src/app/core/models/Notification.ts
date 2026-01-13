import { NotificationType } from "./NotificationType";

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  relatedEntityId: string;
  relatedEntityType: string;
}
