import { Component } from "@angular/core";
import { NavController, NavParams } from "ionic-angular";
import { Common } from "../../providers/common.service";
import { ApiService } from "../../providers/api.service";
import { User } from "../../models";

@Component({
  selector: "page-page2",
  templateUrl: "page2.html"
})

export class Page2 {
  selectedItem: any;
  users: Array<User> = [];
  title = "Home"; 
  onLink(url: string) {
    window.open(url);
  }
  constructor(public navCtrl: NavController, public navParams: NavParams, public auth: ApiService, public common : Common) {
    // If we navigated to this page, we will have an item available as a nav param
    this.selectedItem = navParams.get("item");
    this.auth.Users().subscribe(users => {
      this.users = users;
    });
  }
  
  itemTapped(event, item) {
    // That's right, we're pushing to ourselves!
    this.navCtrl.push(Page2, {
      item: item
    });
  }
}
