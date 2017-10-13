import { User } from "./user.model"

export class Position {
  id: number;
  userId: string;
  title: string;
  company: string;
  description: string;
  isCurrent: boolean;
  from: string;
  to: string;
  user: User;
}
