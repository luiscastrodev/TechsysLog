import { User } from "./User";

export interface UserResponse {
  isSuccess: boolean;
  data: User;
  message?: string;
}
