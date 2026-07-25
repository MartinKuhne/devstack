import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
    return twMerge(clsx(inputs));
}

/**
 * Sanitizes a URL string to ensure it is safe for use in `href` attributes.
 * Allows safe protocols (http, https, mailto, tel) and relative paths.
 * Rejects javascript:, data:, vbscript:, and other unsafe schemes.
 */
export function sanitizeUrl(url?: string | null): string | undefined {
    if (!url) return undefined;

    const trimmed = url.trim();
    if (!trimmed) return undefined;

    if (trimmed.startsWith('//')) {
        return undefined;
    }

    const normalized = trimmed.toLowerCase();
    if (
        normalized.startsWith('javascript:') ||
        normalized.startsWith('vbscript:') ||
        normalized.startsWith('data:') ||
        normalized.startsWith('file:') ||
        normalized.startsWith('blob:')
    ) {
        return undefined;
    }

    if (trimmed.startsWith('/')) {
        return trimmed;
    }
    if (trimmed.startsWith('#') || trimmed.startsWith('./') || trimmed.startsWith('../')) {
        return trimmed;
    }

    try {
        const parsed = new URL(trimmed, 'https://dummy.invalid');
        const allowedProtocols = ['http:', 'https:', 'mailto:', 'tel:'];
        if (allowedProtocols.includes(parsed.protocol)) {
            return trimmed;
        }
    } catch {
        return undefined;
    }

    return undefined;
}

