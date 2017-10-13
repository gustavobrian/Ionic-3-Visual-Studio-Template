import { Injectable } from "@angular/core";
import { Http, Headers, URLSearchParams, Response, RequestMethod } from "@angular/http";
import { Observable } from "rxjs";
import { ChangePasswordModel, LoginModel, RegisterModel, ProfileModel, User, Token } from "../models";
import { Common } from "./common.service";
import { Events } from "ionic-angular";



@Injectable()
export class ApiService {
  navigate: () => void;
  private domain: string;
  user: User;

  constructor(private http: Http, public common: Common, public events: Events) {
    this.domain = this.common.domain;
    this.common.getKey<User>("user").then(value => {
      this.user = value || new User();
    });
  }

  map<T>(response: Response) {
    const contentType = response.headers.get("Content-type");
    if (contentType && contentType.includes("application/json")) {
      return response.json() as T;
    }
    return ((response) as any) as T;
  };

  /**
  * Method ChangePasswordPost
  * @param model The
  * @param Authorization The The bearer authorization header
  * @return The full HTTP response as Observable
  */
  ChangePassword(model: ChangePasswordModel): Observable<Token> {
    const uri = this.common.paths.changePasswordPath;
    const headers = new Headers();
    const params = new URLSearchParams();
    if (this.user && this.user.isAuthenticated) {
      headers.append("Authorization", `Bearer ${this.user.accessToken}`);
    }
    return this.sendRequest<Token>(RequestMethod.Post, uri, headers, params, JSON.stringify(model));
  }

  /**
  * Method LoginPost
  * @param model The
  * @return The full HTTP response as Observable
  */
  Login(model: LoginModel) {
    const loader = this.common.createLodder({ content: "Loging in..." });
    const uri = this.common.paths.loginPath;
    const headers = new Headers();
    const params = new URLSearchParams();
    this.sendRequest<Token>(RequestMethod.Post, uri, headers, params, JSON.stringify(model)).subscribe((data) => {
      this.user = data.user_data;
      this.user.isAuthenticated = true;
      this.user.accessToken = data.access_token;
      this.common.storage.set(`user`, this.user);
      loader.dismiss().then(() => {
        this.events.publish("updateScreen:login", { title : "Home" });
      });
    },
      error => {
        loader.dismissAll();
        var body = error.json();
        var message = this.common.getMessage(body);
        this.common.createAlert({
          title: "Login Error",
          subTitle: message,
          buttons: ["OK"]
        });
      });
  }

  /**
  * Method LogoutPost
  * @param Authorization The The bearer authorization header
  * @return The full HTTP response as Observable
  */
  Logout() {
    var lodder = this.common.createLodder({ content: "Logging out..." });
    const uri = this.common.paths.logoutPath;
    const headers = new Headers();
    const params = new URLSearchParams();
    if (this.user && this.user.isAuthenticated) {
      headers.append("Authorization", `Bearer ${this.user.accessToken}`);
    }
    this.sendRequest<Response>(RequestMethod.Post, uri, headers, params, null).subscribe(() => {
      this.common.storage.remove("user");
      this.events.publish("updateScreen:logout", { });
    }, error => {
      lodder.dismissAll();
      if (error.status === 401) {
        this.common.createAlert({
          title: error.statusText,
          buttons: ["OK"]
        });
      } else {
        console.log(error);
      }
    });
  }

  /**
  * Method RegisterPost
  * @param model The
  * @return The full HTTP response as Observable
  */
  Register(model: RegisterModel) {
    const loader = this.common.createLodder({ content: "Registering..." });
    const uri = this.common.paths.registerPath;
    const headers = new Headers();
    const params = new URLSearchParams();
    this.sendRequest<Response>(RequestMethod.Post, uri, headers, params, JSON.stringify(model)).subscribe(() => {
      loader.dismissAll();
      const alert = this.common.createAlert({
        title: "Register",
        message: "Registerigin is succussful",
        buttons: [
          {
            text: "Login",
            handler: () => {
              alert.dismiss().then(() => {
                this.events.publish("updateScreen:register", { title: "Login" });
              });
              return false;
            }
          }
        ]
      });
    },
      error => {
        loader.dismissAll();
        var body = error.json();
        var message = this.common.getMessage(body);
        this.common.createAlert({
          title: "Register Error",
          subTitle: message,
          buttons: ["OK"]
        });
      });
  }

  //upload(files: Array<File>) {
  //  const formData = new FormData();
  //  for (let i = 0; i < files.length; i++) {
  //    formData.append("files", files[i]);
  //  }
  //  const url = `${this.common.serviceUrl}${this.common.paths.uploadPath}`;
  //  return this.http.post(url, formData).subscribe(response => {
  //      console.log(response);
  //    },
  //    error => {
  //      console.log(error);
  //    });
  //}
  /**
  * Method UpdateProfilePost
  * @param model The
  * @param Authorization The The bearer authorization header
  * @return The full HTTP response as Observable
  */
  UpdateProfile(model: ProfileModel): Observable<Token> {
    const uri = this.common.paths.updateProfilePath;
    const headers = new Headers();
    const params = new URLSearchParams();
    if (this.user && this.user.isAuthenticated) {
      headers.append("Authorization", `Bearer ${this.user.accessToken}`);
    }
    return this.sendRequest<Token>(RequestMethod.Post, uri, headers, params, JSON.stringify(model));
  }

  /**
  * Method UsersGet
  * @return The full HTTP response as Observable
  */
  Users(): Observable<User[]> {
    const uri = `/api/Users`;
    const headers = new Headers();
    if (this.user && this.user.isAuthenticated) {
      headers.append("Authorization", `Bearer ${this.user.accessToken}`);
    }
    const params = new URLSearchParams();
    return this.sendRequest<User[]>(RequestMethod.Get, uri, headers, params, null);
  }

  /**
  * Method UserByIdGet
  * @param id The
  * @return The full HTTP response as Observable
  */
  UserById(id: string): Observable<User> {
    const uri = `/api/Users/${id}`;
    const headers = new Headers();
    if (this.user && this.user.isAuthenticated) {
      headers.append("Authorization", `Bearer ${this.user.accessToken}`);
    }
    const params = new URLSearchParams();
    return this.sendRequest<User>(RequestMethod.Get, uri, headers, params, null);
  }

  private sendRequest<T>(method: RequestMethod, uri: string, headers: Headers, params: URLSearchParams, body: any): Observable<T> {
    switch (method) {
      case RequestMethod.Get:
        headers.append("Accept", "application/json");
        return this.http.get(this.domain + uri, { headers: headers, params: params }).map((value) => this.map(value));
      case RequestMethod.Put:
        headers.append("Content-Type", "application/json");
        return this.http.put(this.domain + uri, body, { headers: headers, params: params }).map((value) => this.map(value));
      case RequestMethod.Post:
        headers.append("Content-Type", "application/json");
        return this.http.post(this.domain + uri, body, { headers: headers, params: params }).map((value) => this.map(value));
      case RequestMethod.Delete:
        return this.http.delete(this.domain + uri, { headers: headers, params: params }).map((value) => this.map(value));
      default:
        console.error(`Unsupported request: ${method}`);
        return Observable.throw(`Unsupported request: ${method}`);
    }
  }
}
