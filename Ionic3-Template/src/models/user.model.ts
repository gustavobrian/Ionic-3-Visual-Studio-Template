import { Position } from "./position.model"

export class User {
  isAuthenticated = false;
  firstName: string;
  lastName: string;
  headline: string;
  summary: string;
  profileImageUrl: string;
  id: string;
  userName: string;
  email: string;
  phoneNumber: string;
  positions: Position[];
  accessToken : string;
}
