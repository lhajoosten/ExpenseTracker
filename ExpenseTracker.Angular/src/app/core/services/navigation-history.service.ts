import { Injectable } from '@angular/core';
import { Router, NavigationEnd, Event, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class NavigationHistoryService {
    private history: string[] = [];
    private maxHistoryLength = 10;
    private ignoredPaths: string[] = ['not-found', '404']; // Paths to ignore

    constructor(private router: Router) {
        // Subscribe to router events to track navigation
        this.router.events.pipe(
            filter((event: Event): event is NavigationEnd => event instanceof NavigationEnd)
        ).subscribe((event: NavigationEnd) => {
            // Store URL without query parameters
            const url = this.cleanUrl(event.urlAfterRedirects);

            // Skip adding ignored paths to history
            if (this.shouldIgnoreUrl(url)) {
                return;
            }

            // Only add to history if it's not the same as the last entry
            if (this.history.length === 0 || this.history[this.history.length - 1] !== url) {
                this.addToHistory(url);
            }
        });
    }

    /**
     * Check if URL should be ignored in history
     */
    private shouldIgnoreUrl(url: string): boolean {
        return this.ignoredPaths.some(path => url.includes(path));
    }

    /**
     * Add URL to history, maintaining the maximum history length
     */
    private addToHistory(url: string): void {
        this.history.push(url);

        // Trim history if it exceeds the max length
        if (this.history.length > this.maxHistoryLength) {
            this.history.shift(); // Remove oldest entry
        }
    }

    /**
     * Get the previous URL in the navigation history
     * @returns The previous URL or null if no previous history exists
     */
    getPreviousUrl(): string | null {
        // If we have at least 2 items in history, return the second-last one
        if (this.history.length > 1) {
            return this.history[this.history.length - 2];
        }
        return null;
    }

    /**
     * Get the current URL in the navigation history
     */
    getCurrentUrl(): string | null {
        if (this.history.length > 0) {
            return this.history[this.history.length - 1];
        }
        return null;
    }

    /**
     * Get the full navigation history
     */
    getHistory(): string[] {
        return [...this.history];
    }

    /**
     * Clear the navigation history
     */
    clearHistory(): void {
        this.history = [];
    }

    /**
     * Remove query parameters from URL for consistency
     */
    private cleanUrl(url: string): string {
        const questionMarkIndex = url.indexOf('?');
        if (questionMarkIndex !== -1) {
            return url.substring(0, questionMarkIndex);
        }
        return url;
    }
}