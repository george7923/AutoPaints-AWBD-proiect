import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TalkChatBotComponent } from './talk-chat-bot.component';

describe('TalkChatBotComponent', () => {
  let component: TalkChatBotComponent;
  let fixture: ComponentFixture<TalkChatBotComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TalkChatBotComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(TalkChatBotComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
