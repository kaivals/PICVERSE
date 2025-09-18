import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserProfile {
  id: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  bio?: string;
  profilePictureUrl?: string;
  coverPictureUrl?: string;
  dateOfBirth: string;
  isPrivate: boolean;
  followersCount: number;
  followingCount: number;
  postsCount: number;
  isFollowing?: boolean;
  isFollowedBy?: boolean;
  createdAt: string;
}

export interface UpdateProfileRequest {
  firstName?: string;
  lastName?: string;
  bio?: string;
  dateOfBirth?: string;
  isPrivate?: boolean;
}

export interface UserSearchResult {
  id: number;
  username: string;
  firstName?: string;
  lastName?: string;
  profilePictureUrl?: string;
  isFollowing: boolean;
  followersCount: number;
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
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiUrl}/profile`, request);
  }

  searchUsers(query: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<UserSearchResult>> {
    const params = new HttpParams()
      .set('query', query)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<UserSearchResult>>(`${this.apiUrl}/search`, { params });
  }

  getUserById(id: number): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/${id}`);
  }

  toggleFollow(id: number): Observable<{ isFollowing: boolean; followersCount: number }> {
    return this.http.post<{ isFollowing: boolean; followersCount: number }>(`${this.apiUrl}/${id}/follow`, {});
  }

  getFollowers(id: number, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<UserSearchResult>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<UserSearchResult>>(`${this.apiUrl}/${id}/followers`, { params });
  }

  getFollowing(id: number, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<UserSearchResult>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<UserSearchResult>>(`${this.apiUrl}/${id}/following`, { params });
  }

  getUserPosts(id: number, page: number = 1, pageSize: number = 10): Observable<any> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get(`${this.apiUrl}/${id}/posts`, { params });
  }
}