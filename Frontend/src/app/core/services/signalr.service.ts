import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { ChatService, Message } from './chat.service';

export interface OnlineUser {
  userId: number;
  isOnline: boolean;
}

export interface TypingUser {
  chatId: number;
  userId: number;
  isTyping: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private connectionState = new BehaviorSubject<string>('Disconnected');
  private onlineUsers = new BehaviorSubject<Set<number>>(new Set());
  private typingUsers = new BehaviorSubject<Map<number, Set<number>>>(new Map());

  public connectionState$ = this.connectionState.asObservable();
  public onlineUsers$ = this.onlineUsers.asObservable();
  public typingUsers$ = this.typingUsers.asObservable();

  constructor(
    private authService: AuthService,
    private chatService: ChatService
  ) {
    this.authService.isAuthenticated$.subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.startConnection();
      } else {
        this.stopConnection();
      }
    });
  }

  private async startConnection(): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      return;
    }

    const accessToken = this.authService.getAccessToken();
    if (!accessToken) {
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/chat`, {
        accessTokenFactory: () => accessToken
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.setupEventHandlers();

    try {
      await this.hubConnection.start();
      this.connectionState.next('Connected');
      console.log('SignalR Connected');
    } catch (error) {
      console.error('SignalR Connection Error:', error);
      this.connectionState.next('Disconnected');
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Connection events
    this.hubConnection.onreconnecting(() => {
      this.connectionState.next('Reconnecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState.next('Connected');
    });

    this.hubConnection.onclose(() => {
      this.connectionState.next('Disconnected');
    });

    // User online/offline events
    this.hubConnection.on('UserOnline', (userId: number) => {
      const currentOnlineUsers = this.onlineUsers.value;
      currentOnlineUsers.add(userId);
      this.onlineUsers.next(new Set(currentOnlineUsers));
    });

    this.hubConnection.on('UserOffline', (userId: number) => {
      const currentOnlineUsers = this.onlineUsers.value;
      currentOnlineUsers.delete(userId);
      this.onlineUsers.next(new Set(currentOnlineUsers));
    });

    // Chat events
    this.hubConnection.on('JoinedChat', (chatId: number) => {
      console.log(`Joined chat ${chatId}`);
    });

    this.hubConnection.on('LeftChat', (chatId: number) => {
      console.log(`Left chat ${chatId}`);
    });

    this.hubConnection.on('ReceiveMessage', (message: Message) => {
      this.chatService.addNewMessage(message);
    });

    // Typing events
    this.hubConnection.on('UserTyping', (chatId: number, userId: number, isTyping: boolean) => {
      const currentTypingUsers = this.typingUsers.value;
      
      if (!currentTypingUsers.has(chatId)) {
        currentTypingUsers.set(chatId, new Set());
      }
      
      const chatTypingUsers = currentTypingUsers.get(chatId)!;
      
      if (isTyping) {
        chatTypingUsers.add(userId);
      } else {
        chatTypingUsers.delete(userId);
      }
      
      this.typingUsers.next(new Map(currentTypingUsers));
    });

    // Error events
    this.hubConnection.on('Error', (error: string) => {
      console.error('SignalR Error:', error);
    });
  }

  async joinChat(chatId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('JoinChat', chatId);
      } catch (error) {
        console.error('Error joining chat:', error);
      }
    }
  }

  async leaveChat(chatId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('LeaveChat', chatId);
      } catch (error) {
        console.error('Error leaving chat:', error);
      }
    }
  }

  async sendMessage(chatId: number, content: string, messageType: string = 'text'): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('SendMessage', chatId, content, messageType);
      } catch (error) {
        console.error('Error sending message:', error);
      }
    }
  }

  async startTyping(chatId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('TypingStart', chatId);
      } catch (error) {
        console.error('Error sending typing start:', error);
      }
    }
  }

  async stopTyping(chatId: number): Promise<void> {
    if (this.hubConnection?.state === 'Connected') {
      try {
        await this.hubConnection.invoke('TypingStop', chatId);
      } catch (error) {
        console.error('Error sending typing stop:', error);
      }
    }
  }

  private async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this.connectionState.next('Disconnected');
        console.log('SignalR Disconnected');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  isUserOnline(userId: number): boolean {
    return this.onlineUsers.value.has(userId);
  }

  getChatTypingUsers(chatId: number): Set<number> {
    return this.typingUsers.value.get(chatId) || new Set();
  }
}