import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MarkdownViewer } from './MarkdownViewer';

describe('MarkdownViewer', () => {
    describe('headings', () => {
        it('renders h1', () => {
            render(<MarkdownViewer content="# Heading 1" />);
            expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument();
            expect(screen.getByText('Heading 1')).toBeInTheDocument();
        });

        it('renders h2', () => {
            render(<MarkdownViewer content="## Heading 2" />);
            expect(screen.getByRole('heading', { level: 2 })).toBeInTheDocument();
            expect(screen.getByText('Heading 2')).toBeInTheDocument();
        });

        it('renders h3', () => {
            render(<MarkdownViewer content="### Heading 3" />);
            expect(screen.getByRole('heading', { level: 3 })).toBeInTheDocument();
            expect(screen.getByText('Heading 3')).toBeInTheDocument();
        });

        it('renders h4', () => {
            render(<MarkdownViewer content="#### Heading 4" />);
            expect(screen.getByRole('heading', { level: 4 })).toBeInTheDocument();
            expect(screen.getByText('Heading 4')).toBeInTheDocument();
        });

        it('renders h5', () => {
            render(<MarkdownViewer content="##### Heading 5" />);
            expect(screen.getByRole('heading', { level: 5 })).toBeInTheDocument();
            expect(screen.getByText('Heading 5')).toBeInTheDocument();
        });

        it('renders h6', () => {
            render(<MarkdownViewer content="###### Heading 6" />);
            expect(screen.getByRole('heading', { level: 6 })).toBeInTheDocument();
            expect(screen.getByText('Heading 6')).toBeInTheDocument();
        });

        it('renders setext heading (section 4.3)', () => {
            render(<MarkdownViewer content={"Setext Heading\n==="} />);
            expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Setext Heading');
        });

        it('renders setext h2 (section 4.3)', () => {
            render(<MarkdownViewer content={"Setext H2\n---"} />);
            expect(screen.getByRole('heading', { level: 2 })).toHaveTextContent('Setext H2');
        });
    });

    describe('bold and italic', () => {
        it('renders bold text', () => {
            render(<MarkdownViewer content="**bold text**" />);
            expect(screen.getByText('bold text')).toBeInTheDocument();
        });

        it('renders italic text', () => {
            render(<MarkdownViewer content="*italic text*" />);
            expect(screen.getByText('italic text')).toBeInTheDocument();
        });

        it('renders bold and italic text', () => {
            render(<MarkdownViewer content="***bold and italic***" />);
            expect(screen.getByText('bold and italic')).toBeInTheDocument();
        });
    });

    describe('lists', () => {
        it('renders unordered list', () => {
            const content = '- Item 1\n- Item 2\n- Item 3';
            render(<MarkdownViewer content={content} />);
            const items = screen.getAllByRole('listitem');
            expect(items).toHaveLength(3);
            expect(items[0]).toHaveTextContent('Item 1');
            expect(items[1]).toHaveTextContent('Item 2');
            expect(items[2]).toHaveTextContent('Item 3');
        });

        it('renders ordered list', () => {
            const content = '1. First\n2. Second\n3. Third';
            render(<MarkdownViewer content={content} />);
            const items = screen.getAllByRole('listitem');
            expect(items).toHaveLength(3);
            expect(items[0]).toHaveTextContent('First');
            expect(items[1]).toHaveTextContent('Second');
            expect(items[2]).toHaveTextContent('Third');
        });

        it('renders nested unordered list', () => {
            const content = '- Item 1\n  - Subitem 1.1\n  - Subitem 1.2\n- Item 2';
            render(<MarkdownViewer content={content} />);
            const items = screen.getAllByRole('listitem');
            expect(items).toHaveLength(4);
        });

        it('renders ordered list with start attribute (section 5.2)', () => {
            const { container } = render(<MarkdownViewer content={"3. Third\n4. Fourth"} />);
            const ol = container.querySelector('ol');
            expect(ol).toHaveAttribute('start', '3');
        });

        it('renders mixed ordered/unordered nesting (section 5.2)', () => {
            const content = '1. Ordered item\n   - Unordered nested\n   - Another nested\n2. Second ordered';
            render(<MarkdownViewer content={content} />);
            const items = screen.getAllByRole('listitem');
            expect(items.length).toBeGreaterThanOrEqual(4);
        });
    });

    describe('tables', () => {
        it('renders table with header and body', () => {
            const content = '| Column 1 | Column 2 |\n|----------|----------|\n| Value 1  | Value 2  |';
            render(<MarkdownViewer content={content} />);
            expect(screen.getByText('Column 1')).toBeInTheDocument();
            expect(screen.getByText('Column 2')).toBeInTheDocument();
            expect(screen.getByText('Value 1')).toBeInTheDocument();
            expect(screen.getByText('Value 2')).toBeInTheDocument();
        });

        it('renders table with alignment', () => {
            const content = '| Left | Center | Right |\n|:-----|:------:|------:|\n| A    | B      | C     |';
            render(<MarkdownViewer content={content} />);
            expect(screen.getByText('Left')).toBeInTheDocument();
            expect(screen.getByText('Center')).toBeInTheDocument();
            expect(screen.getByText('Right')).toBeInTheDocument();
        });
    });

    describe('task lists (GFM)', () => {
        it('renders completed task item with checked input', () => {
            const { container } = render(<MarkdownViewer content="- [x] Completed task" />);
            expect(screen.getByText('Completed task')).toBeInTheDocument();
            const checkbox = container.querySelector('input[type="checkbox"]');
            expect(checkbox).toBeChecked();
        });

        it('renders incomplete task item with unchecked input', () => {
            const { container } = render(<MarkdownViewer content="- [ ] Incomplete task" />);
            expect(screen.getByText('Incomplete task')).toBeInTheDocument();
            const checkbox = container.querySelector('input[type="checkbox"]');
            expect(checkbox).not.toBeChecked();
        });
    });

    describe('links', () => {
        it('renders link with target="_blank" and rel="noopener noreferrer"', () => {
            render(<MarkdownViewer content="[Click here](https://example.com)" />);
            const link = screen.getByText('Click here');
            const anchor = link.closest('a');
            expect(anchor).toHaveAttribute('href', 'https://example.com');
            expect(anchor).toHaveAttribute('target', '_blank');
            expect(anchor).toHaveAttribute('rel', 'noopener noreferrer');
        });

        it('renders internal link without target and rel', () => {
            render(<MarkdownViewer content="[Internal](/internal-page)" />);
            const internalLink = document.querySelector('a[href="/internal-page"]');
            expect(internalLink).toBeInTheDocument();
            expect(internalLink).not.toHaveAttribute('target', '_blank');
            expect(internalLink).not.toHaveAttribute('rel', 'noopener noreferrer');
        });

        it('renders inline link with title (section 6.3)', () => {
            render(<MarkdownViewer content='[Docs](https://example.com "my title")' />);
            const anchor = screen.getByText('Docs').closest('a');
            expect(anchor).toHaveAttribute('title', 'my title');
        });

        it('renders reference link with title (section 4.7)', () => {
            const content = '[Docs][ref]\n\n[ref]: https://example.com "Docs title"';
            render(<MarkdownViewer content={content} />);
            const anchor = screen.getByText('Docs').closest('a');
            expect(anchor).toHaveAttribute('href', 'https://example.com');
            expect(anchor).toHaveAttribute('title', 'Docs title');
        });

        it('renders collapsed reference link [foo][] (section 6.3)', () => {
            const content = '[example][]\n\n[example]: https://example.com';
            render(<MarkdownViewer content={content} />);
            const anchor = screen.getByText('example').closest('a');
            expect(anchor).toHaveAttribute('href', 'https://example.com');
        });

        it('renders shortcut reference link [foo] (section 6.3)', () => {
            const content = '[example]\n\n[example]: https://example.com';
            render(<MarkdownViewer content={content} />);
            const anchor = screen.getByText('example').closest('a');
            expect(anchor).toHaveAttribute('href', 'https://example.com');
        });

        it('adds a11y screen reader text for external links', () => {
            render(<MarkdownViewer content="[External](https://example.com)" />);
            expect(screen.getByText('(opens in new tab)')).toBeInTheDocument();
        });

        it('does not add a11y text for internal links', () => {
            render(<MarkdownViewer content="[Internal](/page)" />);
            expect(screen.queryByText('(opens in new tab)')).not.toBeInTheDocument();
        });

        it('blocks javascript: protocol in links (section 6.3 / XSS)', () => {
            render(<MarkdownViewer content="[x](javascript:alert(1))" />);
            const anchors = document.querySelectorAll('a');
            for (const a of anchors) {
                expect(a.getAttribute('href')).not.toContain('javascript:');
            }
        });
    });

    describe('images', () => {
        it('renders image (section 6.4)', () => {
            render(<MarkdownViewer content="![Alt text](https://example.com/img.png)" />);
            const img = screen.getByRole('img', { name: 'Alt text' });
            expect(img).toHaveAttribute('src', 'https://example.com/img.png');
        });

        it('renders image with lazy loading and max-width', () => {
            render(<MarkdownViewer content="![Alt](https://example.com/img.png)" />);
            const img = screen.getByRole('img', { name: 'Alt' });
            expect(img).toHaveAttribute('loading', 'lazy');
            expect(img).toHaveAttribute('decoding', 'async');
            expect(img.className).toContain('max-w-full');
        });
    });

    describe('autolinks', () => {
        it('renders autolink (section 6.5)', () => {
            render(<MarkdownViewer content="<https://example.com>" />);
            const anchor = document.querySelector('a[href="https://example.com"]');
            expect(anchor).toBeInTheDocument();
        });

        it('renders GFM plain-URL autolink', () => {
            render(<MarkdownViewer content="Visit https://example.com for details" />);
            const anchor = document.querySelector('a[href="https://example.com"]');
            expect(anchor).toBeInTheDocument();
        });
    });

    describe('code blocks', () => {
        it('renders inline code (section 6.1)', () => {
            render(<MarkdownViewer content="Use `console.log` to debug" />);
            expect(screen.getByText('console.log')).toBeInTheDocument();
        });

        it('renders fenced code block (section 4.5)', () => {
            const content = '```javascript\nconst x = 42;\n```';
            render(<MarkdownViewer content={content} />);
            const codeBlocks = document.querySelectorAll('code');
            expect(codeBlocks.length).toBeGreaterThan(0);
            const jsCode = Array.from(codeBlocks).find(c => c.classList.contains('language-javascript'));
            expect(jsCode).toBeInTheDocument();
            expect(jsCode?.textContent).toContain('const');
            expect(jsCode?.textContent).toContain('42');
        });

        it('renders fenced code block with different language', () => {
            const content = '```python\nprint("hello")\n```';
            render(<MarkdownViewer content={content} />);
            const codeBlocks = document.querySelectorAll('code');
            expect(codeBlocks.length).toBeGreaterThan(0);
            const pyCode = Array.from(codeBlocks).find(c => c.classList.contains('language-python'));
            expect(pyCode).toBeInTheDocument();
            expect(pyCode?.textContent).toContain('print');
            expect(pyCode?.textContent).toContain('hello');
        });

        it('renders indented code block (section 4.4)', () => {
            const content = '    const x = 42;\n    const y = 10;';
            render(<MarkdownViewer content={content} />);
            const codeEl = document.querySelector('pre code');
            expect(codeEl).toBeInTheDocument();
            expect(codeEl?.textContent).toContain('const x = 42');
        });
    });

    describe('thematic break (section 4.1)', () => {
        it('renders --- as horizontal rule', () => {
            const { container } = render(<MarkdownViewer content={"Above\n\n---\n\nBelow"} />);
            expect(container.querySelector('hr')).toBeInTheDocument();
        });

        it('renders *** as horizontal rule', () => {
            const { container } = render(<MarkdownViewer content={"Above\n\n***\n\nBelow"} />);
            expect(container.querySelector('hr')).toBeInTheDocument();
        });

        it('renders ___ as horizontal rule', () => {
            const { container } = render(<MarkdownViewer content={"Above\n\n___\n\nBelow"} />);
            expect(container.querySelector('hr')).toBeInTheDocument();
        });
    });

    describe('block quotes (section 5.1)', () => {
        it('renders block quote', () => {
            render(<MarkdownViewer content="> This is a quote" />);
            const blockquote = document.querySelector('blockquote');
            expect(blockquote).toBeInTheDocument();
            expect(blockquote).toHaveTextContent('This is a quote');
        });

        it('renders nested block quote', () => {
            const { container } = render(<MarkdownViewer content={"> Outer\n> > Inner"} />);
            const blockquotes = container.querySelectorAll('blockquote');
            expect(blockquotes.length).toBeGreaterThanOrEqual(2);
        });
    });

    describe('line breaks (sections 6.7, 6.8)', () => {
        it('renders hard line break with two trailing spaces', () => {
            const { container } = render(<MarkdownViewer content={"Line 1  \nLine 2"} />);
            const br = container.querySelector('br');
            expect(br).toBeInTheDocument();
        });

        it('renders hard line break with backslash', () => {
            const { container } = render(<MarkdownViewer content={"Line 1\\\nLine 2"} />);
            const br = container.querySelector('br');
            expect(br).toBeInTheDocument();
        });
    });

    describe('backslash escapes (section 2.4)', () => {
        it('renders escaped special characters as literal text', () => {
            render(<MarkdownViewer content={"\\*not italic\\*"} />);
            expect(screen.getByText('*not italic*')).toBeInTheDocument();
        });
    });

    describe('entity and numeric character references (section 2.5)', () => {
        it('renders named entity references', () => {
            const { container } = render(<MarkdownViewer content="&amp; &copy;" />);
            expect(container.textContent).toContain('&');
            expect(container.textContent).toContain('©');
        });

        it('renders numeric character references', () => {
            const { container } = render(<MarkdownViewer content="&#65;" />);
            expect(container.textContent).toContain('A');
        });
    });

    describe('strikethrough (GFM)', () => {
        it('renders strikethrough text', () => {
            const { container } = render(<MarkdownViewer content="~~deleted~~" />);
            const del = container.querySelector('del');
            expect(del).toBeInTheDocument();
            expect(del).toHaveTextContent('deleted');
        });
    });

    describe('escaped HTML', () => {
        it('escapes script tags', () => {
            render(<MarkdownViewer content="<script>alert('xss')</script>" />);
            expect(screen.queryByText("alert('xss')")).not.toBeInTheDocument();
            expect(screen.getByText('<script>alert(\'xss\')</script>')).toBeInTheDocument();
        });

        it('escapes img tags', () => {
            render(<MarkdownViewer content="<img src=x onerror=alert(1)>" />);
            const images = screen.queryAllByRole('img');
            expect(images).toHaveLength(0);
        });

        it('escapes onclick attributes', () => {
            render(<MarkdownViewer content="<div onclick='alert(1)'>click me</div>" />);
            expect(screen.queryByText('click me')).not.toBeInTheDocument();
        });
    });

    describe('null handling', () => {
        it('returns null when content is undefined', () => {
            const { container } = render(<MarkdownViewer content={undefined} />);
            expect(container.firstChild).toBeNull();
        });

        it('returns null when content is null', () => {
            const { container } = render(<MarkdownViewer content={null} />);
            expect(container.firstChild).toBeNull();
        });

        it('returns null when content is empty string', () => {
            const { container } = render(<MarkdownViewer content="" />);
            expect(container.firstChild).toBeNull();
        });

        it('returns null when content prop is not provided', () => {
            const { container } = render(<MarkdownViewer />);
            expect(container.firstChild).toBeNull();
        });
    });

    describe('className', () => {
        it('applies default prose classes when no className provided', () => {
            const { container } = render(<MarkdownViewer content="Hello" />);
            const wrapper = container.firstChild;
            expect(wrapper).toHaveClass('prose');
            expect(wrapper).toHaveClass('prose-sm');
            expect(wrapper).toHaveClass('dark:prose-invert');
            expect(wrapper).toHaveClass('max-w-none');
        });

        it('applies custom className instead of default', () => {
            const { container } = render(<MarkdownViewer content="Hello" className="custom-class" />);
            expect(container.firstChild).toHaveClass('custom-class');
            expect(container.firstChild).not.toHaveClass('prose');
        });
    });

    describe('console warnings', () => {
        it('does not log React warnings about invalid DOM props', () => {
            const spy = vi.spyOn(console, 'error').mockImplementation(() => {});
            render(<MarkdownViewer content="[test](https://example.com)" />);
            const reactWarnings = spy.mock.calls.filter(
                call => typeof call[0] === 'string' && call[0].includes('React does not recognize'),
            );
            expect(reactWarnings).toHaveLength(0);
            spy.mockRestore();
        });
    });

    describe('mixed content', () => {
        it('renders complex markdown with multiple elements', () => {
            const content = `# Title

**Bold** and *italic* text.

- List item 1
- List item 2

| A | B |
|---|---|
| 1 | 2 |

\`\`\`js
const x = 1;
\`\`\`

[Link](https://example.com)`;
            render(<MarkdownViewer content={content} />);
            expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument();
            expect(screen.getByText('Bold')).toBeInTheDocument();
            expect(screen.getByText('italic')).toBeInTheDocument();
            expect(screen.getByText('Link')).toBeInTheDocument();
        });
    });
});
