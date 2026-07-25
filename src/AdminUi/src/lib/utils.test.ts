import { cn, sanitizeUrl } from './utils';

describe('cn', () => {
    it('returns empty string for no inputs', () => {
        expect(cn()).toBe('');
    });

    it('returns a single class as-is', () => {
        expect(cn('foo')).toBe('foo');
    });

    it('merges multiple classes', () => {
        expect(cn('foo', 'bar', 'baz')).toBe('foo bar baz');
    });

    it('later classes override earlier ones', () => {
        expect(cn('p-2', 'p-4')).toBe('p-4');
    });

    it('handles falsy values', () => {
        expect(cn('foo', false, 'bar', null, undefined, 'baz')).toBe('foo bar baz');
    });

    it('handles conditional classes', () => {
        const isActive = true;
        expect(cn('base', isActive && 'active')).toBe('base active');
    });

    it('handles object class values', () => {
        expect(cn('base', { 'text-red': true, 'text-blue': false })).toBe('base text-red');
    });

    it('handles array of classes', () => {
        expect(cn('base', ['foo', 'bar'], 'baz')).toBe('base foo bar baz');
    });

    it('merges conflicting Tailwind classes correctly', () => {
        expect(cn('text-red-500', 'text-blue-500')).toBe('text-blue-500');
        expect(cn('p-4', 'm-2', 'p-2')).toBe('m-2 p-2');
    });
});

describe('sanitizeUrl', () => {
    it('allows valid http and https URLs', () => {
        expect(sanitizeUrl('http://example.com')).toBe('http://example.com');
        expect(sanitizeUrl('https://github.com/foo/bar')).toBe('https://github.com/foo/bar');
    });

    it('allows relative paths and anchors', () => {
        expect(sanitizeUrl('/projects/123')).toBe('/projects/123');
        expect(sanitizeUrl('#section')).toBe('#section');
        expect(sanitizeUrl('./path')).toBe('./path');
        expect(sanitizeUrl('../path')).toBe('../path');
    });

    it('allows mailto and tel links', () => {
        expect(sanitizeUrl('mailto:test@example.com')).toBe('mailto:test@example.com');
        expect(sanitizeUrl('tel:+1234567890')).toBe('tel:+1234567890');
    });

    it('rejects javascript: URLs (XSS)', () => {
        expect(sanitizeUrl('javascript:alert(1)')).toBeUndefined();
        expect(sanitizeUrl('JAVASCRIPT:alert("xss")')).toBeUndefined();
        expect(sanitizeUrl('  javascript:void(0)')).toBeUndefined();
    });

    it('rejects data: and vbscript: URLs', () => {
        expect(sanitizeUrl('data:text/html,<script>alert(1)</script>')).toBeUndefined();
        expect(sanitizeUrl('vbscript:msgbox(1)')).toBeUndefined();
        expect(sanitizeUrl('file:///etc/passwd')).toBeUndefined();
        expect(sanitizeUrl('blob:http://example.com/uuid')).toBeUndefined();
    });

    it('rejects protocol-relative URLs starting with //', () => {
        expect(sanitizeUrl('//evil.com')).toBeUndefined();
    });

    it('handles empty or nullish inputs', () => {
        expect(sanitizeUrl(null)).toBeUndefined();
        expect(sanitizeUrl(undefined)).toBeUndefined();
        expect(sanitizeUrl('')).toBeUndefined();
        expect(sanitizeUrl('   ')).toBeUndefined();
    });
});

