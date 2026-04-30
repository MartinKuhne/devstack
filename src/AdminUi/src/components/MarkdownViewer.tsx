import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import type { Components } from 'react-markdown';

interface MarkdownViewerProps {
    content?: string | null;
    className?: string;
}

function ExternalLink({ href, children, ...props }: { href?: string; children: React.ReactNode; [key: string]: unknown }) {
    if (!href || href.startsWith('/') || href.startsWith('#') || href.startsWith('mailto:')) {
        return <a href={href} {...props}>{children}</a>;
    }

    return (
        <a href={href} target="_blank" rel="noopener noreferrer" {...props}>
            {children}
        </a>
    );
}

const components: Components = {
    a: props => {
        return <ExternalLink href={props.href}>{props.children}</ExternalLink>;
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
