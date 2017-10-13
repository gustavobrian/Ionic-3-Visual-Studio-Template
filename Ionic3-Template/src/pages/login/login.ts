import { Component } from "@angular/core";
import { NavController } from "ionic-angular";
import { LoginModel } from "../../models";
import { ApiService } from "../../providers/api.service";

@Component({
  selector: "page-login",
  templateUrl: "login.html"
})
export class LoginPage {
  model = new LoginModel();

  constructor(public navCtrl: NavController, public auth: ApiService) {
  }

  login() {
    this.auth.Login(this.model);
  }
}
