import { UserRole } from "./UserRole";

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  role: UserRole;
}
