import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Room } from "../models/room";

@Injectable({
  providedIn: "root"
})
export class RoomService {
  private baseUrl = "";
  private controllerUrl = "api/room/";

  constructor(private readonly httpClient: HttpClient, @Inject("BASE_URL") baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  addRoom(roomOwner: string) {
    const headers = new HttpHeaders().set("Content-Type", "application/json");
    return this.httpClient.post(this.baseUrl + this.controllerUrl + "addRoom", roomOwner, { headers: headers });
  }

  addUserToRoom(shortUrl: string, roomUser: string): Observable<Room> {
    const headers = new HttpHeaders().set("Content-Type", "application/json");
    return this.httpClient.put<Room>(this.baseUrl + this.controllerUrl + shortUrl, roomUser, { headers: headers });
  }


  assignChitsToUsers(shortUrl: string, chits: string[]): Observable<Room> {
    const headers = new HttpHeaders().set("Content-Type", "application/json");
    return this.httpClient.put<Room>(this.baseUrl + this.controllerUrl + "assignChitsToUsers/" + shortUrl,
      chits,
      { headers: headers });
  }

  getRoom(shortUrl: string): Observable<Room> {
    return this.httpClient.get<Room>(this.baseUrl + this.controllerUrl + shortUrl);
  }
}
