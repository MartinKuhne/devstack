import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import 'highlight.js/styles/github-dark.css';
import { Link } from 'react-router-dom';
import type { ComponentProps } from 'react';
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const rehypeHighlightPlugin = rehypeHighlight as any;

interface MarkdownRendererProps {
    content: string;
    className?: string;
}

export function MarkdownRenderer({ content, className }: MarkdownRendererProps) {
    const handleExternalLink = (href: string) => {
        try {
            const url = new URL(href);
            return url.protocol === 'http:' || url.protocol === 'https:';
        } catch {
            return false;
        }
    };

    const linkComponent = ({ href, children, ...props }: ComponentProps<'a'>) => {
        const isExternal = href && handleExternalLink(href);
        
        if (isExternal) {
            return (
                <a
                    href={href}
                    target="_blank"
                    rel="noopener noreferrer"
                    {...props}
                >
                    {children}
                </a>
            );
        }

        return href ? (
            <Link to={href} {...props}>
                {children}
            </Link>
        ) : (
            <a {...props}>{children}</a>
        );
    };

    return (
        <div className={`prose prose-slate dark:prose-invert max-w-none ${className ?? ''}`}>
            <ReactMarkdown
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeHighlightPlugin]}
                components={{
                    a: linkComponent,
                }}
            >
                {content}
            </ReactMarkdown>
        </div>
    );
}
