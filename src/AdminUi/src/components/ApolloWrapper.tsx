import { type FC, type ReactNode } from 'react';
import { ApolloProvider } from '@apollo/client/react';
import { getApolloClient } from '@/hooks/useApolloClient';

interface ApolloProviderProps {
    children: ReactNode;
}

export const ApolloWrapper: FC<ApolloProviderProps> = ({ children }) => {
    const client = getApolloClient();
    return <ApolloProvider client={client}>{children}</ApolloProvider>;
};
