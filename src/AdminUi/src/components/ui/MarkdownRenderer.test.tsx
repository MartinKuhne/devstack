import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MarkdownRenderer } from '@/components/ui/MarkdownRenderer';

describe('MarkdownRenderer', () => {
    it('renders basic markdown text', () => {
        const markdown = '# Hello World';
        render(<MarkdownRenderer content={markdown} />);
        
        expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Hello World');
    });

    it('renders paragraphs', () => {
        const markdown = 'This is a paragraph.\n\nThis is another paragraph.';
        render(<MarkdownRenderer content={markdown} />);
        
        const paragraphs = screen.getAllByRole('paragraph');
        expect(paragraphs).toHaveLength(2);
        expect(paragraphs[0]).toHaveTextContent('This is a paragraph.');
        expect(paragraphs[1]).toHaveTextContent('This is another paragraph.');
    });

    it('renders unordered lists with GFM support', () => {
        const markdown = '- Item 1\n- Item 2\n- Item 3';
        render(<MarkdownRenderer content={markdown} />);
        
        const listItems = screen.getAllByRole('listitem');
        expect(listItems).toHaveLength(3);
        expect(listItems[0]).toHaveTextContent('Item 1');
    });

    it('renders ordered lists', () => {
        const markdown = '1. First\n2. Second\n3. Third';
        render(<MarkdownRenderer content={markdown} />);
        
        const listItems = screen.getAllByRole('listitem');
        expect(listItems).toHaveLength(3);
    });

    it('renders code blocks', () => {
        const markdown = '```\ncode here\n```';
        render(<MarkdownRenderer content={markdown} />);
        
        expect(screen.getByRole('code')).toBeInTheDocument();
    });

    it('renders inline code', () => {
        const markdown = 'Use `console.log` for debugging';
        render(<MarkdownRenderer content={markdown} />);
        
        expect(screen.getByRole('code')).toHaveTextContent('console.log');
    });

    it('renders links', () => {
        const markdown = '[Click here](https://example.com)';
        render(<MarkdownRenderer content={markdown} />);
        
        const link = screen.getByRole('link', { name: 'Click here' });
        expect(link).toHaveAttribute('href', 'https://example.com');
    });

    it('renders external links with correct attributes', () => {
        const markdown = '[External Link](https://github.com)';
        render(<MarkdownRenderer content={markdown} />);
        
        const link = screen.getByRole('link', { name: 'External Link' });
        expect(link).toHaveAttribute('target', '_blank');
        expect(link).toHaveAttribute('rel', 'noopener noreferrer');
    });

    it('renders tables with GFM support', () => {
        const markdown = '| Header 1 | Header 2 |\n| -------- | -------- |\n| Cell 1   | Cell 2   |';
        render(<MarkdownRenderer content={markdown} />);
        
        expect(screen.getByRole('table')).toBeInTheDocument();
    });

    it('renders bold and italic text', () => {
        const markdown = '**bold** and *italic* text';
        render(<MarkdownRenderer content={markdown} />);
        
        expect(screen.getByText('bold').closest('strong')).toBeInTheDocument();
        expect(screen.getByText('italic').closest('em')).toBeInTheDocument();
    });

    it('handles empty content gracefully', () => {
        const markdown = '';
        const { container } = render(<MarkdownRenderer content={markdown} />);
        
        expect(container.firstChild).toBeInTheDocument();
    });

    it('applies the prose class for Tailwind typography', () => {
        const markdown = 'Test content';
        render(<MarkdownRenderer content={markdown} className="test-class" />);
        
        const container = screen.getByText('Test content').closest('[class*="prose"]');
        expect(container).toBeInTheDocument();
        expect(container).toHaveClass('test-class');
    });
});
