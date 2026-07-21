import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import type { Components } from 'react-markdown';
import { ExternalLink } from 'lucide-react';

interface MarkdownViewerProps {
    content?: string | null;
    className?: string;
}

function LinkRenderer({ href, children, ...props }: { href?: string; children: React.ReactNode; [key: string]: unknown }) {
    if (!href || href.startsWith('/') || href.startsWith('#') || href.startsWith('mailto:')) {
        return <a href={href} {...props}>{children}</a>;
    }

    return (
        <a href={href} target="_blank" rel="noopener noreferrer" {...props} className="inline-flex items-center gap-1">
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
