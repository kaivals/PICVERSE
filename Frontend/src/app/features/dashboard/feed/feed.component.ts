import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { PostService, Post } from '../../../core/services/post.service';
import { AuthService, User } from '../../../core/services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-feed',
  templateUrl: './feed.component.html',
  styleUrls: ['./feed.component.scss']
})
export class FeedComponent implements OnInit, OnDestroy {
  posts: Post[] = [];
  currentUser: User | null = null;
  isLoading = false;
  hasMorePosts = true;
  currentPage = 1;
  pageSize = 10;
  
  private destroy$ = new Subject<void>();

  constructor(
    private postService: PostService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
      });

    this.loadPosts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPosts(loadMore: boolean = false): void {
    if (this.isLoading) return;

    this.isLoading = true;
    const page = loadMore ? this.currentPage + 1 : 1;

    this.postService.getPosts(page, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (loadMore) {
            this.posts = [...this.posts, ...response.items];
            this.currentPage = page;
          } else {
            this.posts = response.items;
            this.currentPage = 1;
          }
          
          this.hasMorePosts = response.items.length === this.pageSize;
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.snackBar.open('Failed to load posts', 'Close', { duration: 3000 });
        }
      });
  }

  onLoadMore(): void {
    this.loadPosts(true);
  }

  onPostLike(post: Post): void {
    this.postService.toggleLike(post.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          post.isLiked = response.isLiked;
          post.likesCount = response.likesCount;
        },
        error: (error) => {
          this.snackBar.open('Failed to update like', 'Close', { duration: 3000 });
        }
      });
  }

  onPostComment(post: Post): void {
    // Navigate to post detail or open comment dialog
    console.log('Comment on post:', post.id);
  }

  onPostShare(post: Post): void {
    // Implement share functionality
    console.log('Share post:', post.id);
  }

  onRefresh(): void {
    this.loadPosts();
  }

  trackByPostId(index: number, post: Post): number {
    return post.id;
  }
}