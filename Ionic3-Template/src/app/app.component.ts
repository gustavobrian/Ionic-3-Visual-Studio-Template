import { Base } from "./../base/base.component";
import { Component, ViewChild } from "@angular/core";
import { Nav, Platform, Events } from "ionic-angular";
import { SplashScreen } from "@ionic-native/splash-screen";
import { StatusBar } from "@ionic-native/status-bar";
import { HomePage } from "../pages/home/home";
import { Page2 } from "../pages/page2/page2";
import { NativeStorage } from "@ionic-native/native-storage";
import { Http } from "@angular/http";
import { Common } from "../providers/common.service";
import { RegisterPage } from "../pages/register/register";
import { LoginPage } from "../pages/login/login";
import { Page } from "../models";
import { ApiService } from "../providers/api.service";


@Component({
  templateUrl: "app.html"
})
export class MyApp extends Base {
  @ViewChild(Nav) nav: Nav;
  rootPage: any = HomePage;
  pages: Array<Page>;
  authorizedUserPages: Array<Page>;
  unauthorizedUserPages: Array<Page>;

  constructor(public platform: Platform, public auth: ApiService, public http: Http, public common: Common, public statusBar: StatusBar, public splashScreen: SplashScreen, public nativeStorage: NativeStorage, public events: Events) {
    super(http, common);
    this.initializeApp();

    // used for an example of ngFor and navigation
    this.pages = [
      { icon: "home", title: "Home", component: HomePage }
    ];

    this.authorizedUserPages = [
      { icon: "list", title: "Page Two", component: Page2 },
      { icon: "settings", title: "Settings", component: Page2 }
    ];

    this.unauthorizedUserPages = [
      { icon: "person-add", title: "Register", component: RegisterPage },
      { icon: "log-in", title: "Login", component: LoginPage }
    ];

    
    this.events.subscribe("updateScreen:login", (data: { title: string }) => {
      this.title = data.title;
      this.nav.setRoot(HomePage);
    });

    this.events.subscribe("updateScreen:register", (data: { title: string }) => {
      this.title = data.title;
      this.nav.setRoot(LoginPage);
    });

    this.events.subscribe("updateScreen:logout", () => {
      location.reload();
    });
  }

  initializeApp() {
    this.platform.ready().then(() => {
      // Okay, so the platform is ready and our plugins are available.
      // Here you can do any higher level native things you might need.
      this.statusBar.styleDefault();
      this.splashScreen.hide();
    });
  }

  openPage(page: Page) {
    this.rootPage = page.component;
    this.title = page.title;
    this.nav.setRoot(page.component);
  }

  logout() {
    this.auth.Logout();
  }
}
