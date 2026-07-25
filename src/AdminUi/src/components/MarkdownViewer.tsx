import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import type { Components } from 'react-markdown';
import DOMPurify from 'dompurify';
import { ExternalLink } from 'lucide-react';

interface MarkdownViewerProps {
    content?: string | null;
    className?: string;
}

function LinkRenderer({ href, children, ...props }: { href?: string; children: React.ReactNode; [key: string]: unknown }) {
    const safeHref = href && !href.trim().startsWith('//') ? DOMPurify.sanitize(href) : '';
    if (!safeHref) {
        return <span>{children}</span>;
    }

    if (safeHref.startsWith('/') || safeHref.startsWith('#') || safeHref.startsWith('mailto:')) {
        return <a href={safeHref} {...props}>{children}</a>;
    }

    return (
        <a href={safeHref} target="_blank" rel="noopener noreferrer" {...props} className="inline-flex items-center gap-1">
            {children}
            <ExternalLink className="h-3 w-3 text-muted-foreground" />
        </a>
    );
}

const components: Components = {
    a: props => {
        return <LinkRenderer href={props.href}>{props.children}</LinkRenderer>;
    },
};

export function MarkdownViewer({ content, className }: MarkdownViewerProps) {
    if (!content) {
        return null;
    }

    return (
        <div className={className}>
            <ReactMarkdown remarkPlugins={[remarkGfm]} rehypePlugins={[rehypeHighlight]} components={components}>
                {content}
            </ReactMarkdown>
        </div>
    );
}
