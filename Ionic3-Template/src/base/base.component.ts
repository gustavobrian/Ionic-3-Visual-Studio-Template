import { Common } from "../providers/common.service";
import { Http } from "@angular/http";


export class ServiceBase {

  constructor(public http: Http, public common: Common) {
  }

  onLink(url: string) {
    window.open(url);
  }
}

export class Base {
  title = "Home";

  constructor(public http: Http, public common: Common) {
  }

  onLink(url: string) {
    window.open(url);
  }

}
