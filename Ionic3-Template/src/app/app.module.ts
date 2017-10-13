import { NgModule, ErrorHandler } from "@angular/core";
import { IonicApp, IonicModule, IonicPageModule, IonicErrorHandler } from "ionic-angular";
import { MyApp } from "./app.component";
import { HomePage } from "../pages/home/home";
import { Page2 } from "../pages/page2/page2";
import { RegisterPage } from "../pages/register/register";
import { BrowserModule } from "@angular/platform-browser";
import { HttpModule } from "@angular/http";
import { SplashScreen } from "@ionic-native/splash-screen";
import { StatusBar } from "@ionic-native/status-bar";
import { NativeStorage } from "@ionic-native/native-storage";
import { Common } from "../providers/common.service";
import { Decode64Pipe } from "../pipes/decode64";
import { IonicStorageModule } from "@ionic/storage";
import { LoginPage } from "../pages/login/login";
import { ApiService } from "../providers/api.service";

@NgModule({
  declarations: [
    MyApp,
    HomePage,
    Page2,
    RegisterPage,
    LoginPage,
    Decode64Pipe
  ],
  imports: [
    IonicModule.forRoot(MyApp),
    BrowserModule,
    HttpModule,
    IonicStorageModule.forRoot(), 
    IonicPageModule.forChild(HomePage)
  ],
  bootstrap: [IonicApp],
  entryComponents: [
    MyApp,
    HomePage,
    Page2,
    RegisterPage,
    LoginPage
  ],
  providers: [
    StatusBar, 
    SplashScreen, 
    NativeStorage,
    ApiService,
    Common,
    { provide: ErrorHandler, useClass: IonicErrorHandler }
  ]
})
export class AppModule { }
