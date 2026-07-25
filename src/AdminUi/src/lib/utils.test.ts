import { cn } from './utils';

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

