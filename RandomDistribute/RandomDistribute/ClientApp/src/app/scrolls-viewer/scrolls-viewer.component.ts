import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Room } from "../models/room";
import { RoomService } from "../services/room.service";

@Component({
  selector: "app-scrolls-viewer",
  templateUrl: "./scrolls-viewer.component.html",
  styleUrls: ["./scrolls-viewer.component.css"]
})
export class ScrollsViewerComponent implements OnInit {
  scrollImageCount = 1;
  isScrollOpen = true;
  showScrollText = false;
  roomStatus = "";
  isOwner = false;
  userName = "";
  roomUser = "";
  roomInfo: Room;
  shortUrl = "";
  chits = "";
  playerNames = "";

  constructor(private route: ActivatedRoute, private roomService: RoomService) {}

  ngOnInit() {
    this.shortUrl = this.route.snapshot.paramMap.get("shortUrl");
    this.roomService.getRoom(this.shortUrl)
      .subscribe(
        roomResp => {
          this.roomInfo = roomResp;
          console.log(roomResp);
          const roomOwner = localStorage.getItem("roomOwner");
          const roomUser = localStorage.getItem("roomUser");
          if (roomOwner === roomResp.roomOwner) {
            this.isOwner = true;
            this.userName = roomOwner;
          } else if (roomUser) {
            this.userName = roomUser;
            if (roomResp.users[roomUser] == null) {
              this.roomService.addUserToRoom(this.shortUrl, JSON.stringify(roomUser)).subscribe(roomResp => {
                  console.log(roomResp);
                },
                error => {
                  this.roomStatus = error.error;
                });
            }
          }
        },
        error => {
          this.roomStatus = error.error;
        });
  }

  getPlayerNames() {
    let names = "";
    const users = this.roomInfo.users;
    for (let user in users) {
      if (Object.prototype.hasOwnProperty.call(users, user)) {
        names += user + ", ";
      }
    }
    return names.substr(0, names.length - 2);
  }

  createUser() {
    this.roomService.addUserToRoom(this.shortUrl, JSON.stringify(this.roomUser))
      .subscribe(
        roomResp => {
          console.log(roomResp);
          this.roomInfo = roomResp; 
          this.userName = this.roomUser;
          localStorage.setItem("roomUser", this.roomUser);
        },
        error => {
          this.roomStatus = error.error;
        }
      );
  }

  toggleScroll() {
    this.isScrollOpen = !this.isScrollOpen;
    var intervalHandler = setInterval(() => {
        if (this.isScrollOpen) {
          this.showScrollText = false;
          if (this.scrollImageCount <= 1) {
            clearInterval(intervalHandler);
            return;
          }
          this.scrollImageCount--;
        } else {
          if (this.scrollImageCount >= 7) {
            clearInterval(intervalHandler);
            this.showScrollText = true;
            return;
          }
          this.scrollImageCount++;
        }
      },
      100);
  }

  closeScroll() {
    this.isScrollOpen = false;
    this.toggleScroll();
  }

  distributeChits() {
    this.roomService.assignChitsToUsers(this.shortUrl, this.chits.split(/[\n;,]+/))
      .subscribe(roomResp => {
          this.roomInfo = roomResp;
          this.closeScroll();
        },
        error => {
          this.roomStatus = error.error;
        });
  }

  refreshChit() {
    this.roomService.getRoom(this.shortUrl)
      .subscribe(
        roomResp => {
          this.roomInfo = roomResp;
        },
        error => {
          this.roomStatus = error.error;
        });
    this.closeScroll();
  }
}
