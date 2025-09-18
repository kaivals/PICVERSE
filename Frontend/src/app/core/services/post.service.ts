import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Post {
  id: number;
  userId: number;
  content?: string;
  imageUrl?: string;
  videoUrl?: string;
  likesCount: number;
  commentsCount: number;
  sharesCount: number;
  isLiked: boolean;
  createdAt: string;
  updatedAt: string;
  user: {
    id: number;
    username: string;
    firstName?: string;
    lastName?: string;
    profilePictureUrl?: string;
  };
  media: PostMedia[];
}

export interface PostMedia {
  id: number;
  mediaUrl: string;
  mediaType: string;
  order: number;
}

export interface Comment {
  id: number;
  userId: number;
  postId: number;
  parentCommentId?: number;
  content: string;
  likesCount: number;
  isLiked: boolean;
  createdAt: string;
  updatedAt: string;
  user: {
    id: number;
    username: string;
    firstName?: string;
    lastName?: string;
    profilePictureUrl?: string;
  };
  replies: Comment[];
}

export interface CreatePostRequest {
  content?: string;
  mediaUrls?: string[];
}

export interface UpdatePostRequest {
  content?: string;
}

export interface CreateCommentRequest {
  content: string;
  parentCommentId?: number;
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
export class PostService {
  private readonly apiUrl = `${environment.apiUrl}/posts`;

  constructor(private http: HttpClient) {}

  getPosts(page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<Post>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Post>>(this.apiUrl, { params });
  }

  getPost(id: number): Observable<Post> {
    return this.http.get<Post>(`${this.apiUrl}/${id}`);
  }

  createPost(request: CreatePostRequest): Observable<Post> {
    return this.http.post<Post>(this.apiUrl, request);
  }

  updatePost(id: number, request: UpdatePostRequest): Observable<Post> {
    return this.http.put<Post>(`${this.apiUrl}/${id}`, request);
  }

  deletePost(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  toggleLike(id: number): Observable<{ isLiked: boolean; likesCount: number }> {
    return this.http.post<{ isLiked: boolean; likesCount: number }>(`${this.apiUrl}/${id}/like`, {});
  }

  getComments(postId: number, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<Comment>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<Comment>>(`${this.apiUrl}/${postId}/comments`, { params });
  }

  createComment(postId: number, request: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${postId}/comments`, request);
  }

  updateComment(commentId: number, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${environment.apiUrl}/comments/${commentId}`, { content });
  }

  deleteComment(commentId: number): Observable<any> {
    return this.http.delete(`${environment.apiUrl}/comments/${commentId}`);
  }

  toggleCommentLike(commentId: number): Observable<{ isLiked: boolean; likesCount: number }> {
    return this.http.post<{ isLiked: boolean; likesCount: number }>(`${environment.apiUrl}/comments/${commentId}/like`, {});
  }
}