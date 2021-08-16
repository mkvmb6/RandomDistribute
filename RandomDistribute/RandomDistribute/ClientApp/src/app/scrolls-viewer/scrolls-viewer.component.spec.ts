import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ScrollsViewerComponent } from './scrolls-viewer.component';

describe('ScrollsViewerComponent', () => {
  let component: ScrollsViewerComponent;
  let fixture: ComponentFixture<ScrollsViewerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ScrollsViewerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScrollsViewerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
