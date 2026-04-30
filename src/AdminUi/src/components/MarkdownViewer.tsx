import ReactMarkdown from 'react-markdown';

interface MarkdownViewerProps {
    content?: string | null;
    className?: string;
}

export function MarkdownViewer({ content, className }: MarkdownViewerProps) {
    if (!content) {
        return null;
    }

    return (
        <div className={className}>
            <ReactMarkdown>{content}</ReactMarkdown>
        </div>
    );
}
