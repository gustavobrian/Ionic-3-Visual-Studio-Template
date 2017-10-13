import { Component } from "@angular/core";
import { NavController } from "ionic-angular";
import { ApiService } from "../../providers/api.service";
import { LoginPage } from "../login/login";
import { RegisterModel } from "../../models";

@Component({
  selector: "page-register",
  templateUrl: "register.html"
})
export class RegisterPage {
  model = new RegisterModel();

  constructor(public navCtrl: NavController, public auth: ApiService) {
  }

  register() {
    this.auth.navigate = () => {
      this.navCtrl.setRoot(LoginPage);
    }
    this.auth.Register(this.model);
  }

  readUrl(event) {
    if (event.target.files && event.target.files[0]) {
      var reader = new FileReader();
      reader.onload = () => {
        this.model.profileImage = reader.result;
      }
      reader.readAsDataURL(event.target.files[0]);
    }
  }
}
