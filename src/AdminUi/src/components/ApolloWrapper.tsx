import { type FC, type ReactNode } from 'react';
import { getApolloClient } from '@/hooks/useApolloClient';

interface ApolloProviderProps {
    children: ReactNode;
}

export const ApolloWrapper: FC<ApolloProviderProps> = ({ children }) => {
    getApolloClient();
    return <div>{children}</div>;
};
