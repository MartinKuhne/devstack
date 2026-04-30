import { describe, it, expect } from 'vitest';
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

    describe('task lists', () => {
        it('renders completed task item', () => {
            render(<MarkdownViewer content="- [x] Completed task" />);
            expect(screen.getByText('Completed task')).toBeInTheDocument();
        });

        it('renders incomplete task item', () => {
            render(<MarkdownViewer content="- [ ] Incomplete task" />);
            expect(screen.getByText('Incomplete task')).toBeInTheDocument();
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
    });

    describe('code blocks', () => {
        it('renders inline code', () => {
            render(<MarkdownViewer content="Use `console.log` to debug" />);
            expect(screen.getByText('console.log')).toBeInTheDocument();
        });

        it('renders fenced code block', () => {
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
        it('applies className to container', () => {
            const { container } = render(<MarkdownViewer content="Hello" className="custom-class" />);
            expect(container.firstChild).toHaveClass('custom-class');
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
