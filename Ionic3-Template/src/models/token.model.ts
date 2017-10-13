import { User } from "./user.model";

export interface Token {
  access_token: string;
  token_type: string;
  expires_in: number;
  user_data: User;
}
