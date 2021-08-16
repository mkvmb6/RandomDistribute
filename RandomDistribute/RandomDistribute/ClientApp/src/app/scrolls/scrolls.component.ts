import { Component, Inject, OnInit } from "@angular/core";
import { RoomService } from "../services/room.service";

@Component({
  selector: "app-scrolls",
  templateUrl: "./scrolls.component.html",
  styleUrls: ["./scrolls.component.css"]
})
export class ScrollsComponent implements OnInit {

  shortUrl = "";
  copiedVisible = false;
  roomOwner = "";
  baseUrl = "";
  errorMessage = "";
  isRequestInProgress = false;

  constructor(private readonly roomService: RoomService, @Inject("BASE_URL") baseUrl: string) {
    this.baseUrl = baseUrl;
  }


  ngOnInit() {
  }



  createRoom() {
    this.isRequestInProgress = true;
    this.shortUrl = "";
    this.errorMessage = "";
    this.roomService.addRoom(JSON.stringify(this.roomOwner))
      .subscribe(
        urlResp => {
          this.shortUrl = this.baseUrl + urlResp["shortUrl"];
          localStorage.setItem("roomOwner", this.roomOwner);
          this.isRequestInProgress = false;
        },
        error => {
          this.isRequestInProgress = false;
          this.errorMessage = error.error;
        });
  }

  copyShortUrl() {
    const selBox = document.createElement("textarea");
    selBox.style.position = "fixed";
    selBox.style.left = "0";
    selBox.style.top = "0";
    selBox.style.opacity = "0";
    selBox.value = this.shortUrl;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand("copy");
    document.body.removeChild(selBox);
    this.copiedVisible = true;
    setTimeout(() => {
        this.copiedVisible = false;
      },
      1000);
  }

}
