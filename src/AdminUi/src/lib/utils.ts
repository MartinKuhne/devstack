import { type ClassValue, clsx } from 'clsx';
import DOMPurify from 'dompurify';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

/**
 * Sanitizes a URL string using DOMPurify to ensure it is safe for use in `href` attributes.
 * Allows safe protocols (http, https, mailto, tel) and relative paths.
 * Rejects javascript:, data:, vbscript:, and other unsafe schemes.
 */
export function sanitizeUrl(url?: string | null): string | undefined {
    if (!url) return undefined;

    const trimmed = url.trim();
    if (!trimmed || trimmed.startsWith('//')) return undefined;

    // Use DOMPurify to check if the href attribute value is safe from XSS / execution schemes
    if (!DOMPurify.isValidAttribute('a', 'href', trimmed)) {
        return undefined;
    }

    return trimmed;
}

