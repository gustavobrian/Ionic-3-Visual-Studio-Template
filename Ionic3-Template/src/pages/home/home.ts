import { Component } from "@angular/core";
import { Http } from "@angular/http";
import { Base } from "../../base/base.component";
import { NavController, IonicPage } from "ionic-angular";
import { Common } from "../../providers/common.service";

@IonicPage({
  name: "HomePage"
})
@Component({
  selector: "page-home",
  templateUrl: "home.html"
})
export class HomePage extends Base{

  constructor(public navCtrl: NavController, public http : Http, public common : Common) {
    super(http, common);
  }


}
