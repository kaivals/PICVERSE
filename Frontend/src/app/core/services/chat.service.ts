import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Chat {
  id: number;
  name?: string;
  isGroup: boolean;
  participants: ChatParticipant[];
  lastMessage?: Message;
  unreadCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ChatParticipant {
  id: number;
  userId: number;
  user: {
    id: number;
    username: string;
    firstName?: string;
    lastName?: string;
    profilePictureUrl?: string;
  };
  joinedAt: string;
  lastReadAt?: string;
  isActive: boolean;
}

export interface Message {
  id: number;
  chatId: number;
  senderId: number;
  senderName: string;
  senderAvatar?: string;
  content: string;
  type: string;
  mediaUrl?: string;
  isRead: boolean;
  isEdited: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateChatRequest {
  participantIds: number[];
  name?: string;
  isGroup?: boolean;
}

export interface SendMessageRequest {
  content: string;
  type?: string;
  mediaUrl?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly apiUrl = `${environment.apiUrl}/chats`;
  private chatsSubject = new BehaviorSubject<Chat[]>([]);
  private activeChat = new BehaviorSubject<Chat | null>(null);

  public chats$ = this.chatsSubject.asObservable();
  public activeChat$ = this.activeChat.asObservable();

  constructor(private http: HttpClient) {}

  getChats(page: number = 1, pageSize: number = 20): Observable<PaginatedResponse<Chat>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Chat>>(this.apiUrl, { params });
  }

  createChat(request: CreateChatRequest): Observable<Chat> {
    return this.http.post<Chat>(this.apiUrl, request);
  }

  getChat(id: number): Observable<Chat> {
    return this.http.get<Chat>(`${this.apiUrl}/${id}`);
  }

  getMessages(chatId: number, page: number = 1, pageSize: number = 50): Observable<PaginatedResponse<Message>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Message>>(`${this.apiUrl}/${chatId}/messages`, { params });
  }

  sendMessage(chatId: number, request: SendMessageRequest): Observable<Message> {
    return this.http.post<Message>(`${this.apiUrl}/${chatId}/messages`, request);
  }

  markMessageAsRead(messageId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/messages/${messageId}/read`, {});
  }

  deleteMessage(messageId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/messages/${messageId}`);
  }

  setActiveChat(chat: Chat | null): void {
    this.activeChat.next(chat);
  }

  updateChats(chats: Chat[]): void {
    this.chatsSubject.next(chats);
  }

  addNewMessage(message: Message): void {
    const currentChats = this.chatsSubject.value;
    const chatIndex = currentChats.findIndex(c => c.id === message.chatId);
    
    if (chatIndex !== -1) {
      const updatedChats = [...currentChats];
      updatedChats[chatIndex] = {
        ...updatedChats[chatIndex],
        lastMessage: message,
        updatedAt: message.createdAt
      };
      
      // Move chat to top
      const [updatedChat] = updatedChats.splice(chatIndex, 1);
      updatedChats.unshift(updatedChat);
      
      this.chatsSubject.next(updatedChats);
    }
  }
}