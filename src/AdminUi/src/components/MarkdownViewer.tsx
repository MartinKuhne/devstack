import { useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import type { Components } from 'react-markdown';
import DOMPurify from 'dompurify';
import { ExternalLink } from 'lucide-react';

const defaultClassName = 'prose prose-sm dark:prose-invert max-w-none text-foreground';

const remarkPlugins = [remarkGfm];
const rehypePlugins = [rehypeHighlight];

interface MarkdownViewerProps {
    content?: string | null;
    className?: string;
}

function LinkRenderer({
    href,
    title,
    children,
    ...rest
}: {
    href?: string;
    title?: string;
    children?: React.ReactNode;
    [k: string]: unknown;
}) {
    const safeHref =
        href && !href.trim().startsWith('//') ? DOMPurify.sanitize(href) : '';
    if (!safeHref) {
        return <span>{children}</span>;
    }

    const isInternal =
        safeHref.startsWith('/') ||
        safeHref.startsWith('#') ||
        safeHref.startsWith('mailto:');

    if (isInternal) {
        return (
            <a href={safeHref} title={title} {...rest}>
                {children}
            </a>
        );
    }

    return (
        <a
            href={safeHref}
            title={title}
            target="_blank"
            rel="noopener noreferrer"
            {...rest}
            className="inline-flex items-center gap-1"
        >
            {children}
            <ExternalLink
                aria-hidden="true"
                className="h-3 w-3 text-muted-foreground"
            />
            <span className="sr-only"> (opens in new tab)</span>
        </a>
    );
}

const components: Components = {
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    a: ({ node, ...props }) => <LinkRenderer {...props} />,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    img: ({ node, src, alt, ...rest }) => (
        <img
            src={src}
            alt={alt ?? ''}
            loading="lazy"
            decoding="async"
            className="max-w-full h-auto rounded"
            {...rest}
        />
    ),
};

export function MarkdownViewer({ content, className }: MarkdownViewerProps) {
    const memoizedRemarkPlugins = useMemo(() => remarkPlugins, []);
    const memoizedRehypePlugins = useMemo(() => rehypePlugins, []);

    if (!content) {
        return null;
    }

    return (
        <div className={className ?? defaultClassName}>
            <ReactMarkdown
                remarkPlugins={memoizedRemarkPlugins}
                rehypePlugins={memoizedRehypePlugins}
                components={components}
            >
                {content}
            </ReactMarkdown>
        </div>
    );
}
