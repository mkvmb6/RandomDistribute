import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScrollsComponent } from './scrolls.component';

describe('ScrollsComponent', () => {
  let component: ScrollsComponent;
  let fixture: ComponentFixture<ScrollsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScrollsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScrollsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
