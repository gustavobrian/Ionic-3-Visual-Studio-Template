import { RequestOptionsArgs, Headers, ResponseContentType } from "@angular/http";
import { Injectable } from "@angular/core";
import { LoadingController, AlertController, LoadingOptions, AlertOptions } from "ionic-angular";
import { Storage } from "@ionic/storage";

@Injectable()
export class Common {
  constructor(public storage: Storage, public alertCtrl: AlertController, public loadingCtrl: LoadingController) {
    this.initailizeService();
  }

  initailizeService() {
    this.paths = {
      registerPath: "/api/Account/Register",
      loginPath: "/api/Account/Login",
      updateProfilePath: "/api/Account/UpdateProfile",
      logoutPath: "/api/Account/Logout",
      changePasswordPath: "/api/Account/ChangePassword"
    };

    this.domain = "https://localhost:44390";
    this.requestOptions = {
      responseType: ResponseContentType.Json,
      headers: new Headers({
        "Content-Type": "application/json;charset=UTF-8"
      })
    };
  }

  paths: { registerPath: string; loginPath: string; updateProfilePath: string; logoutPath: string; changePasswordPath; }
  domain: string;
  requestOptions: RequestOptionsArgs;

  getKey<T>(key: string) {
    return this.storage.get(key).then((value: T) => {
      return value;
    });
  }

  createLodder(options: LoadingOptions) {
    const loader = this.loadingCtrl.create(options);
    loader.present();
    return loader;
  }

  createAlert(options: AlertOptions) {
    const alert = this.alertCtrl.create(options);
    alert.present();
    return alert;
  }

  getMessage(json: {}) {
    let message = "";
    for (let key in json) {
      if (json.hasOwnProperty(key)) {
        message += `${json[key]}\n`;
      }
    }
    return message;
  }
}
